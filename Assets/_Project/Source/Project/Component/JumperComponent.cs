using UnityEngine;

namespace Com.Voobox.Project.Component
{
    public class JumperComponent
    {
        private float m_jumpForce;
        private Rigidbody2D m_rigidbody2D;

        public JumperComponent(float jumpForce, Rigidbody2D rigidbody2D)
        {
            m_jumpForce = jumpForce;
            m_rigidbody2D = rigidbody2D;
        }

        public void Jump()
        {
            m_rigidbody2D.linearVelocity = new Vector2(m_rigidbody2D.linearVelocity.x, 0);
            m_rigidbody2D.AddForce(Vector2.up * m_jumpForce, ForceMode2D.Impulse);
        }
        
        public void JumpCut()
        {
            m_rigidbody2D.linearVelocity = new Vector2(m_rigidbody2D.linearVelocity.x, m_rigidbody2D.linearVelocity.y * 0.5f);
        }
    }
}