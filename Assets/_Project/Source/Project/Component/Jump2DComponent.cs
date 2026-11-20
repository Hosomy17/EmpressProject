using System;
using Com.Voobox.Project.Data;
using Cysharp.Threading.Tasks;
using FMODUnity;
using UnityEngine;

namespace Com.Voobox.Project.Component
{
    public class Jump2DComponent
    {
        private readonly IJumpData m_jumpData;
        private readonly Rigidbody2D m_rigidbody2D;

        private readonly TaskHandle m_taskJumpBuffer = new();

        public bool IsJumpBufferActive { get; private set; }

        public Jump2DComponent(IJumpData jumpData)
        {
            m_jumpData = jumpData;
            m_rigidbody2D = jumpData.Rigidbody2D;
        }

        public void Jump()
        {
            m_rigidbody2D.linearVelocity = new Vector2(m_rigidbody2D.linearVelocity.x, 0);
            m_rigidbody2D.AddForce(Vector2.up * m_jumpData.JumpForce, ForceMode2D.Impulse);
            RuntimeManager.PlayOneShot(m_jumpData.SFXJumpEventReference);
        }

        public void PogoJump()
        {
            m_rigidbody2D.linearVelocity = new Vector2(m_rigidbody2D.linearVelocity.x, 0);
            m_rigidbody2D.AddForce(Vector2.up * m_jumpData.JumpForce, ForceMode2D.Impulse);
        }

        public void JumpCut()
        {
            m_rigidbody2D.linearVelocity = new Vector2(m_rigidbody2D.linearVelocity.x, m_rigidbody2D.linearVelocity.y * 0.5f);
        }

        public async UniTaskVoid StartJumpBuffer()
        {
            IsJumpBufferActive = true;
            await UniTask.Delay(TimeSpan.FromSeconds(m_jumpData.JumpBufferTime), delayTiming: PlayerLoopTiming.Update, cancellationToken: m_taskJumpBuffer.GetNewToken()).SuppressCancellationThrow();
            IsJumpBufferActive = false;
        }
    }
}
