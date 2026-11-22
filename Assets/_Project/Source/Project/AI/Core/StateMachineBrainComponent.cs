using System;
using System.Collections.Generic;
using Com.Voobox.Project.Data;
using UnityEngine;

namespace Com.Voobox.Project.AI
{
    /// <summary>
    /// Generic data-driven finite state machine operating on <see cref="EnemyBodyState"/> and
    /// <see cref="EnemyIntent"/> with zero per-tick heap allocations.
    /// </summary>
    public sealed class StateMachineBrainComponent : IEnemyController
    {
        private readonly struct CompiledTransition
        {
            public readonly int TargetStateHash;
            public readonly IAICondition Condition;

            public CompiledTransition(int targetStateHash, IAICondition condition)
            {
                TargetStateHash = targetStateHash;
                Condition = condition;
            }
        }

        private readonly struct CompiledGlobalTransition
        {
            public readonly int TargetStateHash;
            public readonly IAICondition Condition;
            public readonly HashSet<int> ExcludedStateHashes;

            public CompiledGlobalTransition(
                int targetStateHash,
                IAICondition condition,
                HashSet<int> excludedStateHashes)
            {
                TargetStateHash = targetStateHash;
                Condition = condition;
                ExcludedStateHashes = excludedStateHashes;
            }
        }

        private sealed class StateEntry
        {
            public readonly int StateHash;
            public readonly string StateId;
            public readonly IAIBehavior[] Behaviors;
            public readonly CompiledTransition[] Transitions;

            public StateEntry(
                int stateHash,
                string stateId,
                IAIBehavior[] behaviors,
                CompiledTransition[] transitions)
            {
                StateHash = stateHash;
                StateId = stateId;
                Behaviors = behaviors;
                Transitions = transitions;
            }
        }

        private readonly Dictionary<int, StateEntry> m_states = new();
        private readonly CompiledGlobalTransition[] m_globalTransitions;
        private readonly EnemyContext m_context = new();

        private StateEntry m_currentState;
        private bool m_hasEnteredCurrentState;
        private bool m_isEnabled = true;

        public string CurrentStateId => m_currentState != null ? m_currentState.StateId : string.Empty;
        public EnemyContext Context => m_context;

        public StateMachineBrainComponent(BrainProfileScriptableObject profile)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            IReadOnlyList<AIStateDefinition> profileStates = profile.States;
            for (int i = 0; i < profileStates.Count; i++)
            {
                AIStateDefinition stateDef = profileStates[i];
                if (stateDef == null || string.IsNullOrEmpty(stateDef.StateId))
                {
                    continue;
                }

                int stateHash = Animator.StringToHash(stateDef.StateId);
                IAIBehavior[] behaviors = CompileBehaviors(stateDef.Behaviors);
                CompiledTransition[] transitions = CompileTransitions(stateDef.Transitions);
                m_states[stateHash] = new StateEntry(stateHash, stateDef.StateId, behaviors, transitions);
            }

            m_globalTransitions = CompileGlobalTransitions(profile.GlobalTransitions);

            if (!string.IsNullOrEmpty(profile.InitialStateId))
            {
                int initialHash = Animator.StringToHash(profile.InitialStateId);
                m_states.TryGetValue(initialHash, out m_currentState);
            }

            if (m_currentState == null && profileStates.Count > 0)
            {
                for (int i = 0; i < profileStates.Count; i++)
                {
                    AIStateDefinition stateDef = profileStates[i];
                    if (stateDef != null && !string.IsNullOrEmpty(stateDef.StateId))
                    {
                        int fallbackHash = Animator.StringToHash(stateDef.StateId);
                        if (m_states.TryGetValue(fallbackHash, out m_currentState))
                        {
                            break;
                        }
                    }
                }
            }
        }

        public void Enable()
        {
            m_isEnabled = true;
        }

        public void Disable()
        {
            m_isEnabled = false;
        }

        public void NotifyTargetEntered(Transform target)
        {
            m_context.NotifyTargetEntered(target);
        }

        public void NotifyTargetExited(Transform target)
        {
            m_context.NotifyTargetExited(target);
        }

        public void ClearTarget() => m_context.ClearTarget();

        public EnemyIntent SampleIntent(in EnemyBodyState body, float deltaTime)
        {
            if (!m_isEnabled || m_currentState == null)
            {
                m_context.EndTick();
                return EnemyIntent.Default;
            }

            EnemyIntent intent = EnemyIntent.Default;
            m_context.BeginTick(in body, deltaTime);

            if (!m_hasEnteredCurrentState)
            {
                ExecuteOnEnter(m_currentState.Behaviors, ref intent);
                m_hasEnteredCurrentState = true;
            }

            StateEntry nextState = EvaluateTransitions(m_currentState);
            if (nextState != null)
            {
                ExecuteOnExit(m_currentState.Behaviors);
                m_currentState = nextState;
                m_context.TimeInCurrentState = 0f;
                intent = EnemyIntent.Default;
                ExecuteOnEnter(m_currentState.Behaviors, ref intent);
            }
            else
            {
                ExecuteTick(m_currentState.Behaviors, ref intent);
            }

            m_context.EndTick();
            return intent;
        }

        private StateEntry EvaluateTransitions(StateEntry currentState)
        {
            CompiledTransition[] localTransitions = currentState.Transitions;
            for (int i = 0; i < localTransitions.Length; i++)
            {
                CompiledTransition transition = localTransitions[i];
                if (transition.Condition != null
                    && transition.Condition.Evaluate(m_context)
                    && m_states.TryGetValue(transition.TargetStateHash, out StateEntry nextState))
                {
                    return nextState;
                }
            }

            for (int i = 0; i < m_globalTransitions.Length; i++)
            {
                CompiledGlobalTransition globalTransition = m_globalTransitions[i];
                if (globalTransition.ExcludedStateHashes != null
                    && globalTransition.ExcludedStateHashes.Contains(currentState.StateHash))
                {
                    continue;
                }

                if (globalTransition.Condition != null
                    && globalTransition.Condition.Evaluate(m_context)
                    && m_states.TryGetValue(globalTransition.TargetStateHash, out StateEntry nextState))
                {
                    return nextState;
                }
            }

            return null;
        }

        private void ExecuteOnEnter(IAIBehavior[] behaviors, ref EnemyIntent intent)
        {
            for (int i = 0; i < behaviors.Length; i++)
            {
                behaviors[i].OnEnter(m_context, ref intent);
            }
        }

        private void ExecuteTick(IAIBehavior[] behaviors, ref EnemyIntent intent)
        {
            for (int i = 0; i < behaviors.Length; i++)
            {
                behaviors[i].Tick(m_context, ref intent);
            }
        }

        private void ExecuteOnExit(IAIBehavior[] behaviors)
        {
            for (int i = 0; i < behaviors.Length; i++)
            {
                behaviors[i].OnExit(m_context);
            }
        }

        private static IAIBehavior[] CompileBehaviors(IReadOnlyList<IAIBehavior> source)
        {
            if (source == null || source.Count == 0)
            {
                return Array.Empty<IAIBehavior>();
            }

            int validCount = 0;
            for (int i = 0; i < source.Count; i++)
            {
                if (source[i] != null)
                {
                    validCount++;
                }
            }

            if (validCount == 0)
            {
                return Array.Empty<IAIBehavior>();
            }

            var result = new IAIBehavior[validCount];
            int writeIndex = 0;
            for (int i = 0; i < source.Count; i++)
            {
                if (source[i] != null)
                {
                    result[writeIndex++] = source[i];
                }
            }

            return result;
        }

        private static CompiledTransition[] CompileTransitions(IReadOnlyList<AITransitionDefinition> source)
        {
            if (source == null || source.Count == 0)
            {
                return Array.Empty<CompiledTransition>();
            }

            int validCount = 0;
            for (int i = 0; i < source.Count; i++)
            {
                AITransitionDefinition transition = source[i];
                if (transition != null && !string.IsNullOrEmpty(transition.TargetStateId))
                {
                    validCount++;
                }
            }

            if (validCount == 0)
            {
                return Array.Empty<CompiledTransition>();
            }

            var result = new CompiledTransition[validCount];
            int writeIndex = 0;
            for (int i = 0; i < source.Count; i++)
            {
                AITransitionDefinition transition = source[i];
                if (transition != null && !string.IsNullOrEmpty(transition.TargetStateId))
                {
                    result[writeIndex++] = new CompiledTransition(
                        Animator.StringToHash(transition.TargetStateId),
                        transition.Condition);
                }
            }

            return result;
        }

        private static CompiledGlobalTransition[] CompileGlobalTransitions(IReadOnlyList<GlobalTransitionDefinition> source)
        {
            if (source == null || source.Count == 0)
            {
                return Array.Empty<CompiledGlobalTransition>();
            }

            int validCount = 0;
            for (int i = 0; i < source.Count; i++)
            {
                GlobalTransitionDefinition transition = source[i];
                if (transition != null && !string.IsNullOrEmpty(transition.TargetStateId))
                {
                    validCount++;
                }
            }

            if (validCount == 0)
            {
                return Array.Empty<CompiledGlobalTransition>();
            }

            var result = new CompiledGlobalTransition[validCount];
            int writeIndex = 0;
            for (int i = 0; i < source.Count; i++)
            {
                GlobalTransitionDefinition transition = source[i];
                if (transition == null || string.IsNullOrEmpty(transition.TargetStateId))
                {
                    continue;
                }

                HashSet<int> excludedHashes = null;
                IReadOnlyList<string> excludedIds = transition.ExcludedStateIds;
                if (excludedIds != null && excludedIds.Count > 0)
                {
                    excludedHashes = new HashSet<int>();
                    for (int j = 0; j < excludedIds.Count; j++)
                    {
                        if (!string.IsNullOrEmpty(excludedIds[j]))
                        {
                            excludedHashes.Add(Animator.StringToHash(excludedIds[j]));
                        }
                    }
                }

                result[writeIndex++] = new CompiledGlobalTransition(
                    Animator.StringToHash(transition.TargetStateId),
                    transition.Condition,
                    excludedHashes);
            }

            return result;
        }
    }
}
