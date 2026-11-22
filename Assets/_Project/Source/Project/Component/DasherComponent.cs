using System;
using Com.Voobox.Project.Data;
using Cysharp.Threading.Tasks;
using FMODUnity;
using UnityEngine;

namespace Com.Voobox.Project.Component
{
    public class DasherComponent
    {
        private readonly IDasherData m_dasherData;
        private readonly Rigidbody2D m_rigidbody2D;

        private readonly TaskHandle m_taskDash = new();
        private readonly TaskHandle m_taskDashCooldown = new();

        public bool HasDashSfx => !m_dasherData.SFXDashEventReference.IsNull;

        public DasherComponent(IDasherData dasherData)
        {
            m_dasherData = dasherData;
            m_rigidbody2D = dasherData.Rigidbody2D;
        }

        public bool TryPlayDashSfx()
        {
            if (!HasDashSfx)
            {
                return false;
            }

            RuntimeManager.PlayOneShot(m_dasherData.SFXDashEventReference);
            return true;
        }

        public async UniTask Dash(int dashDir)
        {
            float originalGravity = m_rigidbody2D.gravityScale;
            m_rigidbody2D.gravityScale = 0f;
            m_rigidbody2D.linearVelocity = new Vector2(dashDir * m_dasherData.DashForce, 0f);
            TryPlayDashSfx();

            await UniTask.Delay(TimeSpan.FromSeconds(m_dasherData.DashDuration), cancellationToken: m_taskDash.GetNewToken()).SuppressCancellationThrow();

            m_rigidbody2D.gravityScale = originalGravity;
        }

        public async UniTask DashCooldown()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(m_dasherData.DashCooldown), cancellationToken: m_taskDashCooldown.GetNewToken()).SuppressCancellationThrow();
        }
    }
}