using System;
using UnityEngine;

namespace Com.Voobox.Project.AI
{
    public enum BodyFlag
    {
        IsGrounded = 0,
        IsAttacking = 1,
        IsDashing = 2,
        CanAttack = 3,
        CanDash = 4,
        IsDead = 5
    }

    [Serializable]
    public sealed class BodyFlagCondition : IAICondition
    {
        [SerializeField] private BodyFlag m_flag;
        [SerializeField] private bool m_expectedValue = true;

        public BodyFlag Flag => m_flag;
        public bool ExpectedValue => m_expectedValue;

        public BodyFlagCondition() { }

        public BodyFlagCondition(BodyFlag flag, bool expectedValue = true)
        {
            m_flag = flag;
            m_expectedValue = expectedValue;
        }

        public bool Evaluate(EnemyContext ctx)
        {
            return m_flag switch
            {
                BodyFlag.IsGrounded => ctx.Body.IsGrounded == m_expectedValue,
                BodyFlag.IsAttacking => ctx.Body.IsAttacking == m_expectedValue,
                BodyFlag.IsDashing => ctx.Body.IsDashing == m_expectedValue,
                BodyFlag.CanAttack => ctx.Body.CanAttack == m_expectedValue,
                BodyFlag.CanDash => ctx.Body.CanDash == m_expectedValue,
                BodyFlag.IsDead => ctx.Body.IsDead == m_expectedValue,
                _ => false
            };
        }
    }
}
