using Com.Voobox.Project.Data;
using UnityEngine;

namespace Com.Voobox.Project.Component
{
    public class HealthComponent
    {
        private readonly IHealthData m_healthData;
        private readonly int m_maxHitPoints;

        private int m_currentHitPoints { get; set; }
        public int CurrentHitPoints => m_currentHitPoints;
        public int MaxHitPoints => m_maxHitPoints;
        public float HealthNormalized => m_maxHitPoints > 0 ? (float)m_currentHitPoints / m_maxHitPoints : 0f;
        public bool IsDead => m_currentHitPoints <= 0;

        public HealthComponent(IHealthData healthData)
        {
            m_healthData = healthData;
            m_maxHitPoints = Mathf.Max(0, healthData.MaxHitPoints);
            m_currentHitPoints = m_maxHitPoints;
        }

        /// <summary>
        /// Decreases CurrentHealth by 1, never below 0.
        /// Returns true only on the call that transitions HP to 0.
        /// </summary>
        public void TakeDamage()
        {
            if (IsDead) return;

            m_currentHitPoints = Mathf.Max(0, m_currentHitPoints - 1);
        }
    }
}
