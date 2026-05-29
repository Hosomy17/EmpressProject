using System;
using UnityEngine;

public class AttackArea : MonoBehaviour 
{
    [SerializeField] private LayerMask m_targetLayer;
    [SerializeField] private GameObject m_vfxImpactPrefab;

    public Action OnHitSuccess;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & m_targetLayer) == 0) return;

        OnHitSuccess?.Invoke();
        gameObject.SetActive(false);

        var position = collision.ClosestPoint(transform.position);
        CallVfxImpact(position);
    }

    private void CallVfxImpact(Vector2 position)
    {
        if(m_vfxImpactPrefab == null) return;

        Instantiate(m_vfxImpactPrefab, position, Quaternion.identity);
    }
}