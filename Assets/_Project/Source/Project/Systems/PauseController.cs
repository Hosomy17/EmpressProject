using UnityEngine;
using UnityEngine.InputSystem;

namespace Com.Voobox.Project.Systems
{
    /// <summary>
    /// Handles game pause/resume on ESC press, controlling Time.timeScale and enforcing
    /// Time.timeScale = 0f in LateUpdate while paused so unscaled hit-stop or other scripts
    /// cannot restore Time.timeScale during a pause.
    /// </summary>
    public class PauseController : MonoBehaviour
    {
        private float m_previousTimeScale = 1f;

        public bool IsPaused { get; private set; }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                TogglePause();
            }
        }

        private void LateUpdate()
        {
            if (IsPaused)
            {
                Time.timeScale = 0f;
            }
        }

        private void OnDestroy()
        {
            if (!IsPaused) return;

            IsPaused = false;
            Time.timeScale = 1f;
        }

        public void TogglePause()
        {
            if (IsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }

        public void Pause()
        {
            if (IsPaused) return;

            m_previousTimeScale = Time.timeScale > 0f ? Time.timeScale : 1f;
            IsPaused = true;
            Time.timeScale = 0f;
        }

        public void Resume()
        {
            if (!IsPaused) return;

            IsPaused = false;
            Time.timeScale = m_previousTimeScale;
        }
    }
}
