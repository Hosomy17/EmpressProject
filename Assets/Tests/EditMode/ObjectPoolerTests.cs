using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using Com.Voobox.Framework.ObjectPooling;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.TestTools;

namespace Com.Voobox.Project.Tests.EditMode
{
    public class ObjectPoolerTests
    {
        private readonly List<GameObject> m_createdObjects = new List<GameObject>();

        [SetUp]
        public void SetUp()
        {
            ClearSingletonInstance();
        }

        [TearDown]
        public void TearDown()
        {
            for (var i = m_createdObjects.Count - 1; i >= 0; i--)
            {
                if (m_createdObjects[i] != null)
                    Object.DestroyImmediate(m_createdObjects[i]);
            }

            m_createdObjects.Clear();
            ClearSingletonInstance();
        }

        [Test]
        public void Awake_SetsSingletonInstance()
        {
            var pooler = CreatePooler("Pooler_Primary");

            InvokeAwake(pooler);

            Assert.AreSame(pooler, ObjectPooler.Instance);
        }

        [Test]
        public void Awake_DuplicateInstance_KeepsOriginalAndDestroysDuplicate()
        {
            var firstPooler = CreatePooler("Pooler_First");
            InvokeAwake(firstPooler);

            var secondPooler = CreatePooler("Pooler_Duplicate");
            LogAssert.Expect(LogType.Error, new Regex("Destroy may not be called from edit mode"));

            InvokeAwake(secondPooler);

            Assert.AreSame(firstPooler, ObjectPooler.Instance);
        }

        [Test]
        public void ReturnToPool_NullObject_DoesNotThrow()
        {
            var pooler = CreatePooler("Pooler_ReturnNull");
            InvokeAwake(pooler);

            Assert.DoesNotThrow(() => pooler.ReturnToPool(null));
        }

        [Test]
        public void ReturnToPool_NonPooledObject_DestroysSafelyWithoutThrowing()
        {
            var pooler = CreatePooler("Pooler_ReturnNonPooled");
            InvokeAwake(pooler);
            var nonPooled = TrackObject(new GameObject("NonPooledObject"));

            LogAssert.Expect(LogType.Error, new Regex("Destroy may not be called from edit mode"));

            Assert.DoesNotThrow(() => pooler.ReturnToPool(nonPooled));
        }

        [Test]
        public void ReturnToPool_PooledObject_DeactivatesReparentsAndEnqueues()
        {
            var pooler = CreatePooler("Pooler_ReturnPooled");
            InvokeAwake(pooler);

            var assetRef = new AssetReferenceGameObject("test-guid-001");
            var key = assetRef.RuntimeKey;

            var poolParentGo = TrackObject(new GameObject("[Pool] Test"));
            poolParentGo.transform.SetParent(pooler.transform);

            var poolDictionary = GetPrivateField<Dictionary<object, Queue<GameObject>>>(pooler, "m_poolDictionary");
            var poolParents = GetPrivateField<Dictionary<object, Transform>>(pooler, "m_poolParents");

            var queue = new Queue<GameObject>();
            poolDictionary.Add(key, queue);
            poolParents.Add(key, poolParentGo.transform);

            var pooledObj = TrackObject(new GameObject("PooledObjectInstance"));
            var identity = pooledObj.AddComponent<PooledObject>();
            identity.Initialize(assetRef);
            Assert.IsTrue(pooledObj.activeSelf);

            pooler.ReturnToPool(pooledObj);

            Assert.IsFalse(pooledObj.activeSelf);
            Assert.AreSame(poolParentGo.transform, pooledObj.transform.parent);
            Assert.AreEqual(1, queue.Count);
            Assert.AreSame(pooledObj, queue.Peek());
        }

        private ObjectPooler CreatePooler(string name)
        {
            var go = TrackObject(new GameObject(name));
            return go.AddComponent<ObjectPooler>();
        }

        private GameObject TrackObject(GameObject go)
        {
            m_createdObjects.Add(go);
            return go;
        }

        private static void InvokeAwake(ObjectPooler pooler)
        {
            var awakeMethod = typeof(ObjectPooler).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(awakeMethod, "ObjectPooler.Awake method was not found.");
            awakeMethod.Invoke(pooler, null);
        }

        private static T GetPrivateField<T>(object instance, string fieldName)
        {
            var field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Field '{fieldName}' was not found on {instance.GetType().Name}.");
            return (T)field.GetValue(instance);
        }

        private static void ClearSingletonInstance()
        {
            if (ObjectPooler.Instance != null && ObjectPooler.Instance.gameObject != null)
                Object.DestroyImmediate(ObjectPooler.Instance.gameObject);

            var instanceProp = typeof(ObjectPooler).GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
            instanceProp?.SetValue(null, null);
        }
    }
}
