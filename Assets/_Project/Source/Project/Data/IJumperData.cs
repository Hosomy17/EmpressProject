using UnityEngine;

namespace Com.Voobox.Project.Data
{
    public interface IJumperData
    {
        public float JumpForce { get; }
        public LayerMask GroundLayer { get; }
        public Rigidbody2D Rigidbody2D { get; }
    }
}