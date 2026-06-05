using FMODUnity;
using UnityEngine;

namespace Com.Voobox.Project.Data
{
    public interface IMovementData
    {
        public float Speed { get; }
        public float JumpForce { get; }
        public float RecoilBackForce { get; }
        public float RecoilUpForce { get; }
        public float RecoilDuration { get; }
        public EventReference SFXJumpEventReference { get; }
        public Rigidbody2D Rigidbody2D { get; }
    }
}