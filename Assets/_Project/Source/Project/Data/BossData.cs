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

        public int MaxHitPoints => m_maxHitPoints;
        public LayerMask HurtLayer => m_hurtLayer;
    }
}
