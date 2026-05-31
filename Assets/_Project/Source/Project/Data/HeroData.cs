using System;
using ArtificeToolkit.Attributes;
using Com.Voobox.Project.Others;
using Unity.Cinemachine;
using UnityEngine;

namespace Com.Voobox.Project.Data
{
    [Serializable]
    public class HeroData : IAttackerData, IDasherData, IJumperData, IMovementData, IScreenShakerData
    {
        [SerializeField, FoldoutGroup("Attack Setup")] private float m_attackDuration;

        [SerializeField, FoldoutGroup("Dash Setup")] private float m_dashForce;
        [SerializeField, FoldoutGroup("Dash Setup")] private float m_dashDuration;
        [SerializeField, FoldoutGroup("Dash Setup")] private float m_dashCooldown;

        [SerializeField, FoldoutGroup("Jump Setup")] private float m_jumpForce;
        [SerializeField, FoldoutGroup("Jump Setup")] private LayerMask m_groundLayer;

        [SerializeField, FoldoutGroup("Movement Setup")] private float m_speed;

        [SerializeField, FoldoutGroup("ScreenShake Setup")] private float m_smoothSpeed; 
        [SerializeField, FoldoutGroup("ScreenShake Setup")] private float m_screenShakeForce;
        [SerializeField, FoldoutGroup("ScreenShake Setup")] private Vector2 m_screenShakeDirection;
        [SerializeField, FoldoutGroup("ScreenShake Setup")] private float m_recoilBackForce;
        [SerializeField, FoldoutGroup("ScreenShake Setup")] private float m_recoilUpForce;
        [SerializeField, FoldoutGroup("ScreenShake Setup")] private float m_recoilDuration;

        public float AttackDuration => m_attackDuration;
        public AttackArea AttackUp { get; set; }
        public AttackArea AttackDown { get; set; }
        public AttackArea AttackLeft { get; set; }
        public AttackArea AttackRight { get; set; }

        public float DashForce => m_dashForce;

        public float DashDuration => m_dashDuration;

        public float DashCooldown => m_dashCooldown;
        public CinemachinePositionComposer PositionComposer { get; set; }
        public Rigidbody2D Rigidbody2D { get; set; }

        public float JumpForce => m_jumpForce;

        public LayerMask GroundLayer => m_groundLayer;

        public float Speed => m_speed;

        public float SmoothSpeed => m_smoothSpeed;

        public float ScreenShakeForce => m_screenShakeForce;

        public Vector2 ScreenShakeDirection => m_screenShakeDirection;
        public CinemachineImpulseSource ImpulseSource { get; set; }

        public float RecoilBackForce => m_recoilBackForce;

        public float RecoilUpForce => m_recoilUpForce;

        public float RecoilDuration => m_recoilDuration;
    }
}