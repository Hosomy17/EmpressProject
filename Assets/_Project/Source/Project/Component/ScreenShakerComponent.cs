using Com.Voobox.Project.Data;
using Unity.Cinemachine;
using UnityEngine;

namespace Com.Voobox.Project.Component
{
    public class ScreenShakerComponent
    {
        private readonly IScreenShakerData m_screenShakerData;

        private readonly CinemachineImpulseSource m_impulseSource;
        private readonly CinemachinePositionComposer m_positionComposer;
        private readonly Rigidbody2D m_rigidbody2D;

        private const float TARGET_OFFSET_X = 1;
        private const float TARGET_OFFSET_Y = -0.2f;
        private float m_currentTargetOffsetX;

        public ScreenShakerComponent(IScreenShakerData screenShakerData)
        {
            m_screenShakerData = screenShakerData;
            m_impulseSource = screenShakerData.ImpulseSource;
            m_positionComposer = screenShakerData.PositionComposer;
            m_rigidbody2D = screenShakerData.Rigidbody2D;
            m_currentTargetOffsetX = TARGET_OFFSET_X;
        }

        public void StabilizeTargetOffset()
        {
            var targetOffsetY = Mathf.Clamp(m_rigidbody2D.linearVelocity.y, TARGET_OFFSET_Y, 0f);
            m_positionComposer.TargetOffset.y = Mathf.Lerp(m_positionComposer.TargetOffset.y, targetOffsetY, Time.deltaTime * m_screenShakerData.SmoothSpeed);
            m_positionComposer.TargetOffset.x = Mathf.Lerp(m_positionComposer.TargetOffset.x, m_currentTargetOffsetX, Time.deltaTime * m_screenShakerData.SmoothSpeed);
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
            var screenShakeDirection = m_screenShakerData.ScreenShakeDirection;
            screenShakeDirection.x *= direction;
            m_impulseSource.GenerateImpulseWithVelocity(screenShakeDirection * m_screenShakerData.ScreenShakeForce);
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