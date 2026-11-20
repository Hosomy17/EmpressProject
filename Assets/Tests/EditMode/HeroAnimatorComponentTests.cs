using Com.Voobox.Project.Component;
using NUnit.Framework;
using UnityEngine;

namespace Com.Voobox.Project.Tests.EditMode
{
    [TestFixture]
    public class HeroAnimatorComponentTests
    {
        private GameObject m_gameObject;
        private Animator m_animator;
        private SpriteRenderer m_spriteRenderer;
        private HeroAnimatorComponent m_heroAnimator;

        [SetUp]
        public void SetUp()
        {
            m_gameObject = new GameObject("HeroAnimatorTestObject");
            m_animator = m_gameObject.AddComponent<Animator>();
            m_spriteRenderer = m_gameObject.AddComponent<SpriteRenderer>();
            m_heroAnimator = new HeroAnimatorComponent(m_animator, m_spriteRenderer);
        }

        [TearDown]
        public void TearDown()
        {
            if (m_gameObject != null)
            {
                Object.DestroyImmediate(m_gameObject);
            }

            m_gameObject = null;
            m_animator = null;
            m_spriteRenderer = null;
            m_heroAnimator = null;
        }

        [Test]
        public void FlipX_True_SetsSpriteRendererFlipXTrue()
        {
            m_heroAnimator.FlipX(true);

            Assert.IsTrue(m_spriteRenderer.flipX);
        }

        [Test]
        public void FlipX_False_SetsSpriteRendererFlipXFalse()
        {
            m_heroAnimator.FlipX(true);
            m_heroAnimator.FlipX(false);

            Assert.IsFalse(m_spriteRenderer.flipX);
        }

        [Test]
        public void SetSpeed_WithValidAnimator_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => m_heroAnimator.SetSpeed(5.5f));
        }

        [Test]
        public void SetGrounded_WithValidAnimator_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
            {
                m_heroAnimator.SetGrounded(true);
                m_heroAnimator.SetGrounded(false);
            });
        }

        [Test]
        public void SetDash_WithValidAnimator_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
            {
                m_heroAnimator.SetDash(true);
                m_heroAnimator.SetDash(false);
            });
        }

        [Test]
        public void SetAirVelocity_WithValidAnimator_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => m_heroAnimator.SetAirVelocity(-2.5f));
        }

        [Test]
        public void SetGhost_WithValidAnimator_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
            {
                m_heroAnimator.SetGhost(true);
                m_heroAnimator.SetGhost(false);
            });
        }

        [Test]
        public void SetAttackFront_WithValidAnimator_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
            {
                m_heroAnimator.SetAttackFront(true);
                m_heroAnimator.SetAttackFront(false);
            });
        }

        [Test]
        public void SetAttackUp_WithValidAnimator_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
            {
                m_heroAnimator.SetAttackUp(true);
                m_heroAnimator.SetAttackUp(false);
            });
        }

        [Test]
        public void SetAttackDown_WithValidAnimator_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
            {
                m_heroAnimator.SetAttackDown(true);
                m_heroAnimator.SetAttackDown(false);
            });
        }

        [Test]
        public void SetAllAttacks_WithValidAnimator_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
            {
                m_heroAnimator.SetAllAttacks(true);
                m_heroAnimator.SetAllAttacks(false);
            });
        }
    }
}
