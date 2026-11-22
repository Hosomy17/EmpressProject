using System;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Voobox.Project.AI
{
    [Serializable]
    public class AIStateDefinition
    {
        [SerializeField] private string m_stateId;
        [SerializeReference, SubclassSelector] private List<IAIBehavior> m_behaviors = new();
        [SerializeField] private List<AITransitionDefinition> m_transitions = new();

        public string StateId => m_stateId;
        public IReadOnlyList<IAIBehavior> Behaviors => m_behaviors;
        public IReadOnlyList<AITransitionDefinition> Transitions => m_transitions;

        public AIStateDefinition() { }

        public AIStateDefinition(
            string stateId,
            List<IAIBehavior> behaviors = null,
            List<AITransitionDefinition> transitions = null)
        {
            m_stateId = stateId;
            m_behaviors = behaviors ?? new List<IAIBehavior>();
            m_transitions = transitions ?? new List<AITransitionDefinition>();
        }
    }
}
