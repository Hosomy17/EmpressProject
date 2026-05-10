using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class Hero : MonoBehaviour
{
    [SerializeField] private float m_jumpForce;
    [SerializeField] private LayerMask m_groundLayer;
    [SerializeField] private float m_speed;
    
    [Header("Settings Dash")]
    [SerializeField] private float m_dashForce;
    [SerializeField] private float m_dashDuration;
    [SerializeField] private float m_dashCooldown;
    
    [Header("Settings Attack")]
    [SerializeField] private AttackArea m_atkRightPoint;
    [SerializeField] private AttackArea m_atkLeftPoint;
    [SerializeField] private AttackArea m_atkUpPoint;
    [SerializeField] private AttackArea m_atkDownPoint;
    [SerializeField] private float m_atkTime;
    
    [Header("Settings Recoil")]
    [SerializeField] private float m_recoilForce;
    [SerializeField] private float m_recoilDuration;
    
    private InputSystemActions m_inputActions;
    private Vector2 m_moveInput;
    
    private Rigidbody2D m_rigidbody2D;
    private Collider2D m_boxCollider;
    private Animator m_animator;
    private SpriteRenderer m_renderer;
    
    private bool m_isFacingRight = true;
    private bool m_isGrounded = true;
    private bool m_isAttacking = false;
    private bool m_isRecoiling = false;
    
    private bool m_hasJump = true;
    private bool m_hasDash = true;
    
    private bool m_isDashing = false;

    private TaskHandle m_taskDash = new();
    private TaskHandle m_taskDashCooldown = new();
    private TaskHandle m_taskAttack = new();

    private void Awake()
    {
        m_inputActions = new InputSystemActions();
        m_inputActions.Player.Move.performed += ctx => m_moveInput = ctx.ReadValue<Vector2>();
        m_inputActions.Player.Move.canceled += _ => m_moveInput = Vector2.zero;
        m_inputActions.Player.Jump.performed += Jump;
        m_inputActions.Player.Dash.performed += _ => Dash().Forget();
        m_inputActions.Player.Attack.performed += _ => HandleAttack().Forget();
        
        m_rigidbody2D = GetComponent<Rigidbody2D>();
        m_boxCollider = GetComponent<BoxCollider2D>();
        m_animator = GetComponent<Animator>();
        m_renderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        m_atkRightPoint.OnHitSuccess += () => Recoil(false);
        m_atkLeftPoint.OnHitSuccess += () => Recoil(true);
        m_atkDownPoint.OnHitSuccess += PogoJump;
    }

    private void OnEnable() => m_inputActions.Enable();
    private void OnDisable() => m_inputActions.Disable();

    private void Update()
    {
        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        RaycastHit2D hit = Physics2D.BoxCast(m_boxCollider.bounds.center, m_boxCollider.bounds.size, 0f, Vector2.down, 0.1f, m_groundLayer);
        m_isGrounded = hit.collider != null && m_rigidbody2D.linearVelocity.y <= 0.01f;
        if (m_isGrounded)
            m_hasJump = true;

        Run();
    }
    
    private void UpdateAnimations()
    {
        m_animator.SetFloat("Speed", Mathf.Abs(m_moveInput.x));
        m_animator.SetBool("Grounded", m_isGrounded);
        m_animator.SetBool("Dash", m_isDashing);
        m_animator.SetFloat("AirVelocity", m_rigidbody2D.linearVelocity.y);
        HandleFlip();
    }
    
    private void HandleFlip()
    {
        if (m_isDashing) return;
        
        if (m_moveInput.x > 0)
        {
            m_isFacingRight = true;
            m_renderer.flipX = false;
        }
        else if (m_moveInput.x < 0)
        {
            m_isFacingRight = false;
            m_renderer.flipX = true;
        }
    }

    #region Run Behaviour

    private void Run()
        {
            if (!CanRun()) return;
            
            var speed = m_moveInput.x * m_speed;
            m_rigidbody2D.linearVelocity = new Vector2(speed, m_rigidbody2D.linearVelocity.y);
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
            m_rigidbody2D.linearVelocity = new Vector2(m_rigidbody2D.linearVelocity.x, 0);
            m_rigidbody2D.AddForce(Vector2.up * m_jumpForce, ForceMode2D.Impulse);
        }
    
        private void PogoJump()
        {
            m_taskAttack.Stop();
            m_hasJump = true;
            m_rigidbody2D.linearVelocity = new Vector2(m_rigidbody2D.linearVelocity.x, 0);
            m_rigidbody2D.AddForce(Vector2.up * m_jumpForce, ForceMode2D.Impulse);
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

        var originalGravity = m_rigidbody2D.gravityScale;
        m_rigidbody2D.gravityScale = 0f;

        var dashDir = m_isFacingRight ? 1 : -1;
        m_rigidbody2D.linearVelocity = new Vector2(dashDir * m_dashForce, 0f);

        await UniTask.Delay(TimeSpan.FromSeconds(m_dashDuration), cancellationToken: m_taskDash.GetNewToken()).SuppressCancellationThrow();

        m_rigidbody2D.gravityScale = originalGravity;
        m_isDashing = false;

        await UniTask.Delay(TimeSpan.FromSeconds(m_dashCooldown), cancellationToken: m_taskDashCooldown.GetNewToken()).SuppressCancellationThrow();
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
        var attackKeyAnimation = "AttackFront";

        switch (m_moveInput.y)
        {
            case > 0.5f:
                attackArea = m_atkUpPoint;
                attackKeyAnimation = "AttackUp";
                break;
            case < -0.5f when !m_isGrounded:
                attackArea = m_atkDownPoint;
                attackKeyAnimation = "AttackDown";
                break;
            default:
            {
                if (m_isFacingRight) attackArea = m_atkRightPoint;
                else attackArea = m_atkLeftPoint;
                break;
            }
        }

        var token = m_taskAttack.GetNewToken();
        attackArea.gameObject.SetActive(true);

        m_animator.SetBool(attackKeyAnimation, true);
        m_isAttacking = true;
        await UniTask.Delay(TimeSpan.FromSeconds(m_atkTime), cancellationToken: token).SuppressCancellationThrow();
        m_isAttacking = false;
        m_animator.SetBool(attackKeyAnimation, false);
        
        Debug.Log("Terminou Attack");
        attackArea.gameObject.SetActive(false);
    }
    
    private async UniTaskVoid Recoil(bool isDirectionRight)
    {
        m_taskAttack.Stop();
        m_isRecoiling = true;
        
        var recoilDir = isDirectionRight ? 1f : -1f;
        
        m_rigidbody2D.linearVelocity = new Vector2(recoilDir * m_recoilForce, m_rigidbody2D.linearVelocity.y);
        await UniTask.Delay(TimeSpan.FromSeconds(m_recoilDuration));

        m_isRecoiling = false;
    }

    private bool CanAttack()
    {
        return !m_isDashing && !m_isAttacking;
    }
    
    #endregion
}
