using System;
using UnityEngine;

namespace Com.Voobox.Project.AI
{
    [Serializable]
    public class AITransitionDefinition
    {
        [SerializeField] private string m_targetStateId;
        [SerializeReference, SubclassSelector] private IAICondition m_condition;

        public string TargetStateId => m_targetStateId;
        public IAICondition Condition => m_condition;

        public AITransitionDefinition() { }

        public AITransitionDefinition(string targetStateId, IAICondition condition)
        {
            m_targetStateId = targetStateId;
            m_condition = condition;
        }
    }
}
