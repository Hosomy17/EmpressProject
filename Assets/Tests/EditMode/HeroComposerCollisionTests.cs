using System.Reflection;
using Com.Voobox.Framework.Events;
using Com.Voobox.Project.Data;
using Com.Voobox.Project.Entity;
using Com.Voobox.Project.Events.HeroEvents;
using NUnit.Framework;
using UnityEngine;

namespace Com.Voobox.Project.Tests.EditMode
{
    public class HeroComposerCollisionTests
    {
        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Field '{fieldName}' not found on {target.GetType().Name}.");
            field.SetValue(target, value);
        }

        private static void InvokePrivateMethod(object target, string methodName, params object[] args)
        {
            var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, $"Method '{methodName}' not found on {target.GetType().Name}.");
            method.Invoke(target, args);
        }

        [Test]
        public void OnTriggerEnter2D_IgnoresChildColliderOverlap_AndDamagesOnRootColliderOverlap()
        {
            const int heroLayer = 3;
            const int enemyLayer = 7;
            const int attackAreaLayer = 8;

            var previousSimMode = Physics2D.simulationMode;
            Physics2D.simulationMode = SimulationMode2D.Script;

            var heroDataSo = ScriptableObject.CreateInstance<HeroDataScriptableObject>();
            var heroData = new HeroData();
            SetPrivateField(heroData, "m_maxHitPoints", 1);
            SetPrivateField(heroData, "m_hurtLayer", (LayerMask)(1 << enemyLayer));
            SetPrivateField(heroDataSo, "m_heroData", heroData);

            var heroGo = new GameObject("HeroTest") { layer = heroLayer };
            heroGo.transform.position = Vector3.zero;
            var heroComposer = heroGo.AddComponent<HeroComposer>();
            var heroRb = heroGo.GetComponent<Rigidbody2D>();
            heroRb.bodyType = RigidbodyType2D.Dynamic;
            heroRb.gravityScale = 0f;
            var heroBoxCollider = heroGo.GetComponent<BoxCollider2D>();
            heroBoxCollider.size = Vector2.one;
            SetPrivateField(heroComposer, "m_heroData", heroDataSo);

            var childAttackGo = new GameObject("AttackRightArea") { layer = attackAreaLayer };
            childAttackGo.transform.SetParent(heroGo.transform, false);
            childAttackGo.transform.localPosition = new Vector3(5f, 0f, 0f);
            var childAttackCollider = childAttackGo.AddComponent<BoxCollider2D>();
            childAttackCollider.isTrigger = true;
            childAttackCollider.size = Vector2.one;

            var enemyGo = new GameObject("EnemyTest") { layer = enemyLayer };
            enemyGo.transform.position = new Vector3(5f, 0f, 0f);
            var enemyCollider = enemyGo.AddComponent<BoxCollider2D>();
            enemyCollider.isTrigger = true;
            enemyCollider.size = Vector2.one;

            int heroDiedCount = 0;
            void OnHeroDied(HeroDiedEvent _) => heroDiedCount++;

            EventBus.Subscribe<HeroDiedEvent>(OnHeroDied);
            try
            {
                InvokePrivateMethod(heroComposer, "Awake");

                Physics2D.SyncTransforms();
                Physics2D.Simulate(0.02f);

                Assert.IsTrue(childAttackCollider.IsTouching(enemyCollider), "Expected child AttackArea collider to touch enemy.");
                Assert.IsFalse(heroBoxCollider.IsTouching(enemyCollider), "Expected Hero root BoxCollider2D to not touch enemy.");

                // Unity forwards child trigger events to the parent Rigidbody2D; HeroComposer must ignore it.
                InvokePrivateMethod(heroComposer, "OnTriggerEnter2D", enemyCollider);
                Assert.AreEqual(0, heroDiedCount, "Child AttackArea overlap should not damage Hero.");

                // Move enemy onto Hero's root BoxCollider2D and re-simulate.
                enemyGo.transform.position = Vector3.zero;
                Physics2D.SyncTransforms();
                Physics2D.Simulate(0.02f);

                Assert.IsTrue(heroBoxCollider.IsTouching(enemyCollider), "Expected Hero root BoxCollider2D to touch enemy.");

                InvokePrivateMethod(heroComposer, "OnTriggerEnter2D", enemyCollider);
                Assert.AreEqual(1, heroDiedCount, "Root BoxCollider2D overlap should damage Hero.");
            }
            finally
            {
                EventBus.Unsubscribe<HeroDiedEvent>(OnHeroDied);
                Physics2D.simulationMode = previousSimMode;

                var clonedSo = typeof(HeroComposer)
                    .GetField("m_heroData", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.GetValue(heroComposer) as Object;
                if (clonedSo != null && clonedSo != heroDataSo)
                {
                    Object.DestroyImmediate(clonedSo);
                }

                Object.DestroyImmediate(heroGo);
                Object.DestroyImmediate(enemyGo);
                Object.DestroyImmediate(heroDataSo);
            }
        }

        [Test]
        public void CheckDamage_NotifiesHeroDiedEventOnce_WhenOverlappingHurtLayer()
        {
            const int heroLayer = 3;
            const int enemyLayer = 7;

            var previousSimMode = Physics2D.simulationMode;
            Physics2D.simulationMode = SimulationMode2D.Script;

            var heroDataSo = ScriptableObject.CreateInstance<HeroDataScriptableObject>();
            var heroData = new HeroData();
            SetPrivateField(heroData, "m_maxHitPoints", 1);
            SetPrivateField(heroData, "m_hurtLayer", (LayerMask)(1 << enemyLayer));
            SetPrivateField(heroDataSo, "m_heroData", heroData);

            var heroGo = new GameObject("HeroTest") { layer = heroLayer };
            heroGo.transform.position = Vector3.zero;
            var heroComposer = heroGo.AddComponent<HeroComposer>();
            var heroRb = heroGo.GetComponent<Rigidbody2D>();
            heroRb.bodyType = RigidbodyType2D.Dynamic;
            heroRb.gravityScale = 0f;
            var heroBoxCollider = heroGo.GetComponent<BoxCollider2D>();
            heroBoxCollider.size = Vector2.one;
            SetPrivateField(heroComposer, "m_heroData", heroDataSo);

            var enemyGo = new GameObject("EnemyTest") { layer = enemyLayer };
            enemyGo.transform.position = Vector3.zero;
            var enemyCollider = enemyGo.AddComponent<BoxCollider2D>();
            enemyCollider.isTrigger = true;
            enemyCollider.size = Vector2.one;

            int heroDiedCount = 0;
            void OnHeroDied(HeroDiedEvent _) => heroDiedCount++;

            EventBus.Subscribe<HeroDiedEvent>(OnHeroDied);
            try
            {
                InvokePrivateMethod(heroComposer, "Awake");

                Physics2D.SyncTransforms();
                Physics2D.Simulate(0.02f);

                Assert.IsTrue(heroBoxCollider.IsTouching(enemyCollider), "Expected Hero root BoxCollider2D to touch enemy.");

                InvokePrivateMethod(heroComposer, "CheckDamage");
                Assert.AreEqual(1, heroDiedCount, "CheckDamage should notify HeroDiedEvent exactly once on death.");
            }
            finally
            {
                EventBus.Unsubscribe<HeroDiedEvent>(OnHeroDied);
                Physics2D.simulationMode = previousSimMode;

                var clonedSo = typeof(HeroComposer)
                    .GetField("m_heroData", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.GetValue(heroComposer) as Object;
                if (clonedSo != null && clonedSo != heroDataSo)
                {
                    Object.DestroyImmediate(clonedSo);
                }

                Object.DestroyImmediate(heroGo);
                Object.DestroyImmediate(enemyGo);
                Object.DestroyImmediate(heroDataSo);
            }
        }
    }
}
