using System;
using Com.Voobox.Project.Data;
using Cysharp.Threading.Tasks;
using FMODUnity;
using UnityEngine;

namespace Com.Voobox.Project.Component
{
    public class Movement2DComponent
    {
        private readonly IMovementData m_movementData;
        private readonly Rigidbody2D m_rigidbody2D;

        public Movement2DComponent(IMovementData movementData)
        {
            m_movementData = movementData;
            m_rigidbody2D = movementData.Rigidbody2D;
        }

        public void Run(float vectorX)
        {
            var speed = vectorX * m_movementData.Speed;
            m_rigidbody2D.linearVelocity = new Vector2(speed, m_rigidbody2D.linearVelocity.y);
        }

        public async UniTask Recoil(float direction)
        {
            m_rigidbody2D.linearVelocity = new Vector2(direction * m_movementData.RecoilBackForce, m_movementData.RecoilUpForce);
            await UniTask.Delay(TimeSpan.FromSeconds(m_movementData.RecoilDuration));
        }

        public void StopVerticalVelocity()
        {
            m_rigidbody2D.linearVelocity = new Vector2(m_rigidbody2D.linearVelocity.x, 0);
        }

        public void Jump()
        {
            m_rigidbody2D.linearVelocity = new Vector2(m_rigidbody2D.linearVelocity.x, 0);
            m_rigidbody2D.AddForce(Vector2.up * m_movementData.JumpForce, ForceMode2D.Impulse);
            RuntimeManager.PlayOneShot(m_movementData.SFXJumpEventReference);
        }

        public void PogoJump()
        {
            m_rigidbody2D.linearVelocity = new Vector2(m_rigidbody2D.linearVelocity.x, 0);
            m_rigidbody2D.AddForce(Vector2.up * m_movementData.JumpForce, ForceMode2D.Impulse);
        }

        public void JumpCut()
        {
            m_rigidbody2D.linearVelocity = new Vector2(m_rigidbody2D.linearVelocity.x, m_rigidbody2D.linearVelocity.y * 0.5f);
        }
    }
}