using Com.Voobox.Project.Data;

namespace Com.Voobox.Project.AI
{
    /// <summary>
    /// Atomic, stateless or instance-scoped behavior executed inside an AI state.
    /// </summary>
    public interface IAIBehavior
    {
        void OnEnter(EnemyContext ctx, ref EnemyIntent intent);
        void Tick(EnemyContext ctx, ref EnemyIntent intent);
        void OnExit(EnemyContext ctx);
    }
}
