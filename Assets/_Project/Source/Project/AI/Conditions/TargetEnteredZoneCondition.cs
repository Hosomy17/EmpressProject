using System;

namespace Com.Voobox.Project.AI
{
    [Serializable]
    public sealed class TargetEnteredZoneCondition : IAICondition
    {
        public TargetEnteredZoneCondition() { }

        public bool Evaluate(EnemyContext ctx)
        {
            return ctx.EnteredZoneThisTick;
        }
    }
}
