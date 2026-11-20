using System;
using Com.Voobox.Project.Component;
using Com.Voobox.Project.Data;
using FMODUnity;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Com.Voobox.Project.Tests.EditMode
{
    public class Movement2DComponentTests
    {
        private sealed class StubMovementData : IMovementData
        {
            public float Speed { get; set; } = 10f;
            public float JumpForce { get; set; } = 15f;
            public float JumpBufferTime { get; set; } = 0.2f;
            public float RecoilBackForce { get; set; } = 6f;
            public float RecoilUpForce { get; set; } = 4f;
            public float RecoilDuration { get; set; } = 0.1f;
            public EventReference SFXJumpEventReference { get; set; }
            public Rigidbody2D Rigidbody2D { get; set; }
        }

        private GameObject m_gameObject;
        private Rigidbody2D m_rigidbody2D;
        private StubMovementData m_movementData;
        private Movement2DComponent m_movement;

        [SetUp]
        public void SetUp()
        {
            m_gameObject = new GameObject("Movement2DTestObject");
            m_rigidbody2D = m_gameObject.AddComponent<Rigidbody2D>();
            m_rigidbody2D.gravityScale = 0f;

            m_movementData = new StubMovementData
            {
                Rigidbody2D = m_rigidbody2D
            };

            m_movement = new Movement2DComponent(m_movementData);
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
        public void Run_SetsHorizontalVelocityAndPreservesVerticalVelocity()
        {
            m_rigidbody2D.linearVelocity = new Vector2(0f, -4.5f);

            m_movement.Run(0.75f);

            Assert.AreEqual(7.5f, m_rigidbody2D.linearVelocity.x, 0.001f);
            Assert.AreEqual(-4.5f, m_rigidbody2D.linearVelocity.y, 0.001f);
        }

        [Test]
        public void Run_NegativeInput_SetsNegativeHorizontalVelocityAndPreservesVerticalVelocity()
        {
            m_rigidbody2D.linearVelocity = new Vector2(5f, 2.25f);

            m_movement.Run(-1f);

            Assert.AreEqual(-10f, m_rigidbody2D.linearVelocity.x, 0.001f);
            Assert.AreEqual(2.25f, m_rigidbody2D.linearVelocity.y, 0.001f);
        }

        [Test]
        public void StopVerticalVelocity_ZeroesVerticalVelocityAndPreservesHorizontalVelocity()
        {
            m_rigidbody2D.linearVelocity = new Vector2(6f, 12f);

            m_movement.StopVerticalVelocity();

            Assert.AreEqual(6f, m_rigidbody2D.linearVelocity.x, 0.001f);
            Assert.AreEqual(0f, m_rigidbody2D.linearVelocity.y, 0.001f);
        }

        [Test]
        public void JumpCut_HalvesVerticalVelocityAndPreservesHorizontalVelocity()
        {
            m_rigidbody2D.linearVelocity = new Vector2(-3f, 8f);

            m_movement.JumpCut();

            Assert.AreEqual(-3f, m_rigidbody2D.linearVelocity.x, 0.001f);
            Assert.AreEqual(4f, m_rigidbody2D.linearVelocity.y, 0.001f);
        }

        [Test]
        public void PogoJump_ZeroesVerticalVelocityAndAppliesUpwardImpulse()
        {
            m_rigidbody2D.linearVelocity = new Vector2(4f, -9f);

            m_movement.PogoJump();
            Physics2D.Simulate(Time.fixedDeltaTime);

            Assert.AreEqual(4f, m_rigidbody2D.linearVelocity.x, 0.001f);
            Assert.Greater(m_rigidbody2D.linearVelocity.y, 0f);
            Assert.AreEqual(m_movementData.JumpForce / m_rigidbody2D.mass, m_rigidbody2D.linearVelocity.y, 0.01f);
        }

        [Test]
        public void Jump_ZeroesVerticalVelocityAndAppliesUpwardImpulse()
        {
            m_rigidbody2D.linearVelocity = new Vector2(2f, -5f);

            LogAssert.Expect(LogType.Error, "[FMOD] RuntimeManager accessed outside of runtime. Do not use RuntimeManager for Editor-only functionality, create your own System objects instead.");
            try
            {
                m_movement.Jump();
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
            Assert.AreEqual(m_movementData.JumpForce / m_rigidbody2D.mass, m_rigidbody2D.linearVelocity.y, 0.01f);
        }
    }
}
