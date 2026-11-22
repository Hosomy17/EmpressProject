using System;
using System.Collections.Generic;
using System.Reflection;
using Com.Voobox.Project.AI;
using Com.Voobox.Project.Component;
using Com.Voobox.Project.Data;
using Com.Voobox.Project.Entity;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Com.Voobox.Project.Tests.EditMode
{
    public class EnemyComposerTests
    {
        private sealed class StubEnemyController : IEnemyController
        {
            public int EnableCount { get; private set; }
            public int DisableCount { get; private set; }
            public int SampleCount { get; private set; }
            public EnemyBodyState LastBodyState { get; private set; }
            public EnemyIntent NextIntent { get; set; } = EnemyIntent.Default;

            public void Enable() => EnableCount++;
            public void Disable() => DisableCount++;

            public EnemyIntent SampleIntent(in EnemyBodyState body, float deltaTime)
            {
                SampleCount++;
                LastBodyState = body;
                return NextIntent;
            }
        }

        private readonly List<Object> m_createdObjects = new();

        [TearDown]
        public void TearDown()
        {
            for (int i = m_createdObjects.Count - 1; i >= 0; i--)
            {
                if (m_createdObjects[i] != null)
                {
                    Object.DestroyImmediate(m_createdObjects[i]);
                }
            }

            m_createdObjects.Clear();
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Field '{fieldName}' not found on {target.GetType().Name}.");
            field.SetValue(target, value);
        }

        private static T GetPrivateField<T>(object target, string fieldName)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Field '{fieldName}' not found on {target.GetType().Name}.");
            return (T)field.GetValue(target);
        }

        private static void InvokePrivateMethod(object target, string methodName, params object[] args)
        {
            MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, $"Method '{methodName}' not found on {target.GetType().Name}.");
            method.Invoke(target, args);
        }

        private (GameObject enemyGo, EnemyComposer composer, EnemyDataScriptableObject dataSo) CreateEnemy(
            float speed = 6f,
            int maxHitPoints = 3,
            LayerMask hurtLayer = default,
            BrainProfileScriptableObject brainProfile = null,
            int layer = 7,
            Vector2? detectionBoxSize = null,
            Vector2 detectionBoxOffset = default,
            float detectionRange = 5.5f,
            LayerMask detectionTargetLayer = default)
        {
            var dataSo = ScriptableObject.CreateInstance<EnemyDataScriptableObject>();
            m_createdObjects.Add(dataSo);

            var data = new EnemyData();
            SetPrivateField(data, "m_speed", speed);
            SetPrivateField(data, "m_maxHitPoints", maxHitPoints);
            SetPrivateField(data, "m_hurtLayer", hurtLayer);
            SetPrivateField(data, "m_detectionBoxSize", detectionBoxSize ?? new Vector2(10f, 4f));
            SetPrivateField(data, "m_detectionBoxOffset", detectionBoxOffset);
            SetPrivateField(data, "m_detectionRange", detectionRange);
            SetPrivateField(data, "m_detectionTargetLayer", detectionTargetLayer);
            SetPrivateField(dataSo, "m_enemyData", data);

            var enemyGo = new GameObject("EnemyTest") { layer = layer };
            m_createdObjects.Add(enemyGo);

            enemyGo.AddComponent<SpriteRenderer>();
            var composer = enemyGo.AddComponent<EnemyComposer>();
            var rb = enemyGo.GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;

            SetPrivateField(composer, "m_enemyData", dataSo);
            SetPrivateField(composer, "m_brainProfile", brainProfile);
            return (enemyGo, composer, dataSo);
        }

        private void TrackClonedData(EnemyComposer composer, EnemyDataScriptableObject originalSo)
        {
            var clonedSo = GetPrivateField<Object>(composer, "m_enemyData");
            if (clonedSo != null && clonedSo != originalSo)
            {
                m_createdObjects.Add(clonedSo);
            }
        }

        [Test]
        public void SetController_DrivesVelocityWithSpeedMultiplier_AndFlipsSprite()
        {
            var (enemyGo, composer, dataSo) = CreateEnemy(speed: 6f, maxHitPoints: 4);
            var controller = new StubEnemyController();

            InvokePrivateMethod(composer, "Awake");
            TrackClonedData(composer, dataSo);
            InvokePrivateMethod(composer, "OnEnable");

            composer.SetController(controller);
            Assert.AreEqual(1, controller.EnableCount);

            var rb = enemyGo.GetComponent<Rigidbody2D>();
            var spriteRenderer = enemyGo.GetComponent<SpriteRenderer>();

            controller.NextIntent = new EnemyIntent
            {
                MoveAxis = new Vector2(-1f, 0f),
                SpeedMultiplier = 0.5f
            };

            InvokePrivateMethod(composer, "Update");
            InvokePrivateMethod(composer, "FixedUpdate");

            Assert.AreEqual(1, controller.SampleCount);
            Assert.AreEqual(-3f, rb.linearVelocity.x, 0.001f);
            Assert.IsTrue(spriteRenderer.flipX);
            Assert.IsFalse(controller.LastBodyState.IsDead);
            Assert.AreEqual(1f, controller.LastBodyState.HealthNormalized, 0.001f);

            controller.NextIntent = new EnemyIntent
            {
                MoveAxis = new Vector2(1f, 0f),
                SpeedMultiplier = 1.5f
            };

            InvokePrivateMethod(composer, "Update");
            InvokePrivateMethod(composer, "FixedUpdate");

            Assert.AreEqual(9f, rb.linearVelocity.x, 0.001f);
            Assert.IsFalse(spriteRenderer.flipX);

            InvokePrivateMethod(composer, "OnDisable");
            Assert.AreEqual(1, controller.DisableCount);
        }

        private static void RunWithScriptPhysics2D(Action action)
        {
            SimulationMode2D previousSimMode = Physics2D.simulationMode;
            Physics2D.simulationMode = SimulationMode2D.Script;
            try
            {
                action();
            }
            finally
            {
                Physics2D.simulationMode = previousSimMode;
            }
        }

        private static void SyncAndStepPhysics2D()
        {
            Physics2D.SyncTransforms();
            Physics2D.Simulate(0.02f);
        }

        private BoxCollider2D AttachDetectionZone(GameObject enemyGo, EnemyComposer composer, Vector2 size, int zoneLayer = 9)
        {
            var zoneGo = new GameObject("DetectionZone") { layer = zoneLayer };
            m_createdObjects.Add(zoneGo);
            zoneGo.transform.SetParent(enemyGo.transform, false);
            var zoneCollider = zoneGo.AddComponent<BoxCollider2D>();
            zoneCollider.size = size;
            SetPrivateField(composer, "m_detectionCollider", zoneCollider);
            return zoneCollider;
        }

        [Test]
        public void DetectionZone_TargetEnterAndExit_AdvancesStateMachineBrain_AndIgnoresInnerHitboxExit()
        {
            const int heroLayer = 3;
            const int enemyLayer = 7;

            var profile = ScriptableObject.CreateInstance<BrainProfileScriptableObject>();
            m_createdObjects.Add(profile);
            profile.Configure(
                "Idle",
                new List<AIStateDefinition>
                {
                    new("Idle", new List<IAIBehavior> { new IdleBehavior() }, new List<AITransitionDefinition>
                    {
                        new("Combat", new TargetEnteredZoneCondition())
                    }),
                    new("Combat", new List<IAIBehavior> { new MoveTowardTargetBehavior(1f, 0.5f) }, new List<AITransitionDefinition>
                    {
                        new("Hunting", new TargetExitedZoneCondition())
                    }),
                    new("Hunting", new List<IAIBehavior> { new MoveToLastKnownPositionBehavior(1.5f, 0.25f) }, new List<AITransitionDefinition>
                    {
                        new("Combat", new TargetEnteredZoneCondition())
                    })
                });

            RunWithScriptPhysics2D(() =>
            {
                var (enemyGo, composer, dataSo) = CreateEnemy(
                    speed: 4f,
                    maxHitPoints: 2,
                    brainProfile: profile,
                    layer: enemyLayer,
                    detectionTargetLayer: (LayerMask)(1 << heroLayer));
                enemyGo.transform.position = Vector3.zero;

                BoxCollider2D detectionCollider = AttachDetectionZone(enemyGo, composer, new Vector2(20f, 6f));

                var hitboxGo = new GameObject("Hitbox") { layer = enemyLayer };
                m_createdObjects.Add(hitboxGo);
                hitboxGo.transform.SetParent(enemyGo.transform, false);
                var hitboxCollider = hitboxGo.AddComponent<BoxCollider2D>();
                hitboxCollider.isTrigger = true;
                hitboxCollider.size = new Vector2(3f, 3f);

                InvokePrivateMethod(composer, "Awake");
                Assert.IsTrue(detectionCollider.isTrigger);
                TrackClonedData(composer, dataSo);
                InvokePrivateMethod(composer, "OnEnable");

                var heroGo = new GameObject("HeroTarget") { layer = heroLayer };
                m_createdObjects.Add(heroGo);
                heroGo.transform.position = new Vector3(8f, 0f, 0f);
                var heroCollider = heroGo.AddComponent<BoxCollider2D>();
                heroCollider.size = Vector2.one;

                var rb = enemyGo.GetComponent<Rigidbody2D>();
                var spriteRenderer = enemyGo.GetComponent<SpriteRenderer>();

                Assert.AreEqual("Idle", composer.CurrentBrainStateId);

                // Enter DetectionZone -> Idle -> Combat and moves toward hero (+X).
                SyncAndStepPhysics2D();
                Assert.IsTrue(detectionCollider.IsTouching(heroCollider));
                Assert.IsFalse(hitboxCollider.IsTouching(heroCollider));
                InvokePrivateMethod(composer, "OnTriggerEnter2D", heroCollider);
                InvokePrivateMethod(composer, "Update");
                InvokePrivateMethod(composer, "FixedUpdate");

                Assert.AreEqual("Combat", composer.CurrentBrainStateId);
                Assert.AreEqual(4f, rb.linearVelocity.x, 0.001f);
                Assert.IsFalse(spriteRenderer.flipX);

                // Touch inner Hitbox while already inside DetectionZone -> does not duplicate tracked target.
                heroGo.transform.position = enemyGo.transform.position + new Vector3(1f, 0f, 0f);
                SyncAndStepPhysics2D();
                Assert.IsTrue(hitboxCollider.IsTouching(heroCollider));
                Assert.IsTrue(detectionCollider.IsTouching(heroCollider));
                InvokePrivateMethod(composer, "OnTriggerEnter2D", heroCollider);
                Assert.AreEqual(1, GetPrivateField<List<Collider2D>>(composer, "m_targetsInZone").Count);

                // Exit inner Hitbox while still touching DetectionZone -> stays in Combat (no false Hunting).
                heroGo.transform.position = enemyGo.transform.position + new Vector3(5f, 0f, 0f);
                SyncAndStepPhysics2D();
                Assert.IsFalse(hitboxCollider.IsTouching(heroCollider));
                Assert.IsTrue(detectionCollider.IsTouching(heroCollider));
                InvokePrivateMethod(composer, "OnTriggerExit2D", heroCollider);
                InvokePrivateMethod(composer, "Update");

                Assert.AreEqual("Combat", composer.CurrentBrainStateId);
                Assert.AreEqual(1, GetPrivateField<List<Collider2D>>(composer, "m_targetsInZone").Count);

                // Exit DetectionZone to the left -> Combat -> Hunting (1.5x speed toward -X).
                heroGo.transform.position = enemyGo.transform.position + new Vector3(-15f, 0f, 0f);
                SyncAndStepPhysics2D();
                Assert.IsFalse(detectionCollider.IsTouching(heroCollider));
                InvokePrivateMethod(composer, "OnTriggerExit2D", heroCollider);
                InvokePrivateMethod(composer, "Update");
                InvokePrivateMethod(composer, "FixedUpdate");

                Assert.AreEqual("Hunting", composer.CurrentBrainStateId);
                Assert.AreEqual(-6f, rb.linearVelocity.x, 0.001f);
                Assert.IsTrue(spriteRenderer.flipX);

                // Re-enter DetectionZone -> Hunting -> Combat.
                heroGo.transform.position = enemyGo.transform.position + new Vector3(5f, 0f, 0f);
                SyncAndStepPhysics2D();
                Assert.IsTrue(detectionCollider.IsTouching(heroCollider));
                InvokePrivateMethod(composer, "OnTriggerEnter2D", heroCollider);
                InvokePrivateMethod(composer, "Update");

                Assert.AreEqual("Combat", composer.CurrentBrainStateId);

                InvokePrivateMethod(composer, "OnDisable");
                Assert.AreEqual(0, GetPrivateField<List<Collider2D>>(composer, "m_targetsInZone").Count);
            });
        }

        [Test]
        public void OnTriggerEnter2D_DamagesOnlyOnRootBoxColliderAndHurtLayer_AndDeactivatesAtZeroHp()
        {
            const int enemyLayer = 7;
            const int hurtLayer = 8;
            const int nonHurtLayer = 3;

            RunWithScriptPhysics2D(() =>
            {
                var (enemyGo, composer, dataSo) = CreateEnemy(
                    speed: 5f,
                    maxHitPoints: 2,
                    hurtLayer: (LayerMask)(1 << hurtLayer),
                    layer: enemyLayer);

                enemyGo.transform.position = Vector3.zero;
                var rootBox = enemyGo.GetComponent<BoxCollider2D>();
                rootBox.size = Vector2.one;

                var childGo = new GameObject("ChildCollider") { layer = enemyLayer };
                m_createdObjects.Add(childGo);
                childGo.transform.SetParent(enemyGo.transform, false);
                childGo.transform.localPosition = new Vector3(5f, 0f, 0f);
                var childCollider = childGo.AddComponent<BoxCollider2D>();
                childCollider.isTrigger = true;
                childCollider.size = Vector2.one;

                var childHurtGo = new GameObject("ChildHurtLayerCollider") { layer = hurtLayer };
                m_createdObjects.Add(childHurtGo);
                childHurtGo.transform.SetParent(enemyGo.transform, false);
                childHurtGo.transform.localPosition = Vector3.zero;
                var childHurtCollider = childHurtGo.AddComponent<BoxCollider2D>();
                childHurtCollider.isTrigger = true;
                childHurtCollider.size = Vector2.one;

                var hurtGo = new GameObject("HeroAttack") { layer = hurtLayer };
                m_createdObjects.Add(hurtGo);
                hurtGo.transform.position = new Vector3(5f, 0f, 0f);
                var hurtCollider = hurtGo.AddComponent<BoxCollider2D>();
                hurtCollider.isTrigger = true;
                hurtCollider.size = Vector2.one;

                var nonHurtGo = new GameObject("NonHurtCollider") { layer = nonHurtLayer };
                m_createdObjects.Add(nonHurtGo);
                nonHurtGo.transform.position = Vector3.zero;
                var nonHurtCollider = nonHurtGo.AddComponent<BoxCollider2D>();
                nonHurtCollider.isTrigger = true;
                nonHurtCollider.size = Vector2.one;

                InvokePrivateMethod(composer, "Awake");
                TrackClonedData(composer, dataSo);
                InvokePrivateMethod(composer, "OnEnable");

                var health = GetPrivateField<HealthComponent>(composer, "m_healthComponent");
                var rb = enemyGo.GetComponent<Rigidbody2D>();

                SyncAndStepPhysics2D();

                Assert.IsTrue(childCollider.IsTouching(hurtCollider), "Expected child collider to overlap hurt collider.");
                Assert.IsFalse(rootBox.IsTouching(hurtCollider), "Expected root BoxCollider2D not to overlap hurt collider.");

                // Child collider overlap must not trigger self-damage.
                InvokePrivateMethod(composer, "OnTriggerEnter2D", hurtCollider);
                Assert.AreEqual(2, health.CurrentHitPoints);
                Assert.IsTrue(enemyGo.activeSelf);

                // Own child collider even on HurtLayer must be ignored.
                InvokePrivateMethod(composer, "OnTriggerEnter2D", childHurtCollider);
                Assert.AreEqual(2, health.CurrentHitPoints);
                Assert.IsTrue(enemyGo.activeSelf);

                // Non-HurtLayer overlap on root BoxCollider2D must be ignored.
                Assert.IsTrue(rootBox.IsTouching(nonHurtCollider));
                InvokePrivateMethod(composer, "OnTriggerEnter2D", nonHurtCollider);
                Assert.AreEqual(2, health.CurrentHitPoints);
                Assert.IsTrue(enemyGo.activeSelf);

                // Move HurtLayer collider onto root BoxCollider2D -> decrements HP to 1, stays active.
                hurtGo.transform.position = Vector3.zero;
                SyncAndStepPhysics2D();

                Assert.IsTrue(rootBox.IsTouching(hurtCollider));
                InvokePrivateMethod(composer, "OnTriggerEnter2D", hurtCollider);
                Assert.AreEqual(1, health.CurrentHitPoints);
                Assert.IsFalse(health.IsDead);
                Assert.IsTrue(enemyGo.activeSelf);

                // Second HurtLayer hit -> 0 HP, zeroes velocity, deactivates GameObject.
                rb.linearVelocity = new Vector2(5f, 0f);
                InvokePrivateMethod(composer, "OnTriggerEnter2D", hurtCollider);
                Assert.AreEqual(0, health.CurrentHitPoints);
                Assert.IsTrue(health.IsDead);
                Assert.AreEqual(Vector2.zero, rb.linearVelocity);
                Assert.IsFalse(enemyGo.activeSelf);
            });
        }

        [Test]
        public void DetectionZone_EnterAndExit_PopulatesAndClearsEnemyBodyState()
        {
            const int heroLayer = 3;
            const int enemyLayer = 7;

            RunWithScriptPhysics2D(() =>
            {
                var (enemyGo, composer, dataSo) = CreateEnemy(
                    layer: enemyLayer,
                    detectionRange: 5f,
                    detectionTargetLayer: (LayerMask)(1 << heroLayer));
                enemyGo.transform.position = Vector3.zero;

                BoxCollider2D detectionCollider = AttachDetectionZone(enemyGo, composer, new Vector2(12f, 4f));
                var controller = new StubEnemyController();

                InvokePrivateMethod(composer, "Awake");
                TrackClonedData(composer, dataSo);
                InvokePrivateMethod(composer, "OnEnable");
                composer.SetController(controller);

                var heroGo = new GameObject("HeroTarget") { layer = heroLayer };
                m_createdObjects.Add(heroGo);
                heroGo.transform.position = new Vector3(3f, 0f, 0f);
                var heroCollider = heroGo.AddComponent<BoxCollider2D>();
                heroCollider.size = Vector2.one;

                SyncAndStepPhysics2D();
                Assert.IsTrue(detectionCollider.IsTouching(heroCollider));
                InvokePrivateMethod(composer, "OnTriggerEnter2D", heroCollider);
                InvokePrivateMethod(composer, "Update");

                Assert.IsTrue(controller.LastBodyState.HasDetectedTarget);
                Assert.AreEqual(3f, controller.LastBodyState.DetectedTargetPosition.x, 0.001f);
                Assert.AreEqual(0f, controller.LastBodyState.DetectedTargetPosition.y, 0.001f);
                Assert.AreEqual(3f, controller.LastBodyState.DetectedTargetDistance, 0.001f);
                Assert.LessOrEqual(controller.LastBodyState.DetectedTargetDistance, controller.LastBodyState.DetectionRange);
                Assert.AreEqual(5f, controller.LastBodyState.DetectionRange, 0.001f);

                heroGo.transform.position = new Vector3(15f, 0f, 0f);
                SyncAndStepPhysics2D();
                Assert.IsFalse(detectionCollider.IsTouching(heroCollider));
                InvokePrivateMethod(composer, "OnTriggerExit2D", heroCollider);
                InvokePrivateMethod(composer, "Update");

                Assert.IsFalse(controller.LastBodyState.HasDetectedTarget);
                Assert.IsTrue(float.IsPositiveInfinity(controller.LastBodyState.DetectedTargetDistance));
            });
        }

        [Test]
        public void SetDetectionRangeMultiplier_DropsDetectedHero_WhenEffectiveRangeShrinksBelowDistance()
        {
            const int heroLayer = 3;
            const int enemyLayer = 7;

            RunWithScriptPhysics2D(() =>
            {
                var (enemyGo, composer, dataSo) = CreateEnemy(
                    layer: enemyLayer,
                    detectionRange: 6f,
                    detectionTargetLayer: (LayerMask)(1 << heroLayer));
                enemyGo.transform.position = Vector3.zero;

                BoxCollider2D detectionCollider = AttachDetectionZone(enemyGo, composer, new Vector2(12f, 4f));
                var controller = new StubEnemyController();

                InvokePrivateMethod(composer, "Awake");
                TrackClonedData(composer, dataSo);
                InvokePrivateMethod(composer, "OnEnable");
                composer.SetController(controller);

                var heroGo = new GameObject("HeroTarget") { layer = heroLayer };
                m_createdObjects.Add(heroGo);
                heroGo.transform.position = new Vector3(3f, 0f, 0f);
                var heroCollider = heroGo.AddComponent<BoxCollider2D>();
                heroCollider.size = Vector2.one;

                SyncAndStepPhysics2D();
                Assert.IsTrue(detectionCollider.IsTouching(heroCollider));
                InvokePrivateMethod(composer, "OnTriggerEnter2D", heroCollider);
                InvokePrivateMethod(composer, "Update");
                Assert.IsTrue(controller.LastBodyState.HasDetectedTarget);
                Assert.AreEqual(6f, controller.LastBodyState.DetectionRange, 0.001f);

                // Visibility debuff shrinks effective range to 1.5f (below hero distance of 3f).
                composer.SetDetectionRangeMultiplier(0.25f);
                Assert.AreEqual(1.5f, composer.EffectiveDetectionRange, 0.001f);

                InvokePrivateMethod(composer, "Update");
                Assert.IsFalse(controller.LastBodyState.HasDetectedTarget);
                Assert.AreEqual(1.5f, controller.LastBodyState.DetectionRange, 0.001f);
                Assert.IsTrue(float.IsPositiveInfinity(controller.LastBodyState.DetectedTargetDistance));
            });
        }

        [Test]
        public void SetDetectionTargetLayer_PrunesPreviousTargets_DetectsOtherEnemy_AndIgnoresSelfAndChildColliders()
        {
            const int heroLayer = 3;
            const int enemyLayer = 7;

            RunWithScriptPhysics2D(() =>
            {
                var (enemyGo, composer, dataSo) = CreateEnemy(
                    layer: enemyLayer,
                    detectionRange: 6f,
                    detectionTargetLayer: (LayerMask)(1 << heroLayer));
                enemyGo.transform.position = Vector3.zero;

                var childEnemyColliderGo = new GameObject("ChildEnemyCollider") { layer = enemyLayer };
                m_createdObjects.Add(childEnemyColliderGo);
                childEnemyColliderGo.transform.SetParent(enemyGo.transform, false);
                childEnemyColliderGo.transform.localPosition = new Vector3(0.5f, 0f, 0f);
                var childCollider = childEnemyColliderGo.AddComponent<BoxCollider2D>();
                childCollider.isTrigger = true;
                childCollider.size = Vector2.one;

                BoxCollider2D detectionCollider = AttachDetectionZone(enemyGo, composer, new Vector2(12f, 4f));
                var controller = new StubEnemyController();

                InvokePrivateMethod(composer, "Awake");
                TrackClonedData(composer, dataSo);
                InvokePrivateMethod(composer, "OnEnable");
                composer.SetController(controller);

                var heroGo = new GameObject("HeroTarget") { layer = heroLayer };
                m_createdObjects.Add(heroGo);
                heroGo.transform.position = new Vector3(2f, 0f, 0f);
                var heroCollider = heroGo.AddComponent<BoxCollider2D>();
                heroCollider.size = Vector2.one;

                var otherEnemyGo = new GameObject("OtherEnemy") { layer = enemyLayer };
                m_createdObjects.Add(otherEnemyGo);
                otherEnemyGo.transform.position = new Vector3(4f, 0f, 0f);
                var otherEnemyCollider = otherEnemyGo.AddComponent<BoxCollider2D>();
                otherEnemyCollider.size = Vector2.one;

                SyncAndStepPhysics2D();
                Assert.IsTrue(detectionCollider.IsTouching(heroCollider));
                Assert.IsTrue(detectionCollider.IsTouching(otherEnemyCollider));

                // Hero enters while target layer is Hero; otherEnemy is ignored by hero-only mask.
                InvokePrivateMethod(composer, "OnTriggerEnter2D", heroCollider);
                InvokePrivateMethod(composer, "OnTriggerEnter2D", otherEnemyCollider);
                InvokePrivateMethod(composer, "Update");
                Assert.IsTrue(controller.LastBodyState.HasDetectedTarget);
                Assert.AreEqual(2f, controller.LastBodyState.DetectedTargetDistance, 0.001f);

                // Switch target layer to Enemy -> prunes Hero from m_targetsInZone.
                composer.SetDetectionTargetLayer(1 << enemyLayer);
                Assert.AreEqual((LayerMask)(1 << enemyLayer), composer.DetectionTargetLayer);
                Assert.AreEqual(0, GetPrivateField<List<Collider2D>>(composer, "m_targetsInZone").Count);

                InvokePrivateMethod(composer, "Update");
                Assert.IsFalse(controller.LastBodyState.HasDetectedTarget);

                // Self root and child colliders on enemyLayer must remain ignored.
                InvokePrivateMethod(composer, "OnTriggerEnter2D", enemyGo.GetComponent<BoxCollider2D>());
                InvokePrivateMethod(composer, "OnTriggerEnter2D", childCollider);
                InvokePrivateMethod(composer, "Update");
                Assert.IsFalse(controller.LastBodyState.HasDetectedTarget);

                // Another enemy touching DetectionZone on enemyLayer is detected.
                InvokePrivateMethod(composer, "OnTriggerEnter2D", otherEnemyCollider);
                InvokePrivateMethod(composer, "Update");
                Assert.IsTrue(controller.LastBodyState.HasDetectedTarget);
                Assert.AreEqual(4f, controller.LastBodyState.DetectedTargetPosition.x, 0.001f);
                Assert.AreEqual(0f, controller.LastBodyState.DetectedTargetPosition.y, 0.001f);
                Assert.AreEqual(4f, controller.LastBodyState.DetectedTargetDistance, 0.001f);

                // Exiting DetectionZone clears detection.
                otherEnemyGo.transform.position = new Vector3(15f, 0f, 0f);
                SyncAndStepPhysics2D();
                Assert.IsFalse(detectionCollider.IsTouching(otherEnemyCollider));
                InvokePrivateMethod(composer, "OnTriggerExit2D", otherEnemyCollider);
                InvokePrivateMethod(composer, "Update");
                Assert.IsFalse(controller.LastBodyState.HasDetectedTarget);
            });
        }
    }
}
