using System;
using UnityEngine;

namespace Com.Voobox.Project.AI
{
    [Serializable]
    public sealed class TargetWithinDistanceCondition : IAICondition
    {
        [SerializeField] private float m_minDistance = 1f;

        public float MinDistance => m_minDistance;

        public TargetWithinDistanceCondition() { }

        public TargetWithinDistanceCondition(float minDistance)
        {
            m_minDistance = minDistance;
        }

        public bool Evaluate(EnemyContext ctx)
        {
            return ctx.HasTarget && ctx.DistanceToTarget <= m_minDistance;
        }
    }
}
