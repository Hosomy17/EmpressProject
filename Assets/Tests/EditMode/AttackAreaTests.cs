using System.Collections.Generic;
using System.Reflection;
using Com.Voobox.Project.Others;
using NUnit.Framework;
using UnityEngine;

namespace Com.Voobox.Project.Tests.EditMode
{
    public class AttackAreaTests
    {
        private readonly List<GameObject> m_createdObjects = new List<GameObject>();

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

        private GameObject CreateTrackedGameObject(string name, int layer = 0)
        {
            var go = new GameObject(name) { layer = layer };
            m_createdObjects.Add(go);
            return go;
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

        private (AttackArea area, BoxCollider2D collider) CreateAttackArea(int targetLayer)
        {
            GameObject go = CreateTrackedGameObject("AttackAreaTest");
            var collider = go.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            var area = go.AddComponent<AttackArea>();
            SetPrivateField(area, "m_targetLayer", (LayerMask)(1 << targetLayer));
            return (area, collider);
        }

        [Test]
        public void Awake_DisablesAttachedCollider()
        {
            (AttackArea area, BoxCollider2D collider) = CreateAttackArea(targetLayer: 7);
            collider.enabled = true;

            InvokePrivateMethod(area, "Awake");

            Assert.IsFalse(collider.enabled);
        }

        [Test]
        public void ToggleArea_False_DisablesCollider()
        {
            (AttackArea area, BoxCollider2D collider) = CreateAttackArea(targetLayer: 7);
            InvokePrivateMethod(area, "Awake");
            collider.enabled = true;

            area.ToggleArea(false);

            Assert.IsFalse(collider.enabled);
        }

        [Test]
        public void OnTriggerEnter2D_IgnoresNonMatchingLayer()
        {
            const int targetLayer = 7;
            const int otherLayer = 8;

            (AttackArea area, BoxCollider2D collider) = CreateAttackArea(targetLayer);
            InvokePrivateMethod(area, "Awake");
            collider.enabled = true;

            GameObject otherGo = CreateTrackedGameObject("NonTargetCollider", otherLayer);
            var otherCollider = otherGo.AddComponent<BoxCollider2D>();

            int hitCount = 0;
            area.OnHitSuccess += () => hitCount++;

            InvokePrivateMethod(area, "OnTriggerEnter2D", otherCollider);

            Assert.AreEqual(0, hitCount);
            Assert.IsTrue(collider.enabled);
        }

        [Test]
        public void OnTriggerEnter2D_InvokesOnHitSuccessOnceAndDisablesCollider_OnMatchingLayer()
        {
            const int targetLayer = 7;

            (AttackArea area, BoxCollider2D collider) = CreateAttackArea(targetLayer);
            InvokePrivateMethod(area, "Awake");
            collider.enabled = true;

            GameObject targetGo = CreateTrackedGameObject("TargetCollider", targetLayer);
            var targetCollider = targetGo.AddComponent<BoxCollider2D>();

            int hitCount = 0;
            area.OnHitSuccess += () => hitCount++;

            InvokePrivateMethod(area, "OnTriggerEnter2D", targetCollider);

            Assert.AreEqual(1, hitCount);
            Assert.IsFalse(collider.enabled);
        }
    }
}
