using System;

namespace Com.Voobox.Project.AI
{
    [Serializable]
    public sealed class TargetExitedZoneCondition : IAICondition
    {
        public TargetExitedZoneCondition() { }

        public bool Evaluate(EnemyContext ctx)
        {
            return ctx.ExitedZoneThisTick;
        }
    }
}
