using UnityEngine;

namespace Com.Voobox.Project.Data
{
    public interface IDasherData
    {
        public float DashForce { get; }
        public float DashDuration { get; }
        public float DashCooldown { get; }
        public Rigidbody2D Rigidbody2D { get; }
    }
}