using System;

namespace Com.Voobox.Project.AI
{
    [Serializable]
    public sealed class TargetWithinDetectionRangeCondition : IAICondition
    {
        public TargetWithinDetectionRangeCondition() { }

        public bool Evaluate(EnemyContext ctx)
        {
            return ctx.Body.HasDetectedTarget && ctx.Body.DetectedTargetDistance <= ctx.Body.DetectionRange;
        }
    }
}
