using System;
using Com.Voobox.Project.Data;
using UnityEngine;

namespace Com.Voobox.Project.AI
{
    [Serializable]
    public sealed class FaceTargetBehavior : IAIBehavior
    {
        public FaceTargetBehavior() { }

        public void OnEnter(EnemyContext ctx, ref EnemyIntent intent)
        {
            ApplyFacing(ctx, ref intent);
        }

        public void Tick(EnemyContext ctx, ref EnemyIntent intent)
        {
            ApplyFacing(ctx, ref intent);
        }

        public void OnExit(EnemyContext ctx) { }

        private static void ApplyFacing(EnemyContext ctx, ref EnemyIntent intent)
        {
            intent.SpeedMultiplier = 0f;
            intent.MoveAxis = ctx.HasTarget && ctx.DirectionToTargetX != 0f
                ? new Vector2(ctx.DirectionToTargetX, 0f)
                : Vector2.zero;
        }
    }
}
