using Com.Voobox.Project.Component;
using Com.Voobox.Project.Data;
using FMODUnity;
using NUnit.Framework;
using UnityEngine;

namespace Com.Voobox.Project.Tests.EditMode
{
    public class DasherComponentTests
    {
        private sealed class StubDasherData : IDasherData
        {
            public float DashForce { get; set; } = 15f;
            public float DashDuration { get; set; } = 0.2f;
            public float DashCooldown { get; set; } = 0.5f;
            public EventReference SFXDashEventReference { get; set; }
            public Rigidbody2D Rigidbody2D { get; set; }
        }

        private GameObject m_gameObject;
        private Rigidbody2D m_rigidbody2D;

        [SetUp]
        public void SetUp()
        {
            m_gameObject = new GameObject("DasherComponentTestObject");
            m_rigidbody2D = m_gameObject.AddComponent<Rigidbody2D>();
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
        public void Constructor_WithEmptySfxEventReference_SetsHasDashSfxFalse()
        {
            var dasherData = new StubDasherData
            {
                Rigidbody2D = m_rigidbody2D,
                SFXDashEventReference = default
            };

            var dasher = new DasherComponent(dasherData);

            Assert.IsFalse(dasher.HasDashSfx);
        }

        [Test]
        public void TryPlayDashSfx_WithEmptySfxEventReference_ReturnsFalseWithoutThrowing()
        {
            var dasherData = new StubDasherData
            {
                Rigidbody2D = m_rigidbody2D,
                SFXDashEventReference = default
            };

            var dasher = new DasherComponent(dasherData);
            var played = false;

            Assert.DoesNotThrow(() => played = dasher.TryPlayDashSfx());
            Assert.IsFalse(played);
        }

        [Test]
        public void HasDashSfx_WithAssignedGuid_ReturnsTrue()
        {
            var dasherData = new StubDasherData
            {
                Rigidbody2D = m_rigidbody2D,
                SFXDashEventReference = new EventReference
                {
                    Guid = new FMOD.GUID { Data1 = 1 }
                }
            };

            var dasher = new DasherComponent(dasherData);

            Assert.IsTrue(dasher.HasDashSfx);
        }
    }
}
