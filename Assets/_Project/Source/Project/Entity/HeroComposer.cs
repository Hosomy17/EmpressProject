using ArtificeToolkit.Attributes;
using Com.Voobox.Project.Component;
using Com.Voobox.Project.Data;
using Cysharp.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Com.Voobox.Project.Entity
{
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(Animator))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class HeroComposer : MonoBehaviour
    {
        [SerializeField, InlineObject] private HeroDataScriptableObject m_data;
    
        [Header("Settings Attack")]//TODO
        [SerializeField] private AttackArea m_atkRightPoint;
        [SerializeField] private AttackArea m_atkLeftPoint;
        [SerializeField] private AttackArea m_atkUpPoint;
        [SerializeField] private AttackArea m_atkDownPoint;

        [Header("Settings Screen")]//TODO
        [SerializeField] private CinemachinePositionComposer m_positionComposer;
        
        [Header("Settings Particles")]
        [SerializeField] private ParticleSystem m_runParticles;
        [SerializeField] private float m_runParticleInterval = 0.2f;

        [Header("Settings Jump/Land Particles")]
        [SerializeField] private ParticleSystem m_jumpParticles;
        [SerializeField] private ParticleSystem m_landParticles;
        
        private JumperComponent m_jumperComponent;
        private MovementComponent m_movementComponent;
        private DasherComponent m_dasherComponent;
        private AttackerComponent m_attackerComponent;
        private HeroAnimatorComponent m_heroAnimatorComponent;
        
        private FrameFreezerComponent m_frameFreezerComponent;
        private ScreenShakerComponent m_screenShakerComponent;
    
        private InputSystemActions m_inputActions;
        private Vector2 m_moveInput;
        private Rigidbody2D m_rigidbody2D;
        private Collider2D m_boxCollider;

        private float m_runParticleTimer = 0f;
        private bool m_wasGrounded = true;
    
        private bool m_isFacingRight = true;
        private bool m_isGrounded = true;
        private bool m_isAttacking = false;
        private bool m_isRecoiling = false;
        private bool m_isHitStoping = false;
    
        private bool m_isJumping = false;
        private bool m_hasJump = true;
        private bool m_hasDash = true;
    
        private bool m_isDashing = false;

        private void Awake()
        {
            m_inputActions = new InputSystemActions();
            m_inputActions.Player.Move.performed += ctx => m_moveInput = ApplyDeadZone(axis: ctx.ReadValue<Vector2>());
            m_inputActions.Player.Move.canceled += _ => m_moveInput = Vector2.zero;
            m_inputActions.Player.Jump.performed += Jump;
            m_inputActions.Player.Jump.canceled += JumpCut;
            m_inputActions.Player.Dash.performed += _ => Dash().Forget();
            m_inputActions.Player.Attack.performed += _ => HandleAttack().Forget();
        
            m_rigidbody2D = GetComponent<Rigidbody2D>();
            m_boxCollider = GetComponent<BoxCollider2D>();
            var animator = GetComponent<Animator>();
            var renderer = GetComponent<SpriteRenderer>();
            var impulseSource = GetComponent<CinemachineImpulseSource>();

            m_jumperComponent = new JumperComponent(jumpForce: m_data.HeroData.JumpForce, rigidbody2D: m_rigidbody2D);
            m_movementComponent = new MovementComponent(speed: m_data.HeroData.Speed, recoilBackForce: m_data.HeroData.RecoilBackForce, recoilUpForce: m_data.HeroData.RecoilUpForce, recoilDuration: m_data.HeroData.RecoilDuration, rigidbody2D: m_rigidbody2D);
            m_dasherComponent = new DasherComponent(dashForce: m_data.HeroData.DashForce, dashDuration: m_data.HeroData.DashDuration, dashCooldown: m_data.HeroData.DashCooldown, rigidbody2D: m_rigidbody2D);
            m_attackerComponent = new AttackerComponent(atkTime: m_data.HeroData.AtkTime);
            m_heroAnimatorComponent = new HeroAnimatorComponent(animator, renderer);

            m_frameFreezerComponent = new FrameFreezerComponent();
            m_screenShakerComponent = new ScreenShakerComponent(screenShakeDirection: m_data.HeroData.ScreenShakeDirection, screenShakeForce: m_data.HeroData.ScreenShakeForce, smoothSpeed: m_data.HeroData.SmoothSpeed, impulseSource: impulseSource, positionComposer: m_positionComposer, rigidbody2D: m_rigidbody2D);
        }

        private void Start()
        {
            m_atkRightPoint.OnHitSuccess += () => Recoil(false).Forget();
            m_atkLeftPoint.OnHitSuccess += () => Recoil(true).Forget();
            m_atkDownPoint.OnHitSuccess += () => PogoJump().Forget();
            m_atkUpPoint.OnHitSuccess += HandleUpAttackHit;
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
            var hit = Physics2D.BoxCast(m_boxCollider.bounds.center, m_boxCollider.bounds.size, 0f, Vector2.down, 0.1f, m_data.HeroData.GroundLayer);
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

        #region Run Behaviour

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
            
            m_movementComponent.Run(m_moveInput.x);
        }

        private bool CanRun()
        {
            return !m_isDashing && !m_isRecoiling;
        }

        #endregion

        #region Jump Behaviour

        private void Jump(InputAction.CallbackContext callbackContext)
        {
            if (!CanJump()) return;
            
            m_hasJump = false;
            m_isJumping = true;
            m_jumperComponent.Jump();
        
            if (m_jumpParticles != null)
                m_jumpParticles.Play();
        }
    
        private void JumpCut(InputAction.CallbackContext callbackContext)
        {
            if (m_isJumping && m_rigidbody2D.linearVelocity.y > 0)
                m_jumperComponent.JumpCut();
        
            m_isJumping = false;
        }

        private async UniTask PogoJump()
        {
            await DoHitStop();
            m_hasJump = true;
            m_isJumping = false;
            m_jumperComponent.Jump();
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

            m_hasDash = false;
            m_isDashing = true;

            await m_dasherComponent.Dash(m_isFacingRight ? 1 : -1);
            
            m_isDashing = false;
            
            await m_dasherComponent.DashCooldown();
            
            m_hasDash = true;
        }

        private bool CanDash()
        {
            return m_hasDash && !m_isAttacking;
        }
    
        #endregion

        #region Attack Behaviour

        private async UniTaskVoid HandleAttack()
        {
            if(!CanAttack()) return;

            AttackArea attackArea;
            m_isAttacking = true;
            switch (m_moveInput.y)
            {
                case > 0.5f:
                    attackArea = m_atkUpPoint;
                    m_heroAnimatorComponent.SetAttackUp(true);
                    break;
                case < -0.5f when !m_isGrounded:
                    attackArea = m_atkDownPoint;
                    m_heroAnimatorComponent.SetAttackDown(true);
                    break;
                default:
                {
                    attackArea = m_isFacingRight ? m_atkRightPoint : m_atkLeftPoint;
                    m_heroAnimatorComponent.SetAttackFront(true);
                    break;
                }
            }

            await m_attackerComponent.Attack(attackArea);
            m_heroAnimatorComponent.SetAllAttacks(false);
            m_isAttacking = false;
            
        }
    
        private void HandleUpAttackHit()
        {
            m_screenShakerComponent.ShakeForce(-1f);
            DoHitStop().Forget();
        
            if (!m_isGrounded)
                m_movementComponent.StopVerticalVelocity();
        }
    
        private async UniTaskVoid Recoil(bool isDirectionRight)
        {
            if (m_isRecoiling) return;
        
            await DoHitStop();
            m_isRecoiling = true;
        
            var recoilDirection = isDirectionRight ? 1f : -1f;
        
            m_screenShakerComponent.ShakeDirection(-recoilDirection);
            await m_movementComponent.Recoil(recoilDirection);

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