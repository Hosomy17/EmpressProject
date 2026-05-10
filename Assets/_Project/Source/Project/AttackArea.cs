using System;
using UnityEngine;

public class AttackArea : MonoBehaviour 
{
    [SerializeField] private LayerMask m_targetLayer;
    
    public Action OnHitSuccess;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & m_targetLayer) != 0)
        {
            OnHitSuccess?.Invoke();
            gameObject.SetActive(false);
        }
    }
}