namespace Com.Voobox.Project.AI
{
    /// <summary>
    /// Pure predicate evaluated against the current <see cref="EnemyContext"/> to trigger transitions.
    /// </summary>
    public interface IAICondition
    {
        bool Evaluate(EnemyContext ctx);
    }
}
