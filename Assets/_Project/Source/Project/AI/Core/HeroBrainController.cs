using Com.Voobox.Project.Data;
using UnityEngine;

namespace Com.Voobox.Project.AI
{
    /// <summary>
    /// Adapts <see cref="StateMachineBrainComponent"/> to <see cref="IHeroController"/> with zero
    /// per-tick heap allocations.
    /// </summary>
    public sealed class HeroBrainController : IHeroController
    {
        private readonly StateMachineBrainComponent m_brain;

        public string CurrentStateId => m_brain.CurrentStateId;
        public EnemyContext Context => m_brain.Context;

        public HeroBrainController(BrainProfileScriptableObject profile) =>
            m_brain = new StateMachineBrainComponent(profile);

        public void Enable() => m_brain.Enable();

        public void Disable() => m_brain.Disable();

        public void NotifyTargetEntered(Transform target) => m_brain.NotifyTargetEntered(target);

        public void NotifyTargetExited(Transform target) => m_brain.NotifyTargetExited(target);

        public void ClearTarget() => m_brain.ClearTarget();

        public HeroIntent SampleIntent(in HeroBodyState body, float deltaTime)
        {
            var enemyBody = new EnemyBodyState
            {
                Position = body.Position,
                IsGrounded = body.IsGrounded,
                IsAttacking = body.IsAttacking,
                IsDashing = body.IsDashing,
                CanAttack = body.CanAttack,
                CanDash = body.CanDash,
                IsDead = body.IsDead,
                HealthNormalized = body.HealthNormalized,
                HasDetectedTarget = body.HasDetectedTarget,
                DetectedTargetPosition = body.DetectedTargetPosition,
                DetectedTargetDistance = body.DetectedTargetDistance,
                DetectionRange = body.DetectionRange
            };

            EnemyIntent enemyIntent = m_brain.SampleIntent(in enemyBody, deltaTime);
            return new HeroIntent
            {
                MoveAxis = enemyIntent.MoveAxis,
                SpeedMultiplier = enemyIntent.SpeedMultiplier,
                DashPressed = enemyIntent.DashPressed,
                AttackPressed = enemyIntent.AttackPressed
            };
        }
    }
}
