using System.Collections.Generic;
using System.Reflection;
using Com.Voobox.Framework.ObjectPooling;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Com.Voobox.Project.Tests.EditMode
{
    public class ReturnToPoolBehaviourTests
    {
        private GameObject m_poolerGo;
        private GameObject m_poolParentGo;
        private GameObject m_pooledGo;
        private ReturnToPoolBehaviour m_behaviour;

        [TearDown]
        public void TearDown()
        {
            var instanceProperty = typeof(ObjectPooler).GetProperty(
                nameof(ObjectPooler.Instance),
                BindingFlags.Public | BindingFlags.Static);
            instanceProperty?.SetValue(null, null);

            if (m_behaviour != null)
            {
                Object.DestroyImmediate(m_behaviour);
                m_behaviour = null;
            }

            if (m_pooledGo != null)
            {
                Object.DestroyImmediate(m_pooledGo);
                m_pooledGo = null;
            }

            if (m_poolParentGo != null)
            {
                Object.DestroyImmediate(m_poolParentGo);
                m_poolParentGo = null;
            }

            if (m_poolerGo != null)
            {
                Object.DestroyImmediate(m_poolerGo);
                m_poolerGo = null;
            }
        }

        [Test]
        public void OnStateEnter_WithPooledObjectAndAnimator_ReturnsGameObjectToPoolAndDeactivatesIt()
        {
            m_poolerGo = new GameObject("TestObjectPooler");
            var pooler = m_poolerGo.AddComponent<ObjectPooler>();

            var instanceProperty = typeof(ObjectPooler).GetProperty(
                nameof(ObjectPooler.Instance),
                BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(instanceProperty);
            instanceProperty.SetValue(null, pooler);

            var assetRef = new AssetReferenceGameObject("00000000000000000000000000000001");
            var key = assetRef.RuntimeKey;

            m_poolParentGo = new GameObject("TestPoolParent");
            m_poolParentGo.transform.SetParent(m_poolerGo.transform);

            var poolParentsField = typeof(ObjectPooler).GetField(
                "m_poolParents",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var poolDictionaryField = typeof(ObjectPooler).GetField(
                "m_poolDictionary",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(poolParentsField);
            Assert.IsNotNull(poolDictionaryField);

            var poolParents = (Dictionary<object, Transform>)poolParentsField.GetValue(pooler);
            var poolDictionary = (Dictionary<object, Queue<GameObject>>)poolDictionaryField.GetValue(pooler);
            var queue = new Queue<GameObject>();
            poolParents[key] = m_poolParentGo.transform;
            poolDictionary[key] = queue;

            m_pooledGo = new GameObject("TestPooledObject");
            var animator = m_pooledGo.AddComponent<Animator>();
            var pooledObject = m_pooledGo.AddComponent<PooledObject>();
            pooledObject.Initialize(assetRef);

            Assert.IsTrue(m_pooledGo.activeSelf);

            m_behaviour = ScriptableObject.CreateInstance<ReturnToPoolBehaviour>();
            m_behaviour.OnStateEnter(animator, default, 0);

            Assert.IsFalse(m_pooledGo.activeSelf);
            Assert.AreSame(m_poolParentGo.transform, m_pooledGo.transform.parent);
            Assert.AreEqual(1, queue.Count);
            Assert.AreSame(m_pooledGo, queue.Peek());
        }
    }
}
