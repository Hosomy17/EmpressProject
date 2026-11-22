using System;
using UnityEngine;

namespace Com.Voobox.Project.AI
{
    [Serializable]
    public sealed class StateElapsedCondition : IAICondition
    {
        [SerializeField] private float m_duration = 1f;

        public float Duration => m_duration;

        public StateElapsedCondition() { }

        public StateElapsedCondition(float duration)
        {
            m_duration = duration;
        }

        public bool Evaluate(EnemyContext ctx)
        {
            return ctx.TimeInCurrentState >= m_duration;
        }
    }
}
