using System;
using UnityEngine;

namespace Com.Voobox.Project.AI
{
    [Serializable]
    public sealed class NotCondition : IAICondition
    {
        [SerializeReference, SubclassSelector] private IAICondition m_condition;

        public IAICondition Condition => m_condition;

        public NotCondition() { }

        public NotCondition(IAICondition condition)
        {
            m_condition = condition;
        }

        public bool Evaluate(EnemyContext ctx)
        {
            return m_condition != null && !m_condition.Evaluate(ctx);
        }
    }
}
