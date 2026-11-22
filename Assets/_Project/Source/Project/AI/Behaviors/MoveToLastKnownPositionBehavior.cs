using System;
using Com.Voobox.Project.Data;
using UnityEngine;

namespace Com.Voobox.Project.AI
{
    [Serializable]
    public sealed class MoveToLastKnownPositionBehavior : IAIBehavior
    {
        [SerializeField] private float m_speedMultiplier = 1f;
        [SerializeField] private float m_arrivalDistance = 0.25f;

        public float SpeedMultiplier => m_speedMultiplier;
        public float ArrivalDistance => m_arrivalDistance;

        public MoveToLastKnownPositionBehavior() { }

        public MoveToLastKnownPositionBehavior(float speedMultiplier, float arrivalDistance = 0.25f)
        {
            m_speedMultiplier = speedMultiplier;
            m_arrivalDistance = arrivalDistance;
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
            if (!ctx.HasLastKnownTargetPosition)
            {
                intent.MoveAxis = Vector2.zero;
                return;
            }

            float deltaX = ctx.LastKnownTargetPosition.x - ctx.Body.Position.x;
            if (Mathf.Abs(deltaX) <= m_arrivalDistance || Mathf.Abs(deltaX) <= 0.0001f)
            {
                intent.MoveAxis = Vector2.zero;
                return;
            }

            intent.MoveAxis = new Vector2(Mathf.Sign(deltaX), 0f);
            intent.SpeedMultiplier = m_speedMultiplier;
        }
    }
}
