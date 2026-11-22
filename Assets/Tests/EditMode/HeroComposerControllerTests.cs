using System.Reflection;
using Com.Voobox.Project.Component;
using Com.Voobox.Project.Data;
using Com.Voobox.Project.Entity;
using NUnit.Framework;
using UnityEngine;

namespace Com.Voobox.Project.Tests.EditMode
{
    public class HeroComposerControllerTests
    {
        private sealed class StubHeroController : IHeroController
        {
            public int EnableCount { get; private set; }
            public int DisableCount { get; private set; }
            public int SampleCount { get; private set; }
            public HeroBodyState LastBodyState { get; private set; }
            public float LastDeltaTime { get; private set; }
            public HeroIntent NextIntent { get; set; } = HeroIntent.Default;

            public void Enable() => EnableCount++;
            public void Disable() => DisableCount++;

            public HeroIntent SampleIntent(in HeroBodyState body, float deltaTime)
            {
                SampleCount++;
                LastBodyState = body;
                LastDeltaTime = deltaTime;
                return NextIntent;
            }
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Field '{fieldName}' not found on {target.GetType().Name}.");
            field.SetValue(target, value);
        }

        private static void InvokePrivateMethod(object target, string methodName, params object[] args)
        {
            MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, $"Method '{methodName}' not found on {target.GetType().Name}.");
            method.Invoke(target, args);
        }

        private static (GameObject heroGo, HeroComposer composer, HeroDataScriptableObject dataSo) CreateHero(
            float speed = 10f,
            int maxHitPoints = 3)
        {
            var heroDataSo = ScriptableObject.CreateInstance<HeroDataScriptableObject>();
            var heroData = new HeroData();
            SetPrivateField(heroData, "m_speed", speed);
            SetPrivateField(heroData, "m_maxHitPoints", maxHitPoints);
            SetPrivateField(heroDataSo, "m_heroData", heroData);

            var heroGo = new GameObject("HeroControllerTest");
            var composer = heroGo.AddComponent<HeroComposer>();
            var rb = heroGo.GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;

            SetPrivateField(composer, "m_heroData", heroDataSo);
            return (heroGo, composer, heroDataSo);
        }

        private static void CleanupHero(GameObject heroGo, HeroComposer composer, HeroDataScriptableObject originalDataSo)
        {
            if (composer != null)
            {
                InvokePrivateMethod(composer, "OnDestroy");

                var clonedSo = typeof(HeroComposer)
                    .GetField("m_heroData", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.GetValue(composer) as Object;
                if (clonedSo != null && clonedSo != originalDataSo)
                {
                    Object.DestroyImmediate(clonedSo);
                }
            }

            if (heroGo != null)
            {
                Object.DestroyImmediate(heroGo);
            }

            if (originalDataSo != null)
            {
                Object.DestroyImmediate(originalDataSo);
            }
        }

        [Test]
        public void SetController_SwapsIntentSource_AndManagesEnableDisableLifecycle()
        {
            (GameObject heroGo, HeroComposer composer, HeroDataScriptableObject dataSo) = CreateHero();
            var controllerA = new StubHeroController();
            var controllerB = new StubHeroController();

            try
            {
                InvokePrivateMethod(composer, "Awake");
                InvokePrivateMethod(composer, "Start");

                composer.SetController(controllerA);
                Assert.AreEqual(1, controllerA.EnableCount);
                Assert.AreEqual(0, controllerA.DisableCount);

                composer.SetController(controllerB);
                Assert.AreEqual(1, controllerA.DisableCount);
                Assert.AreEqual(1, controllerB.EnableCount);
                Assert.AreEqual(0, controllerB.DisableCount);

                InvokePrivateMethod(composer, "OnDisable");
                Assert.AreEqual(1, controllerB.DisableCount);

                InvokePrivateMethod(composer, "OnEnable");
                Assert.AreEqual(2, controllerB.EnableCount);
            }
            finally
            {
                CleanupHero(heroGo, composer, dataSo);
            }
        }

        [Test]
        public void SetController_DrivesVelocityWithSpeedMultiplier_AndFlipsSprite()
        {
            (GameObject heroGo, HeroComposer composer, HeroDataScriptableObject dataSo) = CreateHero(speed: 8f);
            var controller = new StubHeroController();

            try
            {
                InvokePrivateMethod(composer, "Awake");
                InvokePrivateMethod(composer, "Start");
                composer.SetController(controller);

                var rb = heroGo.GetComponent<Rigidbody2D>();
                var spriteRenderer = heroGo.GetComponent<SpriteRenderer>();

                controller.NextIntent = new HeroIntent
                {
                    MoveAxis = new Vector2(-1f, 0f),
                    SpeedMultiplier = 0.5f
                };

                InvokePrivateMethod(composer, "Update");
                InvokePrivateMethod(composer, "FixedUpdate");

                Assert.AreEqual(-4f, rb.linearVelocity.x, 0.001f);
                Assert.IsTrue(spriteRenderer.flipX);

                controller.NextIntent = new HeroIntent
                {
                    MoveAxis = new Vector2(1f, 0f),
                    SpeedMultiplier = 1.5f
                };

                InvokePrivateMethod(composer, "Update");
                InvokePrivateMethod(composer, "FixedUpdate");

                Assert.AreEqual(12f, rb.linearVelocity.x, 0.001f);
                Assert.IsFalse(spriteRenderer.flipX);
            }
            finally
            {
                CleanupHero(heroGo, composer, dataSo);
            }
        }

        [Test]
        public void Update_PassesAccurateHeroBodyState_ToController()
        {
            (GameObject heroGo, HeroComposer composer, HeroDataScriptableObject dataSo) = CreateHero(maxHitPoints: 4);
            var controller = new StubHeroController();

            try
            {
                heroGo.transform.position = new Vector3(3.25f, -1.5f, 0f);
                InvokePrivateMethod(composer, "Awake");
                composer.SetController(controller);

                SetPrivateField(composer, "m_isGrounded", true);
                SetPrivateField(composer, "m_isAttacking", false);
                SetPrivateField(composer, "m_isDashing", false);
                SetPrivateField(composer, "m_hasDash", true);

                InvokePrivateMethod(composer, "Update");

                Assert.AreEqual(1, controller.SampleCount);
                Assert.AreEqual(3.25f, controller.LastBodyState.Position.x, 0.001f);
                Assert.AreEqual(-1.5f, controller.LastBodyState.Position.y, 0.001f);
                Assert.IsTrue(controller.LastBodyState.IsGrounded);
                Assert.IsFalse(controller.LastBodyState.IsAttacking);
                Assert.IsFalse(controller.LastBodyState.IsDashing);
                Assert.IsTrue(controller.LastBodyState.CanAttack);
                Assert.IsTrue(controller.LastBodyState.CanDash);
                Assert.IsFalse(controller.LastBodyState.IsDead);
                Assert.AreEqual(1f, controller.LastBodyState.HealthNormalized, 0.001f);

                SetPrivateField(composer, "m_isGrounded", false);
                SetPrivateField(composer, "m_isAttacking", true);
                SetPrivateField(composer, "m_hasDash", false);

                InvokePrivateMethod(composer, "Update");

                Assert.AreEqual(2, controller.SampleCount);
                Assert.IsFalse(controller.LastBodyState.IsGrounded);
                Assert.IsTrue(controller.LastBodyState.IsAttacking);
                Assert.IsFalse(controller.LastBodyState.CanAttack);
                Assert.IsFalse(controller.LastBodyState.CanDash);
            }
            finally
            {
                CleanupHero(heroGo, composer, dataSo);
            }
        }

        [Test]
        public void HeroInputController_SampleIntent_DefaultsToSpeedMultiplierOne()
        {
            using var controller = new HeroInputController();
            controller.Enable();

            var body = new HeroBodyState { IsGrounded = true };
            HeroIntent intent = controller.SampleIntent(in body, 0.016f);

            Assert.AreEqual(Vector2.zero, intent.MoveAxis);
            Assert.AreEqual(1f, intent.SpeedMultiplier, 0.001f);
            Assert.IsFalse(intent.JumpPressed);
            Assert.IsFalse(intent.JumpReleased);
            Assert.IsFalse(intent.DashPressed);
            Assert.IsFalse(intent.AttackPressed);

            controller.Disable();
        }
    }
}
