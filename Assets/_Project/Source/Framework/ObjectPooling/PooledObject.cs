using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Com.Voobox.Framework.ObjectPooling
{
    [AddComponentMenu("")]
    public class PooledObject : MonoBehaviour
    {
        public AssetReferenceGameObject AssetReferenceOrigin { get; private set; }

        public void Initialize(AssetReferenceGameObject assetReferenceOrigin)
        {
            AssetReferenceOrigin = assetReferenceOrigin;
        }

        public void Activate()
        {
            gameObject.SetActive(true);
        }

        public void Deactivate()
        {
            gameObject.SetActive(false);
        }
    }
}