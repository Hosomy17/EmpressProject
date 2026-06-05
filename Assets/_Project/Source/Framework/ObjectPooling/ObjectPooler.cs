using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Com.Voobox.Framework.ObjectPooling
{
    public class ObjectPooler : MonoBehaviour
    {
        [SerializeField] private List<PoolConfiguration> m_poolsToPreWarm;

        private readonly Dictionary<object, Queue<GameObject>> m_poolDictionary = new Dictionary<object, Queue<GameObject>>();
        private readonly Dictionary<object, Transform> m_poolParents = new Dictionary<object, Transform>();

        public static ObjectPooler Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            InitializePreWarmedPools().Forget();
        }

        private async UniTaskVoid InitializePreWarmedPools()
        {
            if (m_poolsToPreWarm == null) return;

            foreach (var poolConfiguration in m_poolsToPreWarm)
            {
                if (poolConfiguration.m_assetReference != null && poolConfiguration.m_assetReference.RuntimeKeyIsValid())
                    await PreWarmPoolAsync(poolConfiguration.m_assetReference, poolConfiguration.m_preWarmAmount);
            }
        }

        private async UniTask PreWarmPoolAsync(AssetReferenceGameObject assetReference, int amount)
        {
            var key = assetReference.RuntimeKey;

            if (!m_poolDictionary.ContainsKey(key))
                m_poolDictionary.Add(key, new Queue<GameObject>());

            if (!m_poolParents.ContainsKey(key))
            {
                var prefabName = "Unknown_Addressable";
                var locations = await Addressables.LoadResourceLocationsAsync(assetReference).ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy());

                if (locations is { Count: > 0 })
                    prefabName = System.IO.Path.GetFileNameWithoutExtension(locations[0].PrimaryKey);

                var newParent = new GameObject($"[Pool] {prefabName}");
                newParent.transform.SetParent(transform);
                m_poolParents.Add(key, newParent.transform);
            }

            var poolParent = m_poolParents[key];
            var instantiationTasks = new List<UniTask<GameObject>>();

            for (var i = 0; i < amount; i++)
            {
                var task = assetReference.InstantiateAsync(poolParent).ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy());
                instantiationTasks.Add(task);
            }

            var spawnedObjects = await UniTask.WhenAll(instantiationTasks);
            foreach (var obj in spawnedObjects)
            {
                obj.SetActive(false);
                var identity = obj.AddComponent<PooledObject>();
                identity.Initialize(assetReference);
                m_poolDictionary[key].Enqueue(obj);
            }
        }

        public async UniTask<GameObject> Spawn(AssetReferenceGameObject assetReference, Vector3 position, Quaternion rotation, Transform customParent = null)
        {
            if (assetReference == null || !assetReference.RuntimeKeyIsValid()) return null;

            var key = assetReference.RuntimeKey;

            if (!m_poolDictionary.ContainsKey(key))
                await PreWarmPoolAsync(assetReference, 0);

            GameObject objToSpawn;

            if (m_poolDictionary[key].Count > 0)
            {
                objToSpawn = m_poolDictionary[key].Dequeue();
                objToSpawn.transform.SetParent(customParent);
            }
            else
            {
                objToSpawn = await assetReference.InstantiateAsync(customParent)
                    .ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy());

                var identity = objToSpawn.AddComponent<PooledObject>();
                identity.Initialize(assetReference);
            }

            objToSpawn.transform.SetPositionAndRotation(position, rotation);
            objToSpawn.SetActive(true);

            return objToSpawn;
        }

        public void ReturnToPool(GameObject objToReturn)
        {
            if (objToReturn == null) return;

            if (!objToReturn.TryGetComponent<PooledObject>(out var identity))
            {
                Destroy(objToReturn);
                return;
            }

            var assetRef = identity.AssetReferenceOrigin;
            var key = assetRef.RuntimeKey;

            identity.Deactivate();
            identity.transform.SetParent(m_poolParents[key]);//TODO

            m_poolDictionary[key].Enqueue(objToReturn);
        }

        [System.Serializable]
        private class PoolConfiguration
        {
            public AssetReferenceGameObject  m_assetReference;
            public int m_preWarmAmount;
        }
    }
}