using System;
using Com.Voobox.Project.Data;
using Com.Voobox.Project.Others;
using Cysharp.Threading.Tasks;

namespace Com.Voobox.Project.Component
{
    public class AttackerComponent
    {
        private readonly IAttackerData m_attackerData;

        private readonly TaskHandle m_taskAttack = new();

        public AttackerComponent(IAttackerData attackerData)
        {
            m_attackerData = attackerData;
        }

        public async UniTask AttackUp()
        {
            await Attack(m_attackerData.AttackUp);
        }

        public async UniTask AttackDown()
        {
            await Attack(m_attackerData.AttackDown);
        }

        public async UniTask AttackLeft()
        {
            await Attack(m_attackerData.AttackLeft);
        }

        public async UniTask AttackRight()
        {
            await Attack(m_attackerData.AttackRight);
        }

        private async UniTask Attack(AttackArea attackArea)
        {
            attackArea.ToggleArea(true);
            await UniTask.Delay(TimeSpan.FromSeconds(m_attackerData.AttackDuration), cancellationToken: m_taskAttack.GetNewToken()).SuppressCancellationThrow();
            attackArea.ToggleArea(false);
        }
    }
}