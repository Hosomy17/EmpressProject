using Com.Voobox.Project.Others;

namespace Com.Voobox.Project.Data
{
    public interface IAttackerData
    {
        public float AttackDuration { get; }
        public AttackArea AttackUp { get;}
        public AttackArea AttackDown { get;}
        public AttackArea AttackLeft { get;}
        public AttackArea AttackRight { get;}
    }
}