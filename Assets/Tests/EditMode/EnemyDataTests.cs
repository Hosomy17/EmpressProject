using System.Reflection;
using Com.Voobox.Project.Data;
using FMODUnity;
using NUnit.Framework;
using UnityEngine;

namespace Com.Voobox.Project.Tests.EditMode
{
    public class EnemyDataTests
    {
        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Field '{fieldName}' not found on {target.GetType().Name}.");
            field.SetValue(target, value);
        }

        private sealed class FakeHeroController : IHeroController
        {
            public bool IsEnabled { get; private set; }

            public void Enable() => IsEnabled = true;

            public void Disable() => IsEnabled = false;

            public HeroIntent SampleIntent(in HeroBodyState body, float deltaTime)
            {
                if (!IsEnabled || body.IsDead)
                {
                    return HeroIntent.Default;
                }

                return new HeroIntent
                {
                    MoveAxis = new Vector2(1f, 0f),
                    SpeedMultiplier = 1.25f,
                    JumpPressed = body.IsGrounded,
                    JumpReleased = false,
                    DashPressed = body.CanDash,
                    AttackPressed = body.CanAttack
                };
            }
        }

        private sealed class FakeEnemyController : IEnemyController
        {
            public bool IsEnabled { get; private set; }

            public void Enable() => IsEnabled = true;

            public void Disable() => IsEnabled = false;

            public EnemyIntent SampleIntent(in EnemyBodyState body, float deltaTime)
            {
                if (!IsEnabled || body.IsDead)
                {
                    return EnemyIntent.Default;
                }

                return new EnemyIntent
                {
                    MoveAxis = body.HasDetectedTarget ? new Vector2(-1f, 0f) : Vector2.zero,
                    SpeedMultiplier = 1f,
                    DashPressed = body.CanDash,
                    AttackPressed = body.CanAttack
                };
            }
        }

        [Test]
        public void HeroIntent_Default_HasUnitSpeedMultiplierAndZeroedInputs()
        {
            Assert.IsTrue(typeof(HeroIntent).IsValueType);

            HeroIntent intent = HeroIntent.Default;

            Assert.AreEqual(Vector2.zero, intent.MoveAxis);
            Assert.AreEqual(1f, intent.SpeedMultiplier, 0.0001f);
            Assert.IsFalse(intent.JumpPressed);
            Assert.IsFalse(intent.JumpReleased);
            Assert.IsFalse(intent.DashPressed);
            Assert.IsFalse(intent.AttackPressed);
        }

        [Test]
        public void HeroBodyState_IsValueTypeAndStoresAllBodyAndDetectionFields()
        {
            Assert.IsTrue(typeof(HeroBodyState).IsValueType);

            var state = new HeroBodyState
            {
                Position = new Vector2(3.5f, -1.25f),
                IsGrounded = true,
                IsAttacking = false,
                IsDashing = true,
                CanAttack = true,
                CanDash = false,
                IsDead = false,
                HealthNormalized = 0.75f,
                HasDetectedTarget = true,
                DetectedTargetPosition = new Vector2(7.5f, -1.25f),
                DetectedTargetDistance = 4f,
                DetectionRange = 5.5f
            };

            Assert.AreEqual(new Vector2(3.5f, -1.25f), state.Position);
            Assert.IsTrue(state.IsGrounded);
            Assert.IsFalse(state.IsAttacking);
            Assert.IsTrue(state.IsDashing);
            Assert.IsTrue(state.CanAttack);
            Assert.IsFalse(state.CanDash);
            Assert.IsFalse(state.IsDead);
            Assert.AreEqual(0.75f, state.HealthNormalized, 0.0001f);
            Assert.IsTrue(state.HasDetectedTarget);
            Assert.AreEqual(new Vector2(7.5f, -1.25f), state.DetectedTargetPosition);
            Assert.AreEqual(4f, state.DetectedTargetDistance, 0.0001f);
            Assert.AreEqual(5.5f, state.DetectionRange, 0.0001f);
        }

        [Test]
        public void EnemyIntent_Default_HasUnitSpeedMultiplierAndZeroedInputs()
        {
            Assert.IsTrue(typeof(EnemyIntent).IsValueType);

            EnemyIntent intent = EnemyIntent.Default;

            Assert.AreEqual(Vector2.zero, intent.MoveAxis);
            Assert.AreEqual(1f, intent.SpeedMultiplier, 0.0001f);
            Assert.IsFalse(intent.DashPressed);
            Assert.IsFalse(intent.AttackPressed);
        }

        [Test]
        public void EnemyBodyState_IsValueTypeAndStoresAllBodyAndDetectionFields()
        {
            Assert.IsTrue(typeof(EnemyBodyState).IsValueType);

            var state = new EnemyBodyState
            {
                Position = new Vector2(-2.5f, 1.5f),
                IsGrounded = false,
                IsAttacking = true,
                IsDashing = false,
                CanAttack = false,
                CanDash = true,
                IsDead = false,
                HealthNormalized = 0.5f,
                HasDetectedTarget = true,
                DetectedTargetPosition = new Vector2(1.5f, 1.5f),
                DetectedTargetDistance = 4f,
                DetectionRange = 6f
            };

            Assert.AreEqual(new Vector2(-2.5f, 1.5f), state.Position);
            Assert.IsFalse(state.IsGrounded);
            Assert.IsTrue(state.IsAttacking);
            Assert.IsFalse(state.IsDashing);
            Assert.IsFalse(state.CanAttack);
            Assert.IsTrue(state.CanDash);
            Assert.IsFalse(state.IsDead);
            Assert.AreEqual(0.5f, state.HealthNormalized, 0.0001f);
            Assert.IsTrue(state.HasDetectedTarget);
            Assert.AreEqual(new Vector2(1.5f, 1.5f), state.DetectedTargetPosition);
            Assert.AreEqual(4f, state.DetectedTargetDistance, 0.0001f);
            Assert.AreEqual(6f, state.DetectionRange, 0.0001f);
        }

        [Test]
        public void ControllerInterfaces_InheritGenericEntityControllerAndSampleIntentsSynchronously()
        {
            Assert.IsTrue(typeof(IEntityController<HeroBodyState, HeroIntent>).IsAssignableFrom(typeof(IHeroController)));
            Assert.IsTrue(typeof(IEntityController<EnemyBodyState, EnemyIntent>).IsAssignableFrom(typeof(IEnemyController)));

            var heroControllerImpl = new FakeHeroController();
            IHeroController heroController = heroControllerImpl;
            var heroBody = new HeroBodyState
            {
                IsGrounded = true,
                CanDash = true,
                CanAttack = false
            };

            Assert.AreEqual(HeroIntent.Default, heroController.SampleIntent(in heroBody, 0.016f));

            heroController.Enable();
            Assert.IsTrue(heroControllerImpl.IsEnabled);
            HeroIntent sampledHero = heroController.SampleIntent(in heroBody, 0.016f);
            Assert.AreEqual(new Vector2(1f, 0f), sampledHero.MoveAxis);
            Assert.AreEqual(1.25f, sampledHero.SpeedMultiplier, 0.0001f);
            Assert.IsTrue(sampledHero.JumpPressed);
            Assert.IsFalse(sampledHero.JumpReleased);
            Assert.IsTrue(sampledHero.DashPressed);
            Assert.IsFalse(sampledHero.AttackPressed);

            heroController.Disable();
            Assert.IsFalse(heroControllerImpl.IsEnabled);

            var enemyControllerImpl = new FakeEnemyController();
            IEnemyController enemyController = enemyControllerImpl;
            var enemyBody = new EnemyBodyState
            {
                HasDetectedTarget = true,
                CanDash = false,
                CanAttack = true
            };

            Assert.AreEqual(EnemyIntent.Default, enemyController.SampleIntent(in enemyBody, 0.016f));

            enemyController.Enable();
            Assert.IsTrue(enemyControllerImpl.IsEnabled);
            EnemyIntent sampledEnemy = enemyController.SampleIntent(in enemyBody, 0.016f);
            Assert.AreEqual(new Vector2(-1f, 0f), sampledEnemy.MoveAxis);
            Assert.AreEqual(1f, sampledEnemy.SpeedMultiplier, 0.0001f);
            Assert.IsFalse(sampledEnemy.DashPressed);
            Assert.IsTrue(sampledEnemy.AttackPressed);

            enemyController.Disable();
            Assert.IsFalse(enemyControllerImpl.IsEnabled);
        }

        [Test]
        public void EnemyData_DefaultDetectionValues_AreInitialized()
        {
            var enemyData = new EnemyData();

            Assert.AreEqual(new Vector2(10f, 4f), enemyData.DetectionBoxSize);
            Assert.AreEqual(Vector2.zero, enemyData.DetectionBoxOffset);
            Assert.AreEqual(5.5f, enemyData.DetectionRange, 0.0001f);
            Assert.AreEqual((LayerMask)0, enemyData.DetectionTargetLayer);
        }

        [Test]
        public void EnemyData_ImplementsExpectedInterfacesAndExposesConfiguredValues()
        {
            var enemyData = new EnemyData();
            var sfxDashRef = new EventReference();

            SetPrivateField(enemyData, "m_maxHitPoints", 12);
            SetPrivateField(enemyData, "m_hurtLayer", (LayerMask)(1 << 8));
            SetPrivateField(enemyData, "m_speed", 6.5f);
            SetPrivateField(enemyData, "m_recoilBackForce", 4f);
            SetPrivateField(enemyData, "m_recoilUpForce", 2.5f);
            SetPrivateField(enemyData, "m_recoilDuration", 0.15f);
            SetPrivateField(enemyData, "m_dashForce", 14f);
            SetPrivateField(enemyData, "m_dashDuration", 0.2f);
            SetPrivateField(enemyData, "m_dashCooldown", 1.1f);
            SetPrivateField(enemyData, "m_sfxDashEventReference", sfxDashRef);
            SetPrivateField(enemyData, "m_detectionBoxSize", new Vector2(12f, 5f));
            SetPrivateField(enemyData, "m_detectionBoxOffset", new Vector2(1.5f, 0.5f));
            SetPrivateField(enemyData, "m_detectionRange", 7.25f);
            SetPrivateField(enemyData, "m_detectionTargetLayer", (LayerMask)(1 << 6));

            var bodyGo = new GameObject("EnemyDataTestBody");
            var enemyDataSo = ScriptableObject.CreateInstance<EnemyDataScriptableObject>();
            try
            {
                var rb = bodyGo.AddComponent<Rigidbody2D>();
                enemyData.Rigidbody2D = rb;
                SetPrivateField(enemyDataSo, "m_enemyData", enemyData);

                EnemyData exposed = enemyDataSo.EnemyData;
                Assert.AreSame(enemyData, exposed);
                Assert.AreEqual((LayerMask)(1 << 8), exposed.HurtLayer);
                Assert.AreEqual(new Vector2(12f, 5f), exposed.DetectionBoxSize);
                Assert.AreEqual(new Vector2(1.5f, 0.5f), exposed.DetectionBoxOffset);
                Assert.AreEqual(7.25f, exposed.DetectionRange, 0.0001f);
                Assert.AreEqual((LayerMask)(1 << 6), exposed.DetectionTargetLayer);

                IHealthData healthData = exposed;
                Assert.AreEqual(12, healthData.MaxHitPoints);

                IMovementData movementData = exposed;
                Assert.AreEqual(6.5f, movementData.Speed, 0.0001f);
                Assert.AreEqual(4f, movementData.RecoilBackForce, 0.0001f);
                Assert.AreEqual(2.5f, movementData.RecoilUpForce, 0.0001f);
                Assert.AreEqual(0.15f, movementData.RecoilDuration, 0.0001f);
                Assert.AreSame(rb, movementData.Rigidbody2D);

                IDasherData dasherData = exposed;
                Assert.AreEqual(14f, dasherData.DashForce, 0.0001f);
                Assert.AreEqual(0.2f, dasherData.DashDuration, 0.0001f);
                Assert.AreEqual(1.1f, dasherData.DashCooldown, 0.0001f);
                Assert.AreEqual(sfxDashRef, dasherData.SFXDashEventReference);
                Assert.AreSame(rb, dasherData.Rigidbody2D);
            }
            finally
            {
                Object.DestroyImmediate(enemyDataSo);
                Object.DestroyImmediate(bodyGo);
            }
        }
    }
}
