using System;
using Com.Voobox.Framework.ObjectPooling;
using Cysharp.Threading.Tasks;
using FMODUnity;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Com.Voobox.Project.Others//TODO
{
    [RequireComponent(typeof(Collider2D))]
    public class AttackArea : MonoBehaviour 
    {
        [SerializeField] private LayerMask m_targetLayer;
        [SerializeField] private AssetReferenceGameObject m_vfxImpactAssetReference;
        [SerializeField] public EventReference m_sfxAttackEventReference;

        private Collider2D m_collider;

        public Action OnHitSuccess;

        private void Awake()
        {
            m_collider = GetComponent<Collider2D>();
            m_collider.enabled = false;
        }

        public void ToggleArea(bool isActive)
        {
            m_collider.enabled = isActive;

            if(isActive)
                RuntimeManager.PlayOneShot(m_sfxAttackEventReference, transform.position);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (((1 << collision.gameObject.layer) & m_targetLayer) == 0) return;

            OnHitSuccess?.Invoke();
            m_collider.enabled = false;

            var position = collision.ClosestPoint(transform.position);
            CallVfxImpact(position);
        }

        private void CallVfxImpact(Vector2 position)
        {
            if(m_vfxImpactAssetReference == null) return;

            ObjectPooler.Instance.Spawn(m_vfxImpactAssetReference, position, Quaternion.identity).Forget();
        }
    }
}