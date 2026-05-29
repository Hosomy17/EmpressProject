using System;
using Cysharp.Threading.Tasks;

namespace Com.Voobox.Project.Component
{
    public class AttackerComponent
    {
        private readonly float m_atkTime;
        
        private readonly TaskHandle m_taskAttack = new();

        public AttackerComponent(float atkTime)
        {
            m_atkTime = atkTime;
        }

        public async UniTask Attack(AttackArea attackArea)
        {
            attackArea.gameObject.SetActive(true);
            await UniTask.Delay(TimeSpan.FromSeconds(m_atkTime), cancellationToken: m_taskAttack.GetNewToken()).SuppressCancellationThrow();
            attackArea.gameObject.SetActive(false);
        }
    }
}