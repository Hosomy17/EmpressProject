using System;
using Com.Voobox.Project.Data;

namespace Com.Voobox.Project.AI
{
    [Serializable]
    public sealed class RequestAttackBehavior : IAIBehavior
    {
        public RequestAttackBehavior() { }

        public void OnEnter(EnemyContext ctx, ref EnemyIntent intent)
        {
            if (ctx.Body.CanAttack)
            {
                intent.AttackPressed = true;
            }
        }

        public void Tick(EnemyContext ctx, ref EnemyIntent intent) { }

        public void OnExit(EnemyContext ctx) { }
    }
}
