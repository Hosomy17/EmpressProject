using Com.Voobox.Framework.ObjectPooling;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Com.Voobox.Project.Tests.EditMode
{
    public class PooledObjectTests
    {
        private GameObject m_gameObject;
        private PooledObject m_pooledObject;

        [SetUp]
        public void SetUp()
        {
            m_gameObject = new GameObject("PooledObjectTest");
            m_pooledObject = m_gameObject.AddComponent<PooledObject>();
        }

        [TearDown]
        public void TearDown()
        {
            if (m_gameObject != null)
            {
                Object.DestroyImmediate(m_gameObject);
                m_gameObject = null;
            }
        }

        [Test]
        public void Initialize_AssignsAssetReferenceOrigin()
        {
            var assetReference = new AssetReferenceGameObject("test-guid");

            m_pooledObject.Initialize(assetReference);

            Assert.AreSame(assetReference, m_pooledObject.AssetReferenceOrigin);
        }

        [Test]
        public void Deactivate_SetsGameObjectInactive()
        {
            m_gameObject.SetActive(true);

            m_pooledObject.Deactivate();

            Assert.IsFalse(m_gameObject.activeSelf);
        }

        [Test]
        public void Activate_SetsGameObjectActive()
        {
            m_gameObject.SetActive(false);

            m_pooledObject.Activate();

            Assert.IsTrue(m_gameObject.activeSelf);
        }
    }
}
