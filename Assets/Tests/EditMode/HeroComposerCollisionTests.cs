using System.Collections.Generic;
using System.Reflection;
using Com.Voobox.Framework.Events;
using Com.Voobox.Project.Data;
using Com.Voobox.Project.Entity;
using Com.Voobox.Project.Events.HeroEvents;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Com.Voobox.Project.Tests.EditMode
{
    public class HeroComposerCollisionTests
    {
        private const int HERO_LAYER = 3;
        private const int ENEMY_LAYER = 7;
        private const int ATTACK_AREA_LAYER = 8;
        private const int SENSOR_LAYER = 9;
        private const string HERO_PREFAB_PATH = "Assets/_Project/Prefabs/Units/Hero.prefab";

        private readonly List<Object> m_createdObjects = new();
        private SimulationMode2D m_previousSimMode;

        [SetUp]
        public void SetUp()
        {
            m_previousSimMode = Physics2D.simulationMode;
            Physics2D.simulationMode = SimulationMode2D.Script;
        }

        [TearDown]
        public void TearDown()
        {
            Physics2D.simulationMode = m_previousSimMode;

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

        private (GameObject heroGo, HeroComposer heroComposer, BoxCollider2D rootBox, BoxCollider2D hurtboxCollider) CreateHeroWithHurtbox(
            Vector2? hurtboxOffset = null,
            bool hurtboxTriggerInitially = true)
        {
            var heroDataSo = ScriptableObject.CreateInstance<HeroDataScriptableObject>();
            m_createdObjects.Add(heroDataSo);

            var heroData = new HeroData();
            SetPrivateField(heroData, "m_maxHitPoints", 1);
            SetPrivateField(heroData, "m_hurtLayer", (LayerMask)(1 << ENEMY_LAYER));
            SetPrivateField(heroDataSo, "m_heroData", heroData);

            var heroGo = new GameObject("HeroTest") { layer = HERO_LAYER };
            m_createdObjects.Add(heroGo);
            heroGo.transform.position = Vector3.zero;

            var heroComposer = heroGo.AddComponent<HeroComposer>();
            var heroRb = heroGo.GetComponent<Rigidbody2D>();
            heroRb.bodyType = RigidbodyType2D.Dynamic;
            heroRb.gravityScale = 0f;

            var rootBox = heroGo.GetComponent<BoxCollider2D>();
            rootBox.size = Vector2.one;

            var hurtboxGo = new GameObject("Hurtbox") { layer = SENSOR_LAYER };
            m_createdObjects.Add(hurtboxGo);
            hurtboxGo.transform.SetParent(heroGo.transform, false);
            var hurtboxCollider = hurtboxGo.AddComponent<BoxCollider2D>();
            hurtboxCollider.isTrigger = hurtboxTriggerInitially;
            hurtboxCollider.offset = hurtboxOffset ?? Vector2.zero;
            hurtboxCollider.size = Vector2.one;

            SetPrivateField(heroComposer, "m_heroData", heroDataSo);
            SetPrivateField(heroComposer, "m_hurtboxCollider", hurtboxCollider);

            InvokePrivateMethod(heroComposer, "Awake");
            var clonedSo = GetPrivateField<Object>(heroComposer, "m_heroData");
            if (clonedSo != null && clonedSo != heroDataSo)
            {
                m_createdObjects.Add(clonedSo);
            }

            return (heroGo, heroComposer, rootBox, hurtboxCollider);
        }

        private static void SyncAndSimulate()
        {
            Physics2D.SyncTransforms();
            Physics2D.Simulate(0.02f);
        }

        [Test]
        public void OnTriggerEnter2D_ResolvesHurtLayerInHierarchy_AndIgnoresUnrelatedColliders()
        {
            var (_, heroComposer, _, hurtboxCollider) = CreateHeroWithHurtbox();

            var unrelatedGo = new GameObject("UnrelatedHeroLayer") { layer = HERO_LAYER };
            m_createdObjects.Add(unrelatedGo);
            unrelatedGo.transform.position = Vector3.zero;
            var unrelatedCollider = unrelatedGo.AddComponent<BoxCollider2D>();
            unrelatedCollider.isTrigger = true;
            unrelatedCollider.size = Vector2.one;

            var enemyParentGo = new GameObject("EnemyParent") { layer = ENEMY_LAYER };
            m_createdObjects.Add(enemyParentGo);
            enemyParentGo.transform.position = Vector3.zero;

            var enemyChildNonEnemyLayerGo = new GameObject("EnemyChildCollider") { layer = HERO_LAYER };
            m_createdObjects.Add(enemyChildNonEnemyLayerGo);
            enemyChildNonEnemyLayerGo.transform.SetParent(enemyParentGo.transform, false);
            var enemyChildCollider = enemyChildNonEnemyLayerGo.AddComponent<BoxCollider2D>();
            enemyChildCollider.isTrigger = true;
            enemyChildCollider.size = Vector2.one;

            int heroDiedCount = 0;
            void OnHeroDied(HeroDiedEvent _) => heroDiedCount++;

            EventBus.Subscribe<HeroDiedEvent>(OnHeroDied);
            try
            {
                SyncAndSimulate();

                Assert.IsTrue(hurtboxCollider.IsTouching(unrelatedCollider), "Expected Hurtbox (Sensor 9) to touch unrelated Hero (3) collider.");
                InvokePrivateMethod(heroComposer, "OnTriggerEnter2D", unrelatedCollider);
                Assert.AreEqual(0, heroDiedCount, "Collider without HurtLayer in its hierarchy must not damage Hero.");

                Assert.IsTrue(hurtboxCollider.IsTouching(enemyChildCollider), "Expected Hurtbox to touch child collider under Enemy parent.");
                InvokePrivateMethod(heroComposer, "OnTriggerEnter2D", enemyChildCollider);
                Assert.AreEqual(1, heroDiedCount, "Child collider under Enemy parent must resolve HurtLayer and damage Hero.");
            }
            finally
            {
                EventBus.Unsubscribe<HeroDiedEvent>(OnHeroDied);
            }
        }

        [Test]
        public void OnTriggerEnter2D_DamagesOnEnemyHitboxChild()
        {
            var (_, heroComposer, heroBoxCollider, hurtboxCollider) = CreateHeroWithHurtbox();

            var enemyGo = new GameObject("EnemyRoot") { layer = ENEMY_LAYER };
            m_createdObjects.Add(enemyGo);
            enemyGo.transform.position = Vector3.zero;

            var hitboxGo = new GameObject("Hitbox") { layer = ENEMY_LAYER };
            m_createdObjects.Add(hitboxGo);
            hitboxGo.transform.SetParent(enemyGo.transform, false);
            var hitboxCollider = hitboxGo.AddComponent<BoxCollider2D>();
            hitboxCollider.isTrigger = true;
            hitboxCollider.size = Vector2.one;

            int heroDiedCount = 0;
            void OnHeroDied(HeroDiedEvent _) => heroDiedCount++;

            EventBus.Subscribe<HeroDiedEvent>(OnHeroDied);
            try
            {
                SyncAndSimulate();

                Assert.IsFalse(heroBoxCollider.IsTouching(hitboxCollider));
                Assert.IsTrue(hurtboxCollider.IsTouching(hitboxCollider));

                InvokePrivateMethod(heroComposer, "OnTriggerEnter2D", hitboxCollider);
                Assert.AreEqual(1, heroDiedCount, "Enemy Hitbox child overlap on Hurtbox should damage Hero.");
            }
            finally
            {
                EventBus.Unsubscribe<HeroDiedEvent>(OnHeroDied);
            }
        }

        [Test]
        public void CheckDamage_UsesHurtboxOverlapWithOffset_AndNotifiesHeroDiedEventOnce()
        {
            var offset = new Vector2(2f, 0f);
            var (_, heroComposer, heroBoxCollider, hurtboxCollider) = CreateHeroWithHurtbox(hurtboxOffset: offset);

            // Overlapping Sensor DetectionZone (with Enemy parent) and unrelated Hero-layer collider at offset must be ignored.
            var enemyForZoneGo = new GameObject("EnemyWithZone") { layer = ENEMY_LAYER };
            m_createdObjects.Add(enemyForZoneGo);
            enemyForZoneGo.transform.position = new Vector3(2f, 0f, 0f);
            var detectionZoneGo = new GameObject("DetectionZone") { layer = SENSOR_LAYER };
            m_createdObjects.Add(detectionZoneGo);
            detectionZoneGo.transform.SetParent(enemyForZoneGo.transform, false);
            var detectionZoneCollider = detectionZoneGo.AddComponent<BoxCollider2D>();
            detectionZoneCollider.isTrigger = true;
            detectionZoneCollider.size = Vector2.one;

            var unrelatedGo = new GameObject("Unrelated") { layer = HERO_LAYER };
            m_createdObjects.Add(unrelatedGo);
            unrelatedGo.transform.position = new Vector3(2f, 0f, 0f);
            var unrelatedCollider = unrelatedGo.AddComponent<BoxCollider2D>();
            unrelatedCollider.isTrigger = true;
            unrelatedCollider.size = Vector2.one;

            // Enemy at Hero root (0,0) does not overlap offset Hurtbox (2,0).
            var enemyAtRootGo = new GameObject("EnemyAtRoot") { layer = ENEMY_LAYER };
            m_createdObjects.Add(enemyAtRootGo);
            enemyAtRootGo.transform.position = Vector3.zero;
            var enemyAtRootCollider = enemyAtRootGo.AddComponent<BoxCollider2D>();
            enemyAtRootCollider.isTrigger = true;
            enemyAtRootCollider.size = Vector2.one;

            int heroDiedCount = 0;
            void OnHeroDied(HeroDiedEvent _) => heroDiedCount++;

            EventBus.Subscribe<HeroDiedEvent>(OnHeroDied);
            try
            {
                SyncAndSimulate();

                Assert.IsFalse(hurtboxCollider.IsTouching(detectionZoneCollider));
                Assert.IsTrue(hurtboxCollider.IsTouching(unrelatedCollider));
                Assert.IsFalse(hurtboxCollider.IsTouching(enemyAtRootCollider));

                InvokePrivateMethod(heroComposer, "CheckDamage");
                Assert.AreEqual(0, heroDiedCount, "CheckDamage should ignore DetectionZone, unrelated layers, and root-only non-Hurtbox overlaps.");

                // Move Enemy onto offset Hurtbox -> CheckDamage detects it and fires HeroDiedEvent once.
                enemyAtRootGo.transform.position = new Vector3(2f, 0f, 0f);
                SyncAndSimulate();

                Assert.IsFalse(heroBoxCollider.IsTouching(enemyAtRootCollider));
                Assert.IsTrue(hurtboxCollider.IsTouching(enemyAtRootCollider));

                InvokePrivateMethod(heroComposer, "CheckDamage");
                Assert.AreEqual(1, heroDiedCount, "CheckDamage should notify HeroDiedEvent exactly once on Hurtbox overlap.");
            }
            finally
            {
                EventBus.Unsubscribe<HeroDiedEvent>(OnHeroDied);
            }
        }

        [Test]
        public void CheckDamage_ResolvesParentHurtLayerOnOffsetHurtbox()
        {
            var offset = new Vector2(2f, 0f);
            var (_, heroComposer, _, hurtboxCollider) = CreateHeroWithHurtbox(hurtboxOffset: offset);

            var enemyParentGo = new GameObject("EnemyParent") { layer = ENEMY_LAYER };
            m_createdObjects.Add(enemyParentGo);
            enemyParentGo.transform.position = new Vector3(2f, 0f, 0f);

            var enemyChildGo = new GameObject("EnemyChild") { layer = HERO_LAYER };
            m_createdObjects.Add(enemyChildGo);
            enemyChildGo.transform.SetParent(enemyParentGo.transform, false);
            var enemyChildCollider = enemyChildGo.AddComponent<BoxCollider2D>();
            enemyChildCollider.isTrigger = true;
            enemyChildCollider.size = Vector2.one;

            int heroDiedCount = 0;
            void OnHeroDied(HeroDiedEvent _) => heroDiedCount++;

            EventBus.Subscribe<HeroDiedEvent>(OnHeroDied);
            try
            {
                SyncAndSimulate();

                Assert.IsTrue(hurtboxCollider.IsTouching(enemyChildCollider));
                InvokePrivateMethod(heroComposer, "CheckDamage");
                Assert.AreEqual(1, heroDiedCount, "CheckDamage should resolve HurtLayer across parent hierarchy.");
            }
            finally
            {
                EventBus.Unsubscribe<HeroDiedEvent>(OnHeroDied);
            }
        }

        [Test]
        public void HeroPrefab_HasSensorHurtboxChildWiredToHeroComposer()
        {
            var heroPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(HERO_PREFAB_PATH);
            Assert.IsNotNull(heroPrefab, $"Expected Hero prefab at '{HERO_PREFAB_PATH}'.");
            Assert.AreEqual(HERO_LAYER, heroPrefab.layer, "Hero prefab root should be on Hero layer (3).");

            var rootBox = heroPrefab.GetComponent<BoxCollider2D>();
            Assert.IsNotNull(rootBox, "Hero prefab root should have a BoxCollider2D.");
            Assert.IsFalse(rootBox.isTrigger, "Hero prefab root BoxCollider2D should be non-trigger.");

            var heroComposer = heroPrefab.GetComponent<HeroComposer>();
            Assert.IsNotNull(heroComposer, "Hero prefab root should have HeroComposer.");

            Transform hurtboxTransform = heroPrefab.transform.Find("Hurtbox");
            Assert.IsNotNull(hurtboxTransform, "Hero prefab should have a child GameObject named 'Hurtbox'.");

            GameObject hurtboxGo = hurtboxTransform.gameObject;
            Assert.AreEqual(SENSOR_LAYER, hurtboxGo.layer, "Hero/Hurtbox should be on Sensor layer (9).");
            Assert.AreEqual(0, hurtboxGo.GetComponents<MonoBehaviour>().Length, "Hero/Hurtbox must have zero MonoBehaviour components.");

            var hurtboxCollider = hurtboxGo.GetComponent<BoxCollider2D>();
            Assert.IsNotNull(hurtboxCollider, "Hero/Hurtbox should have a BoxCollider2D.");
            Assert.IsTrue(hurtboxCollider.isTrigger, "Hero/Hurtbox BoxCollider2D must be a trigger.");
            Assert.AreEqual(rootBox.offset, hurtboxCollider.offset, "Hero/Hurtbox BoxCollider2D offset must match root BoxCollider2D.");
            Assert.AreEqual(rootBox.size, hurtboxCollider.size, "Hero/Hurtbox BoxCollider2D size must match root BoxCollider2D.");

            Collider2D wiredHurtbox = GetPrivateField<Collider2D>(heroComposer, "m_hurtboxCollider");
            Assert.AreSame(hurtboxCollider, wiredHurtbox, "HeroComposer.m_hurtboxCollider must be wired to Hero/Hurtbox BoxCollider2D.");
        }
    }
}
