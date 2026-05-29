using System;
using UnityEngine;

namespace Com.Voobox.Project.Data
{
    [Serializable]
    public class HeroData
    {
        [Header("Settings Jump")]
        [SerializeField] private float m_jumpForce;
        [SerializeField] private LayerMask m_groundLayer;
    
        [Header("Setting Movement")]
        [SerializeField] private float m_speed;
    
        [Header("Settings Dash")]
        [SerializeField] private float m_dashForce;
        [SerializeField] private float m_dashDuration;
        [SerializeField] private float m_dashCooldown;
    
        [Header("Settings Attack")]
        [SerializeField] private float m_atkTime;

        [Header("Settings Screen")]
        [SerializeField] private float m_smoothSpeed = 1f; 
        [SerializeField] private float m_screenShakeForce;
        [SerializeField] private Vector2 m_screenShakeDirection;
        [SerializeField] private float m_recoilBackForce;
        [SerializeField] private float m_recoilUpForce;
        [SerializeField] private float m_recoilDuration;

        public float JumpForce => m_jumpForce;

        public LayerMask GroundLayer => m_groundLayer;

        public float Speed => m_speed;

        public float DashForce => m_dashForce;

        public float DashDuration => m_dashDuration;

        public float DashCooldown => m_dashCooldown;

        public float AtkTime => m_atkTime;

        public float SmoothSpeed => m_smoothSpeed;

        public float ScreenShakeForce => m_screenShakeForce;

        public Vector2 ScreenShakeDirection => m_screenShakeDirection;

        public float RecoilBackForce => m_recoilBackForce;

        public float RecoilUpForce => m_recoilUpForce;

        public float RecoilDuration => m_recoilDuration;
    }
}