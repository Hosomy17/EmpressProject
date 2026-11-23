using FMODUnity;
using UnityEngine;

namespace Com.Voobox.Project.Data
{
    public interface IJumpData
    {
        public float JumpForce { get; }
        public float JumpBufferTime { get; }
        public LayerMask GroundLayer { get; }
        public EventReference SFXJumpEventReference { get; }
        public Rigidbody2D Rigidbody2D { get; }
    }
}
