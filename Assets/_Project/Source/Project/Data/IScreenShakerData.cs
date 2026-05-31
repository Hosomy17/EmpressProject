using Unity.Cinemachine;
using UnityEngine;

namespace Com.Voobox.Project.Data
{
    public interface IScreenShakerData
    {
        public float SmoothSpeed { get; }
        public float ScreenShakeForce { get; }
        public Vector2 ScreenShakeDirection { get; }
        public CinemachineImpulseSource ImpulseSource { get; }
        public CinemachinePositionComposer PositionComposer{ get; }
        public Rigidbody2D Rigidbody2D { get; }
    }
}