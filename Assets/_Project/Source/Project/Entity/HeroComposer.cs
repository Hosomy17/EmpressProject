using System;
using ArtificeToolkit.Attributes;
using Com.Voobox.Project.Component;
using Com.Voobox.Project.Data;
using Com.Voobox.Project.Others;
using Cysharp.Threading.Tasks;
using FMODUnity;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Com.Voobox.Project.Entity
{
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(Animator))]
    [RequireComponent(typeof(SpriteRenderer), typeof(StudioEventEmitter))]
    public class HeroComposer : MonoBehaviour
    {
        [SerializeField]
        [InlineObject]
        private HeroDataScriptableObject m_heroData;

        [Header("Attack Setup")]
        [SerializeField] private AttackArea m_attackUpArea;
        [SerializeField] private AttackArea m_attackDownArea;
        [SerializeField] private AttackArea m_attackLeftArea;
        [SerializeField] private AttackArea m_attackRightArea;

        [Header("ScreenShake Setup")]
        [SerializeField] private CinemachinePositionComposer m_positionComposer;

        [Header("Settings Particles")]//TODO
        [SerializeField] private ParticleSystem m_runParticles;
        [SerializeField] private float m_runParticleInterval = 0.2f;

        [Header("Settings Jump/Land Particles")]//TODO
        [SerializeField] private ParticleSystem m_jumpParticles;
        [SerializeField] private ParticleSystem m_landParticles;

        private Movement2DComponent m_movement2DComponent;
        private DasherComponent m_dasherComponent;
        private AttackerComponent m_attackerComponent;
        private HeroAnimatorComponent m_heroAnimatorComponent;
        private FrameFreezerComponent m_frameFreezerComponent;
        private ScreenShakerComponent m_screenShakerComponent;

        private InputSystemActions m_inputActions;
        private Vector2 m_moveInput;
        private Collider2D m_boxCollider;
        private Rigidbody2D m_rigidbody2D;
        private StudioEventEmitter m_sfxRunEmitter;

        private float m_runParticleTimer = 0f;
        private bool m_wasGrounded = true;

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

            m_inputActions = new InputSystemActions();
            m_inputActions.Player.Move.performed += ctx => m_moveInput = ApplyDeadZone(axis: ctx.ReadValue<Vector2>());
            m_inputActions.Player.Move.canceled += _ => m_moveInput = Vector2.zero;
            m_inputActions.Player.Jump.performed += Jump;
            m_inputActions.Player.Jump.canceled += JumpCut;
            m_inputActions.Player.Dash.performed += _ => Dash().Forget();
            m_inputActions.Player.Attack.performed += _ => HandleAttack().Forget();

            m_heroData = Instantiate(m_heroData);
            m_heroData.HeroData.AttackUp = m_attackUpArea;
            m_heroData.HeroData.AttackDown = m_attackDownArea;
            m_heroData.HeroData.AttackLeft = m_attackLeftArea;
            m_heroData.HeroData.AttackRight = m_attackRightArea;
            m_heroData.HeroData.ImpulseSource = GetComponent<CinemachineImpulseSource>();
            m_heroData.HeroData.Rigidbody2D = m_rigidbody2D;
            m_heroData.HeroData.PositionComposer = m_positionComposer;

            m_movement2DComponent = new Movement2DComponent(m_heroData.HeroData);
            m_dasherComponent = new DasherComponent(m_heroData.HeroData);
            m_attackerComponent = new AttackerComponent(m_heroData.HeroData);
            m_heroAnimatorComponent = new HeroAnimatorComponent(animator, renderer);//TODO
            m_frameFreezerComponent = new FrameFreezerComponent();//TODO
            m_screenShakerComponent = new ScreenShakerComponent(m_heroData.HeroData);
        }

        private void Start()
        {
            m_attackRightArea.OnHitSuccess += () => Recoil(false).Forget();
            m_attackLeftArea.OnHitSuccess += () => Recoil(true).Forget();
            m_attackDownArea.OnHitSuccess += () => PogoJump().Forget();
            m_attackUpArea.OnHitSuccess += HandleUpAttackHit;
        }

        private void OnEnable() => m_inputActions.Enable();
        private void OnDisable() => m_inputActions.Disable();

        private void Update()
        {
            UpdateAnimations();
            HandleFlip();
            HandleRunParticles();
        }

        private void FixedUpdate()
        {
            CheckGround();
            m_screenShakerComponent.StabilizeTargetOffset();
            Run();
        }

        private void CheckGround()
        {
            m_wasGrounded = m_isGrounded;
            var hit = Physics2D.BoxCast(m_boxCollider.bounds.center, m_boxCollider.bounds.size, 0f, Vector2.down, 0.1f, m_heroData.HeroData.GroundLayer);
            m_isGrounded = hit.collider != null && m_rigidbody2D.linearVelocity.y <= 0.01f;

            if (!m_isGrounded) return;
            m_hasJump = true;
            m_isJumping = false;

            if (m_wasGrounded) return;

            if (m_landParticles != null)
            {
                m_landParticles.Play();
            }
        }

        private void UpdateAnimations()
        {
            m_heroAnimatorComponent.SetSpeed(Mathf.Abs(m_moveInput.x));
            m_heroAnimatorComponent.SetGrounded(m_isGrounded);
            m_heroAnimatorComponent.SetDash(m_isDashing);
            m_heroAnimatorComponent.SetAirVelocity(m_rigidbody2D.linearVelocity.y);
        }

        private void HandleFlip()
        {
            if (m_isDashing || m_moveInput.x == 0) return;

            m_isFacingRight = m_moveInput.x > 0;
            m_heroAnimatorComponent.FlipX(!m_isFacingRight);
            m_screenShakerComponent.FlipTargetOffsetX(!m_isFacingRight);
        }

        private static Vector2 ApplyDeadZone(Vector2 axis)//TODO
        {
            var x = Mathf.Abs(axis.x) < 0.2f ? 0 : Mathf.Sign(axis.x);
            var y = Mathf.Abs(axis.y) < 0.2f ? 0 : Mathf.Sign(axis.y);
            return new Vector2(x, y);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (m_isGhosting || ((1 << collision.gameObject.layer) & m_heroData.HeroData.HurtLayer) == 0) return;

            var direction = collision.transform.position.x > transform.position.x ? -1 : 1;
            TakeDamage(direction);
        }

        private void TakeDamage(int direction)
        {
            GhostTimeTask().Forget();
            Knockback(direction).Forget();
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

        private async UniTask Knockback(int direction)
        {
            m_isRecoiling = true;
            await m_movement2DComponent.Recoil(2 * direction);
            m_isRecoiling = false;
        }

        private void CheckDamage()
        {
            var myCollider = GetComponent<Collider2D>();

            if (!myCollider.IsTouchingLayers(m_heroData.HeroData.HurtLayer)) return;

            var enemyCollider = Physics2D.OverlapBox(transform.position, myCollider.bounds.size, 0f, m_heroData.HeroData.HurtLayer);
            var direction = enemyCollider.transform.position.x > transform.position.x ? -1 : 1;
            TakeDamage(direction);
        }

        #region Movement Behaviour

        private void HandleRunParticles()
        {
            var isRunning = m_isGrounded && Mathf.Abs(m_moveInput.x) > 0.05f && CanRun();

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

            m_movement2DComponent.Run(m_moveInput.x);

            var value = 0;
            if (m_isGrounded && m_moveInput.x != 0)
                value = 1;

            m_sfxRunEmitter.SetParameter("run_velocity", value);
        }

        private bool CanRun()
        {
            return !m_isDashing && !m_isRecoiling;
        }

        private void Jump(InputAction.CallbackContext callbackContext)
        {
            if (!CanJump()) return;

            m_hasJump = false;
            m_isJumping = true;
            m_movement2DComponent.Jump();

            if (m_jumpParticles != null)
                m_jumpParticles.Play();
        }

        private void JumpCut(InputAction.CallbackContext callbackContext)
        {
            if (m_isJumping && m_rigidbody2D.linearVelocity.y > 0)
                m_movement2DComponent.JumpCut();

            m_isJumping = false;
        }

        private async UniTask PogoJump()
        {
            await DoHitStop();
            m_hasJump = true;
            m_isJumping = false;
            m_movement2DComponent.PogoJump();
            m_screenShakerComponent.GenerateImpulse();
        }

        private bool CanJump()
        {
            return m_hasJump;
        }

        #endregion

        #region Dash Behaviour

        private async UniTaskVoid Dash()
        {
            if (!CanDash()) return;
            m_attackerComponent.StopAttack();

            m_hasDash = false;
            m_isDashing = true;

            await m_dasherComponent.Dash(m_isFacingRight ? 1 : -1);
            
            m_isDashing = false;
            
            await m_dasherComponent.DashCooldown();
            
            m_hasDash = true;
        }

        private bool CanDash()
        {
            return m_hasDash;
        }

        #endregion

        #region Attack Behaviour

        private async UniTaskVoid HandleAttack()
        {
            if(!CanAttack()) return;

            m_isAttacking = true;
            switch (m_moveInput.y)
            {
                case > 0.75f:
                    m_heroAnimatorComponent.SetAttackUp(true);
                    await m_attackerComponent.AttackUp();
                    break;
                case < -0.75f when !m_isGrounded:
                    m_heroAnimatorComponent.SetAttackDown(true);
                    await m_attackerComponent.AttackDown();
                    break;
                default:
                {
                    m_heroAnimatorComponent.SetAttackFront(true);
                    if (m_isFacingRight)
                        await m_attackerComponent.AttackRight();
                    else
                        await m_attackerComponent.AttackLeft();
                    break;
                }
            }

            m_heroAnimatorComponent.SetAllAttacks(false);
            m_isAttacking = false;
        }

        private void HandleUpAttackHit()
        {
            m_screenShakerComponent.ShakeForce(-1f);
            DoHitStop().Forget();

            if (!m_isGrounded)
                m_movement2DComponent.StopVerticalVelocity();
        }

        private async UniTaskVoid Recoil(bool isDirectionRight)
        {
            if (m_isRecoiling) return;

            await DoHitStop();
            m_isRecoiling = true;

            var recoilDirection = isDirectionRight ? 1f : -1f;

            m_screenShakerComponent.ShakeDirection(-recoilDirection);
            await m_movement2DComponent.Recoil(recoilDirection);

            m_isRecoiling = false;
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