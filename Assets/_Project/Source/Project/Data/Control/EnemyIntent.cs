using UnityEngine;

namespace Com.Voobox.Project.Data
{
    public struct EnemyIntent
    {
        public Vector2 MoveAxis;
        public float SpeedMultiplier;
        public bool DashPressed;
        public bool AttackPressed;

        public static EnemyIntent Default => new() { SpeedMultiplier = 1f };
    }
}
