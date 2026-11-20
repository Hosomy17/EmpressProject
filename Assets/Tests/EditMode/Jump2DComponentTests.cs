using System;
using Com.Voobox.Project.Component;
using Com.Voobox.Project.Data;
using FMODUnity;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Com.Voobox.Project.Tests.EditMode
{
    public class Jump2DComponentTests
    {
        private sealed class StubJumpData : IJumpData
        {
            public float JumpForce { get; set; } = 15f;
            public float JumpBufferTime { get; set; } = 0.2f;
            public EventReference SFXJumpEventReference { get; set; }
            public Rigidbody2D Rigidbody2D { get; set; }
        }

        private GameObject m_gameObject;
        private Rigidbody2D m_rigidbody2D;
        private StubJumpData m_jumpData;
        private Jump2DComponent m_jump;

        [SetUp]
        public void SetUp()
        {
            m_gameObject = new GameObject("Jump2DTestObject");
            m_rigidbody2D = m_gameObject.AddComponent<Rigidbody2D>();
            m_rigidbody2D.gravityScale = 0f;

            m_jumpData = new StubJumpData
            {
                Rigidbody2D = m_rigidbody2D
            };

            m_jump = new Jump2DComponent(m_jumpData);
        }

        [TearDown]
        public void TearDown()
        {
            if (m_gameObject != null)
            {
                UnityEngine.Object.DestroyImmediate(m_gameObject);
            }
        }

        [Test]
        public void JumpCut_HalvesVerticalVelocityAndPreservesHorizontalVelocity()
        {
            m_rigidbody2D.linearVelocity = new Vector2(-3f, 8f);

            m_jump.JumpCut();

            Assert.AreEqual(-3f, m_rigidbody2D.linearVelocity.x, 0.001f);
            Assert.AreEqual(4f, m_rigidbody2D.linearVelocity.y, 0.001f);
        }

        [Test]
        public void PogoJump_ZeroesVerticalVelocityAndAppliesUpwardImpulse()
        {
            m_rigidbody2D.linearVelocity = new Vector2(4f, -9f);

            m_jump.PogoJump();
            Physics2D.Simulate(Time.fixedDeltaTime);

            Assert.AreEqual(4f, m_rigidbody2D.linearVelocity.x, 0.001f);
            Assert.Greater(m_rigidbody2D.linearVelocity.y, 0f);
            Assert.AreEqual(m_jumpData.JumpForce / m_rigidbody2D.mass, m_rigidbody2D.linearVelocity.y, 0.01f);
        }

        [Test]
        public void Jump_ZeroesVerticalVelocityAndAppliesUpwardImpulse()
        {
            m_rigidbody2D.linearVelocity = new Vector2(2f, -5f);

            LogAssert.Expect(LogType.Error, "[FMOD] RuntimeManager accessed outside of runtime. Do not use RuntimeManager for Editor-only functionality, create your own System objects instead.");
            try
            {
                m_jump.Jump();
            }
            catch (NullReferenceException ex) when (ex.StackTrace != null && ex.StackTrace.Contains("FMODUnity.RuntimeManager"))
            {
                // FMOD Studio is not initialized in headless EditMode tests.
            }
            catch (Exception ex) when (ex.GetType().Namespace != null && ex.GetType().Namespace.StartsWith("FMOD", StringComparison.Ordinal))
            {
                // FMOD Studio may not be initialized in headless EditMode tests.
            }

            Physics2D.Simulate(Time.fixedDeltaTime);

            Assert.AreEqual(2f, m_rigidbody2D.linearVelocity.x, 0.001f);
            Assert.Greater(m_rigidbody2D.linearVelocity.y, 0f);
            Assert.AreEqual(m_jumpData.JumpForce / m_rigidbody2D.mass, m_rigidbody2D.linearVelocity.y, 0.01f);
        }
    }
}
