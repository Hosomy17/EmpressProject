using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Com.Voobox.Project.Component
{
    public class DasherComponent
    {
        private readonly float m_dashForce;
        private readonly float m_dashDuration;
        private readonly float m_dashCooldown;
        
        private readonly Rigidbody2D m_rigidbody2D;
        
        private readonly TaskHandle m_taskDash = new();
        private readonly TaskHandle m_taskDashCooldown = new();

        public DasherComponent(float dashForce, float dashDuration, float dashCooldown, Rigidbody2D rigidbody2D)
        {
            m_dashForce = dashForce;
            m_dashDuration = dashDuration;
            m_dashCooldown = dashCooldown;
            m_rigidbody2D = rigidbody2D;
        }

        public async UniTask Dash(int dashDir)
        {

            var originalGravity = m_rigidbody2D.gravityScale;
            m_rigidbody2D.gravityScale = 0f;
            m_rigidbody2D.linearVelocity = new Vector2(dashDir * m_dashForce, 0f);

            await UniTask.Delay(TimeSpan.FromSeconds(m_dashDuration), cancellationToken: m_taskDash.GetNewToken()).SuppressCancellationThrow();

            m_rigidbody2D.gravityScale = originalGravity;
        }

        public async UniTask DashCooldown()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(m_dashCooldown), cancellationToken: m_taskDashCooldown.GetNewToken()).SuppressCancellationThrow();
        }
    }
}