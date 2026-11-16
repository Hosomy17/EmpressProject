using System.Reflection;
using Com.Voobox.Framework.Events;
using Com.Voobox.Project.Data;
using Com.Voobox.Project.Entity;
using Com.Voobox.Project.Events.BossEvents;
using NUnit.Framework;
using UnityEngine;

namespace Com.Voobox.Project.Tests.EditMode
{
    public class BossComposerHealthTests
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
        public void OnTriggerEnter2D_DecrementsHealthOnHurtLayer_EmitsBossDiedEventOnce_AndIgnoresOtherLayers()
        {
            const int attackAreaLayer = 8;

            var bossDataSo = ScriptableObject.CreateInstance<BossDataScriptableObject>();
            var bossData = new BossData();
            SetPrivateField(bossData, "m_maxHitPoints", 3);
            SetPrivateField(bossData, "m_hurtLayer", (LayerMask)(1 << attackAreaLayer));
            SetPrivateField(bossDataSo, "m_bossData", bossData);

            var bossGo = new GameObject("BossTest");
            bossGo.AddComponent<BoxCollider2D>();
            var bossComposer = bossGo.AddComponent<BossComposer>();
            SetPrivateField(bossComposer, "m_bossData", bossDataSo);

            var hitterGo = new GameObject("HitterAttackArea") { layer = attackAreaLayer };
            var hitterCollider = hitterGo.AddComponent<BoxCollider2D>();

            var otherGo = new GameObject("NonMatchingLayer") { layer = 0 };
            var otherCollider = otherGo.AddComponent<BoxCollider2D>();

            int diedEventCount = 0;
            void OnBossDied(BossDiedEvent _) => diedEventCount++;

            EventBus.Subscribe<BossDiedEvent>(OnBossDied);
            try
            {
                InvokePrivateMethod(bossComposer, "Awake");

                // Non-matching layer hits should not deal damage or emit death event
                InvokePrivateMethod(bossComposer, "OnTriggerEnter2D", otherCollider);
                InvokePrivateMethod(bossComposer, "OnTriggerEnter2D", otherCollider);
                InvokePrivateMethod(bossComposer, "OnTriggerEnter2D", otherCollider);
                Assert.AreEqual(0, diedEventCount);

                // First 2 hits from AttackArea layer -> still alive, 0 events
                InvokePrivateMethod(bossComposer, "OnTriggerEnter2D", hitterCollider);
                InvokePrivateMethod(bossComposer, "OnTriggerEnter2D", hitterCollider);
                Assert.AreEqual(0, diedEventCount);

                // 3rd hit -> dead, 1 event
                InvokePrivateMethod(bossComposer, "OnTriggerEnter2D", hitterCollider);
                Assert.AreEqual(1, diedEventCount);

                // 4th and 5th hits -> ignored once dead, still 1 event
                InvokePrivateMethod(bossComposer, "OnTriggerEnter2D", hitterCollider);
                InvokePrivateMethod(bossComposer, "OnTriggerEnter2D", hitterCollider);
                Assert.AreEqual(1, diedEventCount);
            }
            finally
            {
                EventBus.Unsubscribe<BossDiedEvent>(OnBossDied);
                InvokePrivateMethod(bossComposer, "OnDestroy");

                var clonedSo = typeof(BossComposer)
                    .GetField("m_bossData", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.GetValue(bossComposer) as Object;
                if (clonedSo != null && clonedSo != bossDataSo)
                {
                    Object.DestroyImmediate(clonedSo);
                }

                Object.DestroyImmediate(bossGo);
                Object.DestroyImmediate(hitterGo);
                Object.DestroyImmediate(otherGo);
                Object.DestroyImmediate(bossDataSo);
            }
        }
    }
}
