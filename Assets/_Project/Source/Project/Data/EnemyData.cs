using System;
using ArtificeToolkit.Attributes;
using FMODUnity;
using UnityEngine;

namespace Com.Voobox.Project.Data
{
    [Serializable]
    public class EnemyData : IMovementData, IDasherData, IHealthData
    {
        [SerializeField, FoldoutGroup("Health Setup")] private int m_maxHitPoints;
        [SerializeField, FoldoutGroup("Health Setup")] private LayerMask m_hurtLayer;

        [SerializeField, FoldoutGroup("Movement Setup")] private float m_speed;
        [SerializeField, FoldoutGroup("Movement Setup")] private float m_recoilBackForce;
        [SerializeField, FoldoutGroup("Movement Setup")] private float m_recoilUpForce;
        [SerializeField, FoldoutGroup("Movement Setup")] private float m_recoilDuration;

        [SerializeField, FoldoutGroup("Dash Setup")] private float m_dashForce;
        [SerializeField, FoldoutGroup("Dash Setup")] private float m_dashDuration;
        [SerializeField, FoldoutGroup("Dash Setup")] private float m_dashCooldown;
        [SerializeField, FoldoutGroup("Dash Setup")] private EventReference m_sfxDashEventReference;

        [SerializeField, FoldoutGroup("Detection Setup")] private Vector2 m_detectionBoxSize = new Vector2(10f, 4f);
        [SerializeField, FoldoutGroup("Detection Setup")] private Vector2 m_detectionBoxOffset;
        [SerializeField, FoldoutGroup("Detection Setup")] private float m_detectionRange = 5.5f;
        [SerializeField, FoldoutGroup("Detection Setup")] private LayerMask m_detectionTargetLayer;

        public int MaxHitPoints => m_maxHitPoints;
        public LayerMask HurtLayer => m_hurtLayer;

        public float Speed => m_speed;
        public float RecoilBackForce => m_recoilBackForce;
        public float RecoilUpForce => m_recoilUpForce;
        public float RecoilDuration => m_recoilDuration;

        public float DashForce => m_dashForce;
        public float DashDuration => m_dashDuration;
        public float DashCooldown => m_dashCooldown;
        public EventReference SFXDashEventReference => m_sfxDashEventReference;

        public Vector2 DetectionBoxSize => m_detectionBoxSize;
        public Vector2 DetectionBoxOffset => m_detectionBoxOffset;
        public float DetectionRange => m_detectionRange;
        public LayerMask DetectionTargetLayer => m_detectionTargetLayer;

        public Rigidbody2D Rigidbody2D { get; set; }
    }
}
