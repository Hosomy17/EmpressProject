namespace Com.Voobox.Project.Data
{
    public interface IEntityController<TBodyState, TIntent>
        where TBodyState : struct
        where TIntent : struct
    {
        void Enable();
        void Disable();
        TIntent SampleIntent(in TBodyState body, float deltaTime);
    }
}
