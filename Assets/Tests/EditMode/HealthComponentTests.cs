using Com.Voobox.Project.Component;
using Com.Voobox.Project.Data;
using NUnit.Framework;

namespace Com.Voobox.Project.Tests.EditMode
{
    public class HealthComponentTests
    {
        private sealed class StubHealthData : IHealthData
        {
            public int MaxHitPoints { get; }

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
    }
}
