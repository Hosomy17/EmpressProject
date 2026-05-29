using Unity.Cinemachine;
using UnityEngine;

namespace Com.Voobox.Project.Component
{
    public class ScreenShakerComponent
    {
        private readonly Vector2 m_screenShakeDirection;
        private readonly float m_screenShakeForce;
        private readonly float m_smoothSpeed;
        
        private readonly CinemachineImpulseSource m_impulseSource;
        private readonly CinemachinePositionComposer m_positionComposer;
        private readonly Rigidbody2D m_rigidbody2D;
        
        private const float TARGET_OFFSET_X = 1;
        private const float TARGET_OFFSET_Y = -0.2f;
        private float m_currentTargetOffsetX;

        public ScreenShakerComponent(Vector2 screenShakeDirection, float screenShakeForce, float smoothSpeed, CinemachineImpulseSource impulseSource, CinemachinePositionComposer positionComposer ,Rigidbody2D rigidbody2D)
        {
            m_screenShakeDirection = screenShakeDirection;
            m_screenShakeForce = screenShakeForce;
            m_smoothSpeed = smoothSpeed;
            m_impulseSource = impulseSource;
            m_positionComposer = positionComposer;
            m_rigidbody2D = rigidbody2D;
            m_currentTargetOffsetX = TARGET_OFFSET_X;
        }

        public void StabilizeTargetOffset()
        {
            var targetOffsetY = Mathf.Clamp(m_rigidbody2D.linearVelocity.y, TARGET_OFFSET_Y, 0f);
            m_positionComposer.TargetOffset.y = Mathf.Lerp(m_positionComposer.TargetOffset.y, targetOffsetY, Time.deltaTime * m_smoothSpeed);
            m_positionComposer.TargetOffset.x = Mathf.Lerp(m_positionComposer.TargetOffset.x, m_currentTargetOffsetX, Time.deltaTime * m_smoothSpeed);
        }

        public void FlipTargetOffsetX(bool flip)
        {
            if (flip)
                m_currentTargetOffsetX = -TARGET_OFFSET_X;
            else
                m_currentTargetOffsetX = TARGET_OFFSET_X;
        }

        public void ShakeDirection(float direction)
        {
            var screenShakeDirection = m_screenShakeDirection;
            screenShakeDirection.x *= direction;
            m_impulseSource.GenerateImpulseWithVelocity(screenShakeDirection * m_screenShakeForce);
        }

        public void ShakeForce(float screenShakeForce)
        {
            m_impulseSource.GenerateImpulseWithForce(screenShakeForce);
        }

        public void GenerateImpulse()
        {
            m_impulseSource.GenerateImpulse();
        }
    }
}