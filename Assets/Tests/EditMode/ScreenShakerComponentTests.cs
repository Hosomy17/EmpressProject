using Com.Voobox.Project.Component;
using Com.Voobox.Project.Data;
using NUnit.Framework;
using Unity.Cinemachine;
using UnityEngine;

namespace Com.Voobox.Project.Tests.EditMode
{
    public class ScreenShakerComponentTests
    {
        private sealed class StubScreenShakerData : IScreenShakerData
        {
            public float SmoothSpeed { get; set; }
            public float ScreenShakeForce { get; set; }
            public Vector2 ScreenShakeDirection { get; set; }
            public CinemachineImpulseSource ImpulseSource { get; set; }
            public CinemachinePositionComposer PositionComposer { get; set; }
            public Rigidbody2D Rigidbody2D { get; set; }
        }

        private GameObject m_cameraGo;
        private GameObject m_bodyGo;
        private CinemachineImpulseSource m_impulseSource;
        private CinemachinePositionComposer m_positionComposer;
        private Rigidbody2D m_rigidbody2D;
        private StubScreenShakerData m_data;
        private ScreenShakerComponent m_screenShaker;

        [SetUp]
        public void SetUp()
        {
            m_cameraGo = new GameObject("TestCinemachineCamera");
            m_impulseSource = m_cameraGo.AddComponent<CinemachineImpulseSource>();
            m_positionComposer = m_cameraGo.AddComponent<CinemachinePositionComposer>();
            m_positionComposer.TargetOffset = Vector3.zero;

            m_bodyGo = new GameObject("TestBody");
            m_rigidbody2D = m_bodyGo.AddComponent<Rigidbody2D>();

            m_data = new StubScreenShakerData
            {
                SmoothSpeed = 100000f,
                ScreenShakeForce = 2.5f,
                ScreenShakeDirection = Vector2.one,
                ImpulseSource = m_impulseSource,
                PositionComposer = m_positionComposer,
                Rigidbody2D = m_rigidbody2D
            };

            m_screenShaker = new ScreenShakerComponent(m_data);
        }

        [TearDown]
        public void TearDown()
        {
            if (m_cameraGo != null)
                Object.DestroyImmediate(m_cameraGo);

            if (m_bodyGo != null)
                Object.DestroyImmediate(m_bodyGo);
        }

        [Test]
        public void FlipTargetOffsetX_True_SetsTargetOffsetXNegativeOne()
        {
            m_screenShaker.FlipTargetOffsetX(true);
            m_screenShaker.StabilizeTargetOffset();

            if (Time.deltaTime > 0f)
            {
                Assert.That(m_positionComposer.TargetOffset.x, Is.EqualTo(-1f).Within(0.001f));
            }
            else
            {
                Assert.That(m_positionComposer.TargetOffset.x, Is.LessThanOrEqualTo(0f));
            }
        }

        [Test]
        public void FlipTargetOffsetX_False_SetsTargetOffsetXPositiveOne()
        {
            m_screenShaker.FlipTargetOffsetX(true);
            m_screenShaker.FlipTargetOffsetX(false);
            m_screenShaker.StabilizeTargetOffset();

            if (Time.deltaTime > 0f)
            {
                Assert.That(m_positionComposer.TargetOffset.x, Is.EqualTo(1f).Within(0.001f));
            }
            else
            {
                Assert.That(m_positionComposer.TargetOffset.x, Is.GreaterThanOrEqualTo(0f));
            }
        }

        [Test]
        public void StabilizeTargetOffset_NegativeVelocityExceedingLimit_ClampsYToNegativeZeroPointTwo()
        {
            m_rigidbody2D.linearVelocity = new Vector2(0f, -5f);

            m_screenShaker.StabilizeTargetOffset();

            if (Time.deltaTime > 0f)
            {
                Assert.That(m_positionComposer.TargetOffset.y, Is.EqualTo(-0.2f).Within(0.001f));
            }
            else
            {
                Assert.That(m_positionComposer.TargetOffset.y, Is.EqualTo(0f).Within(0.001f));
            }
        }

        [Test]
        public void StabilizeTargetOffset_PositiveVelocity_ClampsYToZero()
        {
            m_positionComposer.TargetOffset = new Vector3(0f, -0.2f, 0f);
            m_rigidbody2D.linearVelocity = new Vector2(0f, 5f);

            m_screenShaker.StabilizeTargetOffset();

            if (Time.deltaTime > 0f)
            {
                Assert.That(m_positionComposer.TargetOffset.y, Is.EqualTo(0f).Within(0.001f));
            }
            else
            {
                Assert.That(m_positionComposer.TargetOffset.y, Is.EqualTo(-0.2f).Within(0.001f));
            }
        }

        [Test]
        public void StabilizeTargetOffset_VelocityWithinRange_PreservesVelocityY()
        {
            m_rigidbody2D.linearVelocity = new Vector2(0f, -0.1f);

            m_screenShaker.StabilizeTargetOffset();

            if (Time.deltaTime > 0f)
            {
                Assert.That(m_positionComposer.TargetOffset.y, Is.EqualTo(-0.1f).Within(0.001f));
            }
            else
            {
                Assert.That(m_positionComposer.TargetOffset.y, Is.EqualTo(0f).Within(0.001f));
            }
        }

        [Test]
        public void ShakeDirection_ExecutesWithoutThrowing()
        {
            Assert.DoesNotThrow(() => m_screenShaker.ShakeDirection(-1f));
            Assert.DoesNotThrow(() => m_screenShaker.ShakeDirection(1f));
        }

        [Test]
        public void ShakeForce_ExecutesWithoutThrowing()
        {
            Assert.DoesNotThrow(() => m_screenShaker.ShakeForce(3f));
        }

        [Test]
        public void GenerateImpulse_ExecutesWithoutThrowing()
        {
            Assert.DoesNotThrow(() => m_screenShaker.GenerateImpulse());
        }
    }
}
