using System;
using ArtificeToolkit.Attributes;
using UnityEngine;

namespace Com.Voobox.Project.Data
{
    [Serializable]
    public class BossData : IHealthData
    {
        [SerializeField, FoldoutGroup("Health Setup")] private int m_maxHitPoints;
        [SerializeField, FoldoutGroup("Health Setup")] private LayerMask m_hurtLayer;
        [SerializeField, FoldoutGroup("Health Setup")] private float m_knockbackForce;

        public int MaxHitPoints => m_maxHitPoints;
        public LayerMask HurtLayer => m_hurtLayer;
        public float KnockbackForce => m_knockbackForce;
    }
}
