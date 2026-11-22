using System;
using Com.Voobox.Project.Data;
using UnityEngine;

namespace Com.Voobox.Project.AI
{
    [Serializable]
    public sealed class RequestDashBehavior : IAIBehavior
    {
        public RequestDashBehavior() { }

        public void OnEnter(EnemyContext ctx, ref EnemyIntent intent)
        {
            if (ctx.Body.CanDash)
            {
                intent.DashPressed = true;
            }

            if (ctx.HasTarget && ctx.DirectionToTargetX != 0f)
            {
                intent.MoveAxis = new Vector2(ctx.DirectionToTargetX, 0f);
            }
        }

        public void Tick(EnemyContext ctx, ref EnemyIntent intent) { }

        public void OnExit(EnemyContext ctx) { }
    }
}
