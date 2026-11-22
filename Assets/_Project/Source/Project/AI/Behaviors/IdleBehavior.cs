using System;
using Com.Voobox.Project.Data;
using UnityEngine;

namespace Com.Voobox.Project.AI
{
    [Serializable]
    public sealed class IdleBehavior : IAIBehavior
    {
        public IdleBehavior() { }

        public void OnEnter(EnemyContext ctx, ref EnemyIntent intent)
        {
            intent.MoveAxis = Vector2.zero;
        }

        public void Tick(EnemyContext ctx, ref EnemyIntent intent)
        {
            intent.MoveAxis = Vector2.zero;
        }

        public void OnExit(EnemyContext ctx) { }
    }
}
