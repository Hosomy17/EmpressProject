using System;
using Com.Voobox.Project.Data;
using UnityEngine;

namespace Com.Voobox.Project.AI
{
    [Serializable]
    public sealed class MoveTowardTargetBehavior : IAIBehavior
    {
        [SerializeField] private float m_speedMultiplier = 1f;
        [SerializeField] private float m_stopDistance;

        public float SpeedMultiplier => m_speedMultiplier;
        public float StopDistance => m_stopDistance;

        public MoveTowardTargetBehavior() { }

        public MoveTowardTargetBehavior(float speedMultiplier, float stopDistance = 0f)
        {
            m_speedMultiplier = speedMultiplier;
            m_stopDistance = stopDistance;
        }

        public void OnEnter(EnemyContext ctx, ref EnemyIntent intent)
        {
            ApplyMove(ctx, ref intent);
        }

        public void Tick(EnemyContext ctx, ref EnemyIntent intent)
        {
            ApplyMove(ctx, ref intent);
        }

        public void OnExit(EnemyContext ctx) { }

        private void ApplyMove(EnemyContext ctx, ref EnemyIntent intent)
        {
            if (!ctx.HasTarget)
            {
                intent.MoveAxis = Vector2.zero;
                return;
            }

            float deltaX = ctx.TargetPosition.x - ctx.Body.Position.x;
            if (Mathf.Abs(deltaX) <= m_stopDistance || Mathf.Abs(deltaX) <= 0.0001f)
            {
                intent.MoveAxis = Vector2.zero;
                return;
            }

            intent.MoveAxis = new Vector2(Mathf.Sign(deltaX), 0f);
            intent.SpeedMultiplier = m_speedMultiplier;
        }
    }
}
