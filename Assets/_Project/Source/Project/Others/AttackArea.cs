using System;
using UnityEngine;

namespace Com.Voobox.Project.Others//TODO
{
    [RequireComponent(typeof(Collider2D))]
    public class AttackArea : MonoBehaviour 
    {
        [SerializeField] private LayerMask m_targetLayer;
        [SerializeField] private GameObject m_vfxImpactPrefab;

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
            if(m_vfxImpactPrefab == null) return;

            Instantiate(m_vfxImpactPrefab, position, Quaternion.identity);
        }
    }
}