using System.Collections.Generic;
using ArtificeToolkit.Attributes;
using Com.Voobox.Project.AI;
using Com.Voobox.Project.Component;
using Com.Voobox.Project.Data;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Com.Voobox.Project.Entity
{
    /// <summary>
    /// Root entity composer orchestrating enemy movement, dash/lunge, health, trigger detection,
    /// and a swappable <see cref="IEnemyController"/> brain.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public class EnemyComposer : MonoBehaviour
    {
        private const int INITIAL_TRIGGER_TARGETS_CAPACITY = 16;

        [SerializeField, InlineObject] private EnemyDataScriptableObject m_enemyData;
        [SerializeField] private BrainProfileScriptableObject m_brainProfile;
        [SerializeField] private Collider2D m_detectionCollider;
        [SerializeField] private SpriteRenderer m_spriteRenderer;

        private readonly List<Collider2D> m_targetsInZone = new(INITIAL_TRIGGER_TARGETS_CAPACITY);

        private Movement2DComponent m_movement2DComponent;
        private DasherComponent m_dasherComponent;
        private HealthComponent m_healthComponent;
        private StateMachineBrainComponent m_brainComponent;

        private IEnemyController m_controller;
        private EnemyIntent m_currentIntent = EnemyIntent.Default;
        private LayerMask m_detectionTargetLayer;
        private float m_detectionRangeMultiplier = 1f;
        private Rigidbody2D m_rigidbody2D;
        private BoxCollider2D m_boxCollider;

        private bool m_isFacingRight = true;
        private bool m_hasDash = true;
        private bool m_isDashing = false;

        private EnemyData Data => m_enemyData.EnemyData;

        /// <summary>
        /// Gets the active state identifier when driven by a <see cref="StateMachineBrainComponent"/>.
        /// </summary>
        public string CurrentBrainStateId => m_brainComponent?.CurrentStateId ?? string.Empty;

        /// <summary>
        /// Gets the active layer mask used by trigger detection.
        /// </summary>
        public LayerMask DetectionTargetLayer => m_detectionTargetLayer;

        /// <summary>
        /// Gets the current effective detection range after applying any range multiplier.
        /// </summary>
        public float EffectiveDetectionRange => Mathf.Max(0f, Data.DetectionRange * m_detectionRangeMultiplier);

        private void Awake()
        {
            m_rigidbody2D = GetComponent<Rigidbody2D>();
            m_boxCollider = GetComponent<BoxCollider2D>();

            if (m_spriteRenderer == null)
                m_spriteRenderer = GetComponent<SpriteRenderer>();

            if (m_detectionCollider != null)
                m_detectionCollider.isTrigger = true;

            m_enemyData = Instantiate(m_enemyData);
            Data.Rigidbody2D = m_rigidbody2D;

            m_detectionTargetLayer = Data.DetectionTargetLayer;

            m_movement2DComponent = new Movement2DComponent(Data);
            m_dasherComponent = new DasherComponent(Data);
            m_healthComponent = new HealthComponent(Data);

            if (m_controller == null && m_brainProfile != null)
            {
                m_brainComponent = new StateMachineBrainComponent(m_brainProfile);
                m_controller = m_brainComponent;
            }
        }

        private void OnEnable()
        {
            m_controller?.Enable();
        }

        private void OnDisable()
        {
            m_targetsInZone.Clear();
            m_controller?.Disable();
        }

        /// <summary>
        /// Replaces the active <see cref="IEnemyController"/> driving this enemy.
        /// </summary>
        public void SetController(IEnemyController controller)
        {
            if (ReferenceEquals(m_controller, controller)) return;

            if (isActiveAndEnabled)
                m_controller?.Disable();

            m_controller = controller;
            m_brainComponent = controller as StateMachineBrainComponent;

            if (isActiveAndEnabled)
                m_controller?.Enable();
        }

        /// <summary>
        /// Sets the multiplier applied to the enemy's configured detection range.
        /// </summary>
        public void SetDetectionRangeMultiplier(float multiplier)
        {
            m_detectionRangeMultiplier = Mathf.Max(0f, multiplier);
        }

        /// <summary>
        /// Sets the layer mask targeted by trigger detection and prunes non-matching tracked targets.
        /// </summary>
        public void SetDetectionTargetLayer(LayerMask targetLayer)
        {
            m_detectionTargetLayer = targetLayer;

            for (int i = m_targetsInZone.Count - 1; i >= 0; i--)
            {
                Collider2D col = m_targetsInZone[i];
                if (col == null || ((1 << col.gameObject.layer) & targetLayer) == 0)
                    m_targetsInZone.RemoveAt(i);
            }
        }

        private void Update()
        {
            if ((m_healthComponent != null && m_healthComponent.IsDead) || m_controller == null)
            {
                m_currentIntent = EnemyIntent.Default;
                return;
            }

            EnemyBodyState bodyState = BuildBodyState();
            m_currentIntent = m_controller.SampleIntent(in bodyState, Time.deltaTime);

            if (Mathf.Abs(m_currentIntent.MoveAxis.x) > 0.01f && !m_isDashing)
            {
                m_isFacingRight = m_currentIntent.MoveAxis.x > 0f;
                if (m_spriteRenderer != null)
                    m_spriteRenderer.flipX = !m_isFacingRight;
            }

            if (m_currentIntent.DashPressed && m_hasDash && !m_isDashing)
            {
                int facingDir = m_isFacingRight ? 1 : -1;
                DashAsync(facingDir).Forget();
            }
        }

        private void FixedUpdate()
        {
            if ((m_healthComponent != null && m_healthComponent.IsDead) || m_isDashing) return;

            m_movement2DComponent?.Run(m_currentIntent.MoveAxis.x * m_currentIntent.SpeedMultiplier);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision == null || m_healthComponent == null || m_healthComponent.IsDead) return;
            if (collision.transform == transform || collision.transform.IsChildOf(transform)) return;

            int collisionLayerMask = 1 << collision.gameObject.layer;

            if ((collisionLayerMask & Data.HurtLayer) != 0 && m_boxCollider != null && m_boxCollider.IsTouching(collision))
            {
                m_healthComponent.TakeDamage();

                if (m_healthComponent.IsDead)
                {
                    if (m_rigidbody2D != null)
                        m_rigidbody2D.linearVelocity = Vector2.zero;

                    gameObject.SetActive(false);
                    return;
                }
            }

            if ((collisionLayerMask & m_detectionTargetLayer) != 0 && m_detectionCollider != null && m_detectionCollider.IsTouching(collision))
            {
                HandleTargetEntered(collision);
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision == null) return;
            if (m_detectionCollider != null && m_detectionCollider.IsTouching(collision)) return;

            bool wasTracked = false;
            for (int i = m_targetsInZone.Count - 1; i >= 0; i--)
            {
                if (ReferenceEquals(m_targetsInZone[i], collision))
                {
                    m_targetsInZone.RemoveAt(i);
                    wasTracked = true;
                }
            }

            if (wasTracked)
                m_brainComponent?.NotifyTargetExited(collision.transform);
        }

        private EnemyBodyState BuildBodyState()
        {
            bool detected = TryGetDetectedTarget(out Vector2 detectedPos, out float detectedDist);

            return new EnemyBodyState
            {
                Position = transform.position,
                IsGrounded = true,
                IsAttacking = false,
                IsDashing = m_isDashing,
                CanAttack = false,
                CanDash = m_hasDash && !m_isDashing,
                IsDead = m_healthComponent != null && m_healthComponent.IsDead,
                HealthNormalized = m_healthComponent != null ? m_healthComponent.HealthNormalized : 0f,
                HasDetectedTarget = detected,
                DetectedTargetPosition = detectedPos,
                DetectedTargetDistance = detected ? detectedDist : float.PositiveInfinity,
                DetectionRange = EffectiveDetectionRange
            };
        }

        private bool TryGetDetectedTarget(out Vector2 detectedPosition, out float detectedDistance)
        {
            float effectiveRange = EffectiveDetectionRange;

            if (effectiveRange <= 0f || m_targetsInZone.Count == 0)
            {
                detectedPosition = Vector2.zero;
                detectedDistance = float.PositiveInfinity;
                return false;
            }

            Vector2 selfPos = transform.position;
            bool found = false;
            float bestSqrDist = float.PositiveInfinity;
            Vector2 bestPos = Vector2.zero;

            for (int i = m_targetsInZone.Count - 1; i >= 0; i--)
            {
                Collider2D col = m_targetsInZone[i];
                if (col == null || !col.gameObject.activeInHierarchy || ((1 << col.gameObject.layer) & m_detectionTargetLayer) == 0)
                {
                    m_targetsInZone.RemoveAt(i);
                    continue;
                }

                Vector2 hitPos = col.transform.position;
                float sqrDist = (hitPos - selfPos).sqrMagnitude;
                if (sqrDist < bestSqrDist)
                {
                    bestSqrDist = sqrDist;
                    bestPos = hitPos;
                    found = true;
                }
            }

            if (found)
            {
                float dist = Mathf.Sqrt(bestSqrDist);
                if (dist <= effectiveRange)
                {
                    detectedPosition = bestPos;
                    detectedDistance = dist;
                    return true;
                }
            }

            detectedPosition = Vector2.zero;
            detectedDistance = float.PositiveInfinity;
            return false;
        }

        private void HandleTargetEntered(Collider2D collision)
        {
            for (int i = 0; i < m_targetsInZone.Count; i++)
            {
                if (ReferenceEquals(m_targetsInZone[i], collision))
                    return;
            }

            m_targetsInZone.Add(collision);
            m_brainComponent?.NotifyTargetEntered(collision.transform);
        }

        private async UniTaskVoid DashAsync(int facingDir)
        {
            if (!m_hasDash || m_isDashing) return;

            m_hasDash = false;
            m_isDashing = true;

            await m_dasherComponent.Dash(facingDir);

            m_isDashing = false;

            await m_dasherComponent.DashCooldown();

            m_hasDash = true;
        }
    }
}
