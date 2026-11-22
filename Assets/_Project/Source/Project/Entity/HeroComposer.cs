using System;
using ArtificeToolkit.Attributes;
using Com.Voobox.Framework.Events;
using Com.Voobox.Project.Component;
using Com.Voobox.Project.Data;
using Com.Voobox.Project.Events.HeroEvents;
using Com.Voobox.Project.Others;
using Cysharp.Threading.Tasks;
using FMODUnity;
using Unity.Cinemachine;
using UnityEngine;

namespace Com.Voobox.Project.Entity
{
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(Animator))]
    [RequireComponent(typeof(SpriteRenderer), typeof(StudioEventEmitter))]
    public class HeroComposer : MonoBehaviour
    {
        private const int HURT_OVERLAP_BUFFER_SIZE = 16;

        [SerializeField]
        [InlineObject]
        private HeroDataScriptableObject m_heroData;

        [Header("Attack Setup")]
        [SerializeField] private AttackArea m_attackUpArea;
        [SerializeField] private AttackArea m_attackDownArea;
        [SerializeField] private AttackArea m_attackLeftArea;
        [SerializeField] private AttackArea m_attackRightArea;

        [Header("Hurtbox Setup")]
        [SerializeField] private Collider2D m_hurtboxCollider;

        [Header("ScreenShake Setup")]
        [SerializeField] private CinemachinePositionComposer m_positionComposer;

        [Header("Settings Particles")]//TODO
        [SerializeField] private ParticleSystem m_runParticles;
        [SerializeField] private float m_runParticleInterval = 0.2f;

        [Header("Settings Jump/Land Particles")]//TODO
        [SerializeField] private ParticleSystem m_jumpParticles;
        [SerializeField] private ParticleSystem m_landParticles;

        private Movement2DComponent m_movement2DComponent;
        private Jump2DComponent m_jump2DComponent;
        private DasherComponent m_dasherComponent;
        private AttackerComponent m_attackerComponent;
        private HeroAnimatorComponent m_heroAnimatorComponent;
        private FrameFreezerComponent m_frameFreezerComponent;
        private ScreenShakerComponent m_screenShakerComponent;
        private HealthComponent m_healthComponent;

        private IHeroController m_controller;
        private HeroInputController m_defaultController;
        private HeroIntent m_currentIntent = HeroIntent.Default;
        private Collider2D m_boxCollider;
        private Rigidbody2D m_rigidbody2D;
        private StudioEventEmitter m_sfxRunEmitter;
        private readonly Collider2D[] m_hurtOverlapBuffer = new Collider2D[HURT_OVERLAP_BUFFER_SIZE];
        private ContactFilter2D m_hurtContactFilter;

        private HeroData Data => m_heroData.HeroData;

        private float m_runParticleTimer = 0f;

        private bool m_isFacingRight = true;
        private bool m_isGrounded = true;
        private bool m_isAttacking = false;
        private bool m_isRecoiling = false;
        private bool m_isHitStoping = false;
        private bool m_isGhosting = false;

        private bool m_isJumping = false;
        private bool m_hasJump = true;
        private bool m_hasDash = true;

        private bool m_isDashing = false;

        private void Awake()
        {
            m_rigidbody2D = GetComponent<Rigidbody2D>();
            m_boxCollider = GetComponent<BoxCollider2D>();
            var animator = GetComponent<Animator>();
            var renderer = GetComponent<SpriteRenderer>();
            m_sfxRunEmitter = GetComponent<StudioEventEmitter>();

            m_hurtContactFilter = new ContactFilter2D
            {
                useTriggers = true,
                useLayerMask = true
            };

            if (m_controller == null)
            {
                m_defaultController = new HeroInputController();
                m_controller = m_defaultController;
            }

            m_heroData = Instantiate(m_heroData);
            Data.AttackUp = m_attackUpArea;
            Data.AttackDown = m_attackDownArea;
            Data.AttackLeft = m_attackLeftArea;
            Data.AttackRight = m_attackRightArea;
            Data.ImpulseSource = GetComponent<CinemachineImpulseSource>();
            Data.Rigidbody2D = m_rigidbody2D;
            Data.PositionComposer = m_positionComposer;

            m_movement2DComponent = new Movement2DComponent(Data);
            m_jump2DComponent = new Jump2DComponent(Data);
            m_dasherComponent = new DasherComponent(Data);
            m_attackerComponent = new AttackerComponent(Data);
            m_heroAnimatorComponent = new HeroAnimatorComponent(animator, renderer);//TODO
            m_frameFreezerComponent = new FrameFreezerComponent();//TODO
            m_screenShakerComponent = (m_positionComposer != null || Data.ImpulseSource != null)
                ? new ScreenShakerComponent(Data)
                : null;
            m_healthComponent = new HealthComponent(Data);
        }

        private void Start()
        {
            if (m_attackRightArea != null)
                m_attackRightArea.OnHitSuccess += HandleRightAttackHit;
            if (m_attackLeftArea != null)
                m_attackLeftArea.OnHitSuccess += HandleLeftAttackHit;
            if (m_attackDownArea != null)
                m_attackDownArea.OnHitSuccess += HandleDownAttackHit;
            if (m_attackUpArea != null)
                m_attackUpArea.OnHitSuccess += HandleUpAttackHit;
        }

        private void OnDestroy()
        {
            if (m_attackRightArea != null)
                m_attackRightArea.OnHitSuccess -= HandleRightAttackHit;
            if (m_attackLeftArea != null)
                m_attackLeftArea.OnHitSuccess -= HandleLeftAttackHit;
            if (m_attackDownArea != null)
                m_attackDownArea.OnHitSuccess -= HandleDownAttackHit;
            if (m_attackUpArea != null)
                m_attackUpArea.OnHitSuccess -= HandleUpAttackHit;

            m_defaultController?.Dispose();
        }

        /// <summary>
        /// Replaces the active <see cref="IHeroController"/> driving this entity.
        /// </summary>
        public void SetController(IHeroController controller)
        {
            if (ReferenceEquals(m_controller, controller)) return;

            if (isActiveAndEnabled)
                m_controller?.Disable();

            m_controller = controller;

            if (isActiveAndEnabled)
                m_controller?.Enable();
        }

        private void OnEnable() => m_controller?.Enable();
        private void OnDisable() => m_controller?.Disable();

        private void Update()
        {
            if (m_controller != null)
            {
                HeroBodyState bodyState = BuildBodyState();
                m_currentIntent = m_controller.SampleIntent(in bodyState, Time.deltaTime);
            }
            else
            {
                m_currentIntent = HeroIntent.Default;
            }

            if (m_currentIntent.JumpPressed)
                TryJump();

            if (m_currentIntent.JumpReleased)
                JumpCut();

            if (m_currentIntent.DashPressed)
                Dash().Forget();

            if (m_currentIntent.AttackPressed)
                HandleAttack().Forget();

            UpdateAnimations();
            HandleFlip();
            HandleRunParticles();
        }

        private void FixedUpdate()
        {
            CheckGround();

            if (m_screenShakerComponent != null && m_positionComposer != null)
                m_screenShakerComponent.StabilizeTargetOffset();

            Run();
        }

        private HeroBodyState BuildBodyState()
        {
            return new HeroBodyState
            {
                Position = transform.position,
                IsGrounded = m_isGrounded,
                IsAttacking = m_isAttacking,
                IsDashing = m_isDashing,
                CanAttack = CanAttack(),
                CanDash = m_hasDash,
                IsDead = m_healthComponent != null && m_healthComponent.IsDead,
                HealthNormalized = m_healthComponent != null ? m_healthComponent.HealthNormalized : 0f
            };
        }

        private void CheckGround()
        {
            bool wasGrounded = m_isGrounded;
            RaycastHit2D hit = Physics2D.BoxCast(m_boxCollider.bounds.center, m_boxCollider.bounds.size, 0f, Vector2.down, 0.1f, Data.GroundLayer);
            m_isGrounded = hit.collider != null && m_rigidbody2D.linearVelocity.y <= 0.01f;

            if (!m_isGrounded) return;
            m_hasJump = true;
            m_isJumping = false;

            if (m_jump2DComponent.IsJumpBufferActive)
                Jump();

            if (wasGrounded) return;

            if (m_landParticles != null)
            {
                m_landParticles.Play();
            }
        }

        private void UpdateAnimations()
        {
            m_heroAnimatorComponent.SetSpeed(Mathf.Abs(m_currentIntent.MoveAxis.x));
            m_heroAnimatorComponent.SetGrounded(m_isGrounded);
            m_heroAnimatorComponent.SetDash(m_isDashing);
            m_heroAnimatorComponent.SetAirVelocity(m_rigidbody2D.linearVelocity.y);
        }

        private void HandleFlip()
        {
            if (m_isDashing || m_currentIntent.MoveAxis.x == 0f) return;

            m_isFacingRight = m_currentIntent.MoveAxis.x > 0f;
            m_heroAnimatorComponent.FlipX(!m_isFacingRight);

            if (m_screenShakerComponent != null && m_positionComposer != null)
                m_screenShakerComponent.FlipTargetOffsetX(!m_isFacingRight);
        }

        private static bool MatchesLayerInHierarchy(Collider2D collision, LayerMask layerMask)
        {
            if (collision == null) return false;

            int mask = layerMask.value;
            if ((mask & (1 << collision.gameObject.layer)) != 0) return true;

            Rigidbody2D attachedBody = collision.attachedRigidbody;
            if (attachedBody != null && (mask & (1 << attachedBody.gameObject.layer)) != 0) return true;

            Transform parent = collision.transform.parent;
            while (parent != null)
            {
                if ((mask & (1 << parent.gameObject.layer)) != 0) return true;
                parent = parent.parent;
            }

            return false;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision == null || m_healthComponent == null || m_healthComponent.IsDead) return;
            if (m_isGhosting) return;
            if (collision.transform == transform || collision.transform.IsChildOf(transform)) return;
            if (m_hurtboxCollider == null || !m_hurtboxCollider.IsTouching(collision)) return;
            if (!MatchesLayerInHierarchy(collision, Data.HurtLayer)) return;

            TakeDamage(GetKnockbackDirection(collision));
        }

        private void TakeDamage(int direction)
        {
            m_healthComponent.TakeDamage();

            if (m_healthComponent.IsDead)
            {
                Die();
                return;
            }

            GhostTimeTask().Forget();
            ApplyRecoilAsync(2 * direction).Forget();
        }

        private void Die()
        {
            EventBus.Notify(new HeroDiedEvent());
        }

        private async UniTask GhostTimeTask()
        {
            m_isGhosting = true;
            m_heroAnimatorComponent.SetGhost(true);
            await UniTask.Delay(TimeSpan.FromSeconds(3));
            m_heroAnimatorComponent.SetGhost(false);
            m_isGhosting = false;

            CheckDamage();
        }

        private async UniTask ApplyRecoilAsync(float direction)
        {
            m_isRecoiling = true;
            await m_movement2DComponent.Recoil(direction);
            m_isRecoiling = false;
        }

        private int GetKnockbackDirection(Collider2D other)
        {
            if (other == null) return 1;
            return other.transform.position.x > transform.position.x ? -1 : 1;
        }

        private void CheckDamage()
        {
            if (m_healthComponent == null || m_healthComponent.IsDead || m_hurtboxCollider == null) return;

            m_hurtContactFilter.useTriggers = true;
            m_hurtContactFilter.useLayerMask = true;
            m_hurtContactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(m_hurtboxCollider.gameObject.layer));

            int hitCount = m_hurtboxCollider.Overlap(m_hurtContactFilter, m_hurtOverlapBuffer);
            Collider2D matchedEnemy = null;
            for (int i = 0; i < hitCount; i++)
            {
                Collider2D candidate = m_hurtOverlapBuffer[i];
                m_hurtOverlapBuffer[i] = null;

                if (matchedEnemy != null || candidate == null) continue;
                if (candidate.transform == transform || candidate.transform.IsChildOf(transform)) continue;
                if (MatchesLayerInHierarchy(candidate, Data.HurtLayer))
                {
                    matchedEnemy = candidate;
                }
            }

            if (matchedEnemy == null) return;

            TakeDamage(GetKnockbackDirection(matchedEnemy));
        }

        #region Movement Behaviour

        private void HandleRunParticles()
        {
            bool isRunning = m_isGrounded && Mathf.Abs(m_currentIntent.MoveAxis.x) > 0.05f && CanRun();

            if (isRunning)
            {
                m_runParticleTimer -= Time.deltaTime;

                if (!(m_runParticleTimer <= 0f)) return;

                if (m_runParticles != null)
                    m_runParticles.Emit(1);

                m_runParticleTimer = m_runParticleInterval;
            }
            else
            {
                m_runParticleTimer = 0f;
            }
        }

        private void Run()
        {
            if (!CanRun()) return;

            float moveX = m_currentIntent.MoveAxis.x * m_currentIntent.SpeedMultiplier;
            m_movement2DComponent.Run(moveX);

            int value = 0;
            if (m_isGrounded && moveX != 0f)
                value = 1;

            if (m_sfxRunEmitter != null && !m_sfxRunEmitter.EventReference.IsNull)
                m_sfxRunEmitter.SetParameter("run_velocity", value);
        }

        private bool CanRun()
        {
            return !m_isDashing && !m_isRecoiling;
        }

        private void TryJump()
        {
            if (!m_hasJump)
            {
                m_jump2DComponent.StartJumpBuffer().Forget();
                return;
            }

            Jump();
        }

        private void Jump()
        {
            m_hasJump = false;
            m_isJumping = true;
            m_jump2DComponent.Jump();

            if (m_jumpParticles != null)
                m_jumpParticles.Play();
        }

        private void JumpCut()
        {
            if (m_isJumping && m_rigidbody2D.linearVelocity.y > 0)
                m_jump2DComponent.JumpCut();

            m_isJumping = false;
        }

        private async UniTask PogoJump()
        {
            await DoHitStop();
            m_hasJump = true;
            m_isJumping = false;
            m_jump2DComponent.PogoJump();

            if (m_screenShakerComponent != null && Data.ImpulseSource != null)
                m_screenShakerComponent.GenerateImpulse();
        }

        #endregion

        #region Dash Behaviour

        private async UniTaskVoid Dash()
        {
            if (!m_hasDash) return;
            m_attackerComponent.StopAttack();

            m_hasDash = false;
            m_isDashing = true;

            await m_dasherComponent.Dash(m_isFacingRight ? 1 : -1);
            
            m_isDashing = false;
            
            await m_dasherComponent.DashCooldown();
            
            m_hasDash = true;
        }

        #endregion

        #region Attack Behaviour

        private async UniTaskVoid HandleAttack()
        {
            if(!CanAttack()) return;

            m_isAttacking = true;
            switch (m_currentIntent.MoveAxis.y)
            {
                case > 0.75f:
                    if (m_attackUpArea != null)
                    {
                        m_heroAnimatorComponent.SetAttackUp(true);
                        await m_attackerComponent.AttackUp();
                    }
                    break;
                case < -0.75f when !m_isGrounded:
                    if (m_attackDownArea != null)
                    {
                        m_heroAnimatorComponent.SetAttackDown(true);
                        await m_attackerComponent.AttackDown();
                    }
                    break;
                default:
                {
                    if (m_isFacingRight && m_attackRightArea != null)
                    {
                        m_heroAnimatorComponent.SetAttackFront(true);
                        await m_attackerComponent.AttackRight();
                    }
                    else if (!m_isFacingRight && m_attackLeftArea != null)
                    {
                        m_heroAnimatorComponent.SetAttackFront(true);
                        await m_attackerComponent.AttackLeft();
                    }
                    break;
                }
            }

            m_heroAnimatorComponent.SetAllAttacks(false);
            m_isAttacking = false;
        }

        private void HandleRightAttackHit() => Recoil(false).Forget();
        private void HandleLeftAttackHit() => Recoil(true).Forget();
        private void HandleDownAttackHit() => PogoJump().Forget();

        private void HandleUpAttackHit()
        {
            if (m_screenShakerComponent != null && Data.ImpulseSource != null)
                m_screenShakerComponent.ShakeForce(-1f);

            DoHitStop().Forget();

            if (!m_isGrounded)
                m_movement2DComponent.StopVerticalVelocity();
        }

        private async UniTaskVoid Recoil(bool isDirectionRight)
        {
            if (m_isRecoiling) return;

            await DoHitStop();

            float recoilDirection = isDirectionRight ? 1f : -1f;

            if (m_screenShakerComponent != null && Data.ImpulseSource != null)
                m_screenShakerComponent.ShakeDirection(-recoilDirection);

            await ApplyRecoilAsync(recoilDirection);
        }

        private async UniTask DoHitStop(float duration = 0.05f, float scale = 0.05f)
        {
            if(m_isHitStoping) return;

            m_isHitStoping = true;
            await m_frameFreezerComponent.Freeze(duration, scale);
            m_isHitStoping = false;
        }

        private bool CanAttack()
        {
            return !m_isDashing && !m_isAttacking;
        }

        #endregion
    }
}