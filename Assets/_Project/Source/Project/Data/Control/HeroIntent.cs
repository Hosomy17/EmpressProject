using UnityEngine;

namespace Com.Voobox.Project.Data
{
    public struct HeroIntent
    {
        public Vector2 MoveAxis;
        public float SpeedMultiplier;
        public bool JumpPressed;
        public bool JumpReleased;
        public bool DashPressed;
        public bool AttackPressed;

        public static HeroIntent Default => new() { SpeedMultiplier = 1f };
    }
}
