using Com.Voobox.Project.Component;
using Com.Voobox.Project.Data;
using NUnit.Framework;
using UnityEngine;

namespace Com.Voobox.Project.Tests.EditMode
{
    public class Movement2DComponentTests
    {
        private sealed class StubMovementData : IMovementData
        {
            public float Speed { get; set; } = 10f;
            public float RecoilBackForce { get; set; } = 6f;
            public float RecoilUpForce { get; set; } = 4f;
            public float RecoilDuration { get; set; } = 0.1f;
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
    }
}
