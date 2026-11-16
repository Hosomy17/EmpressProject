using Com.Voobox.Project.Data;
using UnityEngine;

namespace Com.Voobox.Project.Component
{
    public class HealthComponent
    {
        private readonly IHealthData m_healthData;

        private int m_currentHitPoints { get; set; }
        public bool IsDead => m_currentHitPoints <= 0;

        public HealthComponent(IHealthData healthData)
        {
            m_healthData = healthData;
            m_currentHitPoints = Mathf.Max(0, healthData.MaxHitPoints);
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
