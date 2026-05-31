using Com.Voobox.Project.Data;
using UnityEngine;

namespace Com.Voobox.Project.Component
{
    public class JumperComponent
    {
        private readonly IJumperData m_jumperData;
        private readonly Rigidbody2D m_rigidbody2D;

        public JumperComponent(IJumperData jumperData)
        {
            m_jumperData = jumperData;
            m_rigidbody2D = m_jumperData.Rigidbody2D;
        }

        public void Jump()
        {
            m_rigidbody2D.linearVelocity = new Vector2(m_rigidbody2D.linearVelocity.x, 0);
            m_rigidbody2D.AddForce(Vector2.up * m_jumperData.JumpForce, ForceMode2D.Impulse);
        }

        public void JumpCut()
        {
            m_rigidbody2D.linearVelocity = new Vector2(m_rigidbody2D.linearVelocity.x, m_rigidbody2D.linearVelocity.y * 0.5f);
        }
    }
}