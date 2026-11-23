using UnityEngine;

namespace Com.Voobox.Project.Data
{
    public interface IHealthData
    {
        public int MaxHitPoints { get; }
        public LayerMask HurtLayer { get; }
        public float KnockbackForce { get; }
    }
}
