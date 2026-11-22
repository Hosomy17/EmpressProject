using Com.Voobox.Project.Data;
using UnityEngine;

namespace Com.Voobox.Project.AI
{
    /// <summary>
    /// Pre-allocated per-brain blackboard holding perception, body state, and state timing.
    /// </summary>
    public sealed class EnemyContext
    {
        private Vector2 m_targetPositionOverride;
        private bool m_hasTargetPositionOverride;

        public bool HasTarget { get; private set; }
        public Transform Target { get; private set; }
        public Vector2 TargetPosition { get; private set; }
        public Vector2 LastKnownTargetPosition { get; private set; }
        public bool HasLastKnownTargetPosition { get; private set; }

        public bool IsTargetInZone { get; private set; }
        public bool EnteredZoneThisTick { get; private set; }
        public bool ExitedZoneThisTick { get; private set; }

        public float DistanceToTarget { get; private set; } = float.PositiveInfinity;
        public float DirectionToTargetX { get; private set; }

        public EnemyBodyState Body { get; private set; }
        public float TimeInCurrentState { get; internal set; }
        public int TickIndex { get; private set; }

        /// <summary>
        /// Sets an explicit target position override so tests can drive perception without scene transforms.
        /// </summary>
        public void SetTargetPosition(Vector2 position)
        {
            m_targetPositionOverride = position;
            m_hasTargetPositionOverride = true;
            TargetPosition = position;
            HasTarget = true;
            LastKnownTargetPosition = position;
            HasLastKnownTargetPosition = true;
            UpdateTargetMetrics();
        }

        public void ClearTargetPositionOverride()
        {
            m_hasTargetPositionOverride = false;
            RefreshTargetPresence();
        }

        public void ClearTarget()
        {
            Target = null;
            m_hasTargetPositionOverride = false;
            IsTargetInZone = false;
            RefreshTargetPresence();
        }

        public void NotifyTargetEntered(Transform target)
        {
            if (target != null)
            {
                Target = target;
                m_hasTargetPositionOverride = false;
                TargetPosition = target.position;
                LastKnownTargetPosition = TargetPosition;
                HasLastKnownTargetPosition = true;
            }

            IsTargetInZone = true;
            EnteredZoneThisTick = true;
            RefreshTargetPresence();
        }

        public void NotifyTargetExited(Transform target)
        {
            if (Target != null && target != null && target != Target)
            {
                return;
            }

            if (Target != null)
            {
                TargetPosition = Target.position;
                LastKnownTargetPosition = TargetPosition;
                HasLastKnownTargetPosition = true;
            }
            else if (target != null)
            {
                LastKnownTargetPosition = target.position;
                HasLastKnownTargetPosition = true;
            }
            else if (m_hasTargetPositionOverride)
            {
                LastKnownTargetPosition = m_targetPositionOverride;
                HasLastKnownTargetPosition = true;
            }

            m_hasTargetPositionOverride = false;
            IsTargetInZone = false;
            ExitedZoneThisTick = true;
            RefreshTargetPresence();
        }

        internal void BeginTick(in EnemyBodyState body, float deltaTime)
        {
            TickIndex++;

            Body = body;
            TimeInCurrentState += deltaTime > 0f ? deltaTime : 0f;

            RefreshTargetPresence();
        }

        internal void EndTick()
        {
            EnteredZoneThisTick = false;
            ExitedZoneThisTick = false;
        }

        private void RefreshTargetPresence()
        {
            if (m_hasTargetPositionOverride)
            {
                HasTarget = true;
                TargetPosition = m_targetPositionOverride;
                UpdateTargetMetrics();
            }
            else if (Body.HasDetectedTarget)
            {
                HasTarget = true;
                TargetPosition = Body.DetectedTargetPosition;
                DistanceToTarget = Body.DetectedTargetDistance;
                DirectionToTargetX = Mathf.Abs(TargetPosition.x - Body.Position.x) > 0.0001f
                    ? Mathf.Sign(TargetPosition.x - Body.Position.x)
                    : 0f;
            }
            else if (Target != null)
            {
                HasTarget = true;
                TargetPosition = Target.position;
                UpdateTargetMetrics();
            }
            else
            {
                HasTarget = false;
                UpdateTargetMetrics();
            }

            if (HasTarget)
            {
                LastKnownTargetPosition = TargetPosition;
                HasLastKnownTargetPosition = true;
            }
        }

        private void UpdateTargetMetrics()
        {
            if (!HasTarget)
            {
                DistanceToTarget = float.PositiveInfinity;
                DirectionToTargetX = 0f;
                return;
            }

            Vector2 delta = TargetPosition - Body.Position;
            DistanceToTarget = delta.magnitude;
            DirectionToTargetX = Mathf.Abs(delta.x) > 0.0001f ? Mathf.Sign(delta.x) : 0f;
        }
    }
}
