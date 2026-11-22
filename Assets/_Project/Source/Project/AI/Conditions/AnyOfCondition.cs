using System;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Voobox.Project.AI
{
    [Serializable]
    public sealed class AnyOfCondition : IAICondition
    {
        [SerializeReference, SubclassSelector] private List<IAICondition> m_conditions = new();

        public IReadOnlyList<IAICondition> Conditions => m_conditions;

        public AnyOfCondition() { }

        public AnyOfCondition(params IAICondition[] conditions)
        {
            m_conditions = conditions != null ? new List<IAICondition>(conditions) : new List<IAICondition>();
        }

        public AnyOfCondition(List<IAICondition> conditions)
        {
            m_conditions = conditions ?? new List<IAICondition>();
        }

        public bool Evaluate(EnemyContext ctx)
        {
            if (m_conditions == null || m_conditions.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < m_conditions.Count; i++)
            {
                IAICondition condition = m_conditions[i];
                if (condition != null && condition.Evaluate(ctx))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
