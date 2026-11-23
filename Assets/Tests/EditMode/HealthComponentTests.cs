using Com.Voobox.Project.Component;
using Com.Voobox.Project.Data;
using NUnit.Framework;
using UnityEngine;

namespace Com.Voobox.Project.Tests.EditMode
{
    public class HealthComponentTests
    {
        private sealed class StubHealthData : IHealthData
        {
            public int MaxHitPoints { get; }
            public LayerMask HurtLayer { get; }
            public float KnockbackForce { get; }

            public StubHealthData(int maxHealth)
            {
                MaxHitPoints = maxHealth;
            }
        }

        [Test]
        public void InitialCurrentHealth_EqualsMaxHealth()
        {
            var health = new HealthComponent(new StubHealthData(3));

            Assert.IsFalse(health.IsDead);
        }

        [Test]
        public void TakeDamage_Once_DecreasesCurrentHealthByOne()
        {
            var health = new HealthComponent(new StubHealthData(3));

            health.TakeDamage();

            Assert.IsFalse(health.IsDead);
        }

        [Test]
        public void TakeDamage_ThreeTimes_ReachesZeroAndReturnsTrueOnlyOnThirdCall()
        {
            var health = new HealthComponent(new StubHealthData(3));

            health.TakeDamage();
            Assert.IsFalse(health.IsDead);
            health.TakeDamage();
            Assert.IsFalse(health.IsDead);
            health.TakeDamage();
            Assert.IsTrue(health.IsDead);
        }

        [Test]
        public void TakeDamage_FiveTimes_ClampsAtZeroAndReturnsFalseAfterDeath()
        {
            var health = new HealthComponent(new StubHealthData(3));

            health.TakeDamage();
            Assert.IsFalse(health.IsDead);
            health.TakeDamage();
            Assert.IsFalse(health.IsDead);
            health.TakeDamage();
            Assert.IsTrue(health.IsDead);
            health.TakeDamage();
            Assert.IsTrue(health.IsDead);
            health.TakeDamage();
            Assert.IsTrue(health.IsDead);
        }

        [Test]
        public void Metrics_InitialState_ExposeMaxCurrentAndFullNormalizedHealth()
        {
            var health = new HealthComponent(new StubHealthData(4));

            Assert.AreEqual(4, health.MaxHitPoints);
            Assert.AreEqual(4, health.CurrentHitPoints);
            Assert.AreEqual(1f, health.HealthNormalized, 0.0001f);
        }

        [Test]
        public void Metrics_AfterTakeDamage_UpdateCurrentAndNormalizedHealth()
        {
            var health = new HealthComponent(new StubHealthData(4));

            health.TakeDamage();
            Assert.AreEqual(4, health.MaxHitPoints);
            Assert.AreEqual(3, health.CurrentHitPoints);
            Assert.AreEqual(0.75f, health.HealthNormalized, 0.0001f);

            health.TakeDamage();
            health.TakeDamage();
            health.TakeDamage();
            Assert.AreEqual(0, health.CurrentHitPoints);
            Assert.AreEqual(0f, health.HealthNormalized, 0.0001f);

            health.TakeDamage();
            Assert.AreEqual(0, health.CurrentHitPoints);
            Assert.AreEqual(0f, health.HealthNormalized, 0.0001f);
        }

        [Test]
        public void Metrics_WithZeroOrNegativeMaxHitPoints_ClampToZeroAndReturnZeroNormalized()
        {
            var zeroHealth = new HealthComponent(new StubHealthData(0));
            Assert.AreEqual(0, zeroHealth.MaxHitPoints);
            Assert.AreEqual(0, zeroHealth.CurrentHitPoints);
            Assert.AreEqual(0f, zeroHealth.HealthNormalized, 0.0001f);
            Assert.IsTrue(zeroHealth.IsDead);

            var negativeHealth = new HealthComponent(new StubHealthData(-5));
            Assert.AreEqual(0, negativeHealth.MaxHitPoints);
            Assert.AreEqual(0, negativeHealth.CurrentHitPoints);
            Assert.AreEqual(0f, negativeHealth.HealthNormalized, 0.0001f);
            Assert.IsTrue(negativeHealth.IsDead);
        }
    }
}
