using System;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Voobox.Project.AI
{
    [Serializable]
    public class GlobalTransitionDefinition
    {
        [SerializeField] private string m_targetStateId;
        [SerializeReference, SubclassSelector] private IAICondition m_condition;
        [SerializeField] private List<string> m_excludedStateIds = new();

        public string TargetStateId => m_targetStateId;
        public IAICondition Condition => m_condition;
        public IReadOnlyList<string> ExcludedStateIds => m_excludedStateIds;

        public GlobalTransitionDefinition() { }

        public GlobalTransitionDefinition(
            string targetStateId,
            IAICondition condition,
            List<string> excludedStateIds = null)
        {
            m_targetStateId = targetStateId;
            m_condition = condition;
            m_excludedStateIds = excludedStateIds ?? new List<string>();
        }
    }

    [CreateAssetMenu(fileName = "BrainProfile", menuName = "AI/Brain Profile")]
    public class BrainProfileScriptableObject : ScriptableObject
    {
        [SerializeField] private string m_initialStateId;
        [SerializeField] private List<AIStateDefinition> m_states = new();
        [SerializeField] private List<GlobalTransitionDefinition> m_globalTransitions = new();

        public string InitialStateId => m_initialStateId;
        public IReadOnlyList<AIStateDefinition> States => m_states;
        public IReadOnlyList<GlobalTransitionDefinition> GlobalTransitions => m_globalTransitions;

        public void Configure(
            string initialStateId,
            List<AIStateDefinition> states,
            List<GlobalTransitionDefinition> globalTransitions = null)
        {
            m_initialStateId = initialStateId;
            m_states = states ?? new List<AIStateDefinition>();
            m_globalTransitions = globalTransitions ?? new List<GlobalTransitionDefinition>();
        }
    }
}
