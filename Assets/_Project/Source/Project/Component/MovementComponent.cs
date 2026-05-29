using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Com.Voobox.Project.Component
{
    public class MovementComponent
    {
        private readonly float m_speed;
        private readonly Rigidbody2D m_rigidbody2D;
        
        private readonly float m_recoilBackForce;
        private readonly float m_recoilUpForce;
        private readonly float m_recoilDuration;

        public MovementComponent(float speed, float recoilBackForce, float recoilUpForce, float recoilDuration, Rigidbody2D rigidbody2D)
        {
            m_speed = speed;
            m_recoilBackForce = recoilBackForce;
            m_recoilUpForce = recoilUpForce;
            m_recoilDuration = recoilDuration;
            m_rigidbody2D = rigidbody2D;
        }
        
        public void Run(float vectorX)
        {
            var speed = vectorX * m_speed;
            m_rigidbody2D.linearVelocity = new Vector2(speed, m_rigidbody2D.linearVelocity.y);
        }
        
        public async UniTask Recoil(float direction)
        {
            m_rigidbody2D.linearVelocity = new Vector2(direction * m_recoilBackForce, m_recoilUpForce);
            await UniTask.Delay(TimeSpan.FromSeconds(m_recoilDuration));
        }

        public void StopVerticalVelocity()
        {
            m_rigidbody2D.linearVelocity = new Vector2(m_rigidbody2D.linearVelocity.x, 0);
        }
    }
}