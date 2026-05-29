using UnityEngine;

namespace Com.Voobox.Project.Component
{
    public class HeroAnimatorComponent
    {
        private readonly int m_attackFront = Animator.StringToHash("AttackFront");
        private readonly int m_attackUp = Animator.StringToHash("AttackUp");
        private readonly int m_attackDown = Animator.StringToHash("AttackDown");
        private readonly int m_grounded = Animator.StringToHash("Grounded");
        private readonly int m_dash = Animator.StringToHash("Dash");
        private readonly int m_airVelocity = Animator.StringToHash("AirVelocity");
        private readonly int m_speed = Animator.StringToHash("Speed");
        private readonly Animator m_animator;
        
        private SpriteRenderer m_renderer;

        public HeroAnimatorComponent(Animator animator, SpriteRenderer renderer)
        {
            m_animator = animator;
            m_renderer = renderer;
        }

        public void SetSpeed(float speed)
        {
            m_animator.SetFloat(m_speed, speed);
        }

        public void SetGrounded(bool isGrounded)
        {
            m_animator.SetBool(m_grounded, isGrounded);
        }

        public void SetDash(bool isDashing)
        {
            m_animator.SetBool(m_dash, isDashing);
        }

        public void SetAirVelocity(float airVelocity)
        {
            m_animator.SetFloat(m_airVelocity, airVelocity);
        }

        public void SetAttackFront(bool isAttacking)
        {
            m_animator.SetBool(m_attackFront, isAttacking);
        }
        
        public void SetAttackUp(bool isAttacking)
        {
            m_animator.SetBool(m_attackUp, isAttacking);
        }
        
        public void SetAttackDown(bool isAttacking)
        {
            m_animator.SetBool(m_attackDown, isAttacking);
        }

        public void SetAllAttacks(bool isAttacking)
        {
            SetAttackFront(isAttacking);
            SetAttackUp(isAttacking);
            SetAttackDown(isAttacking);
        }

        public void FlipX(bool isFlipping)
        {
            m_renderer.flipX = isFlipping;
        }
    }
}