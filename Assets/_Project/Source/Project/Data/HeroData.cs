using System;
using ArtificeToolkit.Attributes;
using Com.Voobox.Project.Others;
using FMODUnity;
using Unity.Cinemachine;
using UnityEngine;

namespace Com.Voobox.Project.Data
{
    [Serializable]
    public class HeroData : IAttackerData, IDasherData, IHealthData, IJumpData, IMovementData, IScreenShakerData
    {
        [SerializeField, FoldoutGroup("Attack Setup")] private float m_attackDuration;

        [SerializeField, FoldoutGroup("Dash Setup")] private float m_dashForce;
        [SerializeField, FoldoutGroup("Dash Setup")] private float m_dashDuration;
        [SerializeField, FoldoutGroup("Dash Setup")] private float m_dashCooldown;
        [SerializeField, FoldoutGroup("Dash Setup")] private EventReference m_sfxDashEventReference;

        [SerializeField, FoldoutGroup("Health Setup")] private int m_maxHitPoints;
        [SerializeField, FoldoutGroup("Health Setup")] private LayerMask m_hurtLayer;
        [SerializeField, FoldoutGroup("Health Setup")] private float m_knockbackForce;

        [SerializeField, FoldoutGroup("Jump Setup")] private float m_jumpForce;
        [SerializeField, FoldoutGroup("Jump Setup")] private float m_jumpBufferTime;
        [SerializeField, FoldoutGroup("Jump Setup")] private LayerMask m_groundLayer;
        [SerializeField, FoldoutGroup("Jump Setup")] private EventReference m_sfxJumpEventReference;

        [SerializeField, FoldoutGroup("Movement Setup")] private float m_speed;
        [SerializeField, FoldoutGroup("Movement Setup")] private float m_recoilBackForce;
        [SerializeField, FoldoutGroup("Movement Setup")] private float m_recoilUpForce;
        [SerializeField, FoldoutGroup("Movement Setup")] private float m_recoilDuration;

        [SerializeField, FoldoutGroup("ScreenShake Setup")] private float m_smoothSpeed; 
        [SerializeField, FoldoutGroup("ScreenShake Setup")] private float m_screenShakeForce;
        [SerializeField, FoldoutGroup("ScreenShake Setup")] private Vector2 m_screenShakeDirection;

        public float AttackDuration => m_attackDuration;
        public AttackArea AttackUp { get; set; }
        public AttackArea AttackDown { get; set; }
        public AttackArea AttackLeft { get; set; }
        public AttackArea AttackRight { get; set; }

        public float DashForce => m_dashForce;
        public float DashDuration => m_dashDuration;
        public float DashCooldown => m_dashCooldown;
        public EventReference SFXDashEventReference => m_sfxDashEventReference;

        public int MaxHitPoints => m_maxHitPoints;
        public LayerMask HurtLayer => m_hurtLayer;
        public float KnockbackForce => m_knockbackForce;

        public float JumpForce => m_jumpForce;
        public float JumpBufferTime => m_jumpBufferTime;
        public LayerMask GroundLayer => m_groundLayer;
        public EventReference SFXJumpEventReference => m_sfxJumpEventReference;

        public float Speed => m_speed;
        public float RecoilBackForce => m_recoilBackForce;
        public float RecoilUpForce => m_recoilUpForce;
        public float RecoilDuration => m_recoilDuration;

        public float SmoothSpeed => m_smoothSpeed;
        public float ScreenShakeForce => m_screenShakeForce;
        public Vector2 ScreenShakeDirection => m_screenShakeDirection;
        public CinemachineImpulseSource ImpulseSource { get; set; }
        public CinemachinePositionComposer PositionComposer { get; set; }

        public Rigidbody2D Rigidbody2D { get; set; }
    }
}