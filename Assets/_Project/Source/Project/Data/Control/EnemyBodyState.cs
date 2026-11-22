using UnityEngine;

namespace Com.Voobox.Project.Data
{
    public struct EnemyBodyState
    {
        public Vector2 Position;
        public bool IsGrounded;
        public bool IsAttacking;
        public bool IsDashing;
        public bool CanAttack;
        public bool CanDash;
        public bool IsDead;
        public float HealthNormalized;
        public bool HasDetectedTarget;
        public Vector2 DetectedTargetPosition;
        public float DetectedTargetDistance;
        public float DetectionRange;
    }
}
