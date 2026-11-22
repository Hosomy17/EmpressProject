using System;
using Com.Voobox.Project.Data;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Com.Voobox.Project.Component
{
    /// <summary>
    /// Samples Unity Input System player actions into frame-safe <see cref="HeroIntent"/> snapshots.
    /// </summary>
    public sealed class HeroInputController : IHeroController, IDisposable
    {
        private const float AXIS_DEAD_ZONE = 0.2f;

        private readonly InputSystemActions m_inputActions;
        private readonly Action<InputAction.CallbackContext> m_onMovePerformed;
        private readonly Action<InputAction.CallbackContext> m_onMoveCanceled;
        private readonly Action<InputAction.CallbackContext> m_onJumpPerformed;
        private readonly Action<InputAction.CallbackContext> m_onJumpCanceled;
        private readonly Action<InputAction.CallbackContext> m_onDashPerformed;
        private readonly Action<InputAction.CallbackContext> m_onAttackPerformed;

        private Vector2 m_moveAxis;
        private bool m_jumpPressed;
        private bool m_jumpReleased;
        private bool m_dashPressed;
        private bool m_attackPressed;
        private bool m_disposed;

        public HeroInputController()
        {
            m_inputActions = new InputSystemActions();

            m_onMovePerformed = OnMovePerformed;
            m_onMoveCanceled = OnMoveCanceled;
            m_onJumpPerformed = OnJumpPerformed;
            m_onJumpCanceled = OnJumpCanceled;
            m_onDashPerformed = OnDashPerformed;
            m_onAttackPerformed = OnAttackPerformed;

            m_inputActions.Player.Move.performed += m_onMovePerformed;
            m_inputActions.Player.Move.canceled += m_onMoveCanceled;
            m_inputActions.Player.Jump.performed += m_onJumpPerformed;
            m_inputActions.Player.Jump.canceled += m_onJumpCanceled;
            m_inputActions.Player.Dash.performed += m_onDashPerformed;
            m_inputActions.Player.Attack.performed += m_onAttackPerformed;
        }

        public void Enable()
        {
            if (m_disposed) return;
            m_inputActions.Enable();
        }

        public void Disable()
        {
            if (m_disposed) return;
            m_inputActions.Disable();
            m_moveAxis = Vector2.zero;
            m_jumpPressed = false;
            m_jumpReleased = false;
            m_dashPressed = false;
            m_attackPressed = false;
        }

        public HeroIntent SampleIntent(in HeroBodyState body, float deltaTime)
        {
            var intent = new HeroIntent
            {
                MoveAxis = m_moveAxis,
                SpeedMultiplier = 1f,
                JumpPressed = m_jumpPressed,
                JumpReleased = m_jumpReleased,
                DashPressed = m_dashPressed,
                AttackPressed = m_attackPressed
            };

            m_jumpPressed = false;
            m_jumpReleased = false;
            m_dashPressed = false;
            m_attackPressed = false;

            return intent;
        }

        public void Dispose()
        {
            if (m_disposed) return;
            m_disposed = true;

            m_inputActions.Player.Move.performed -= m_onMovePerformed;
            m_inputActions.Player.Move.canceled -= m_onMoveCanceled;
            m_inputActions.Player.Jump.performed -= m_onJumpPerformed;
            m_inputActions.Player.Jump.canceled -= m_onJumpCanceled;
            m_inputActions.Player.Dash.performed -= m_onDashPerformed;
            m_inputActions.Player.Attack.performed -= m_onAttackPerformed;

            m_inputActions.Disable();

            if (Application.isPlaying)
            {
                m_inputActions.Dispose();
            }
            else if (m_inputActions.asset != null)
            {
                UnityEngine.Object.DestroyImmediate(m_inputActions.asset);
            }
        }

        private void OnMovePerformed(InputAction.CallbackContext ctx)
        {
            m_moveAxis = ApplyDeadZone(ctx.ReadValue<Vector2>());
        }

        private void OnMoveCanceled(InputAction.CallbackContext ctx)
        {
            m_moveAxis = Vector2.zero;
        }

        private void OnJumpPerformed(InputAction.CallbackContext ctx)
        {
            m_jumpPressed = true;
        }

        private void OnJumpCanceled(InputAction.CallbackContext ctx)
        {
            m_jumpReleased = true;
        }

        private void OnDashPerformed(InputAction.CallbackContext ctx)
        {
            m_dashPressed = true;
        }

        private void OnAttackPerformed(InputAction.CallbackContext ctx)
        {
            m_attackPressed = true;
        }

        private static Vector2 ApplyDeadZone(Vector2 axis)
        {
            float x = Mathf.Abs(axis.x) < AXIS_DEAD_ZONE ? 0f : Mathf.Sign(axis.x);
            float y = Mathf.Abs(axis.y) < AXIS_DEAD_ZONE ? 0f : Mathf.Sign(axis.y);
            return new Vector2(x, y);
        }
    }
}
