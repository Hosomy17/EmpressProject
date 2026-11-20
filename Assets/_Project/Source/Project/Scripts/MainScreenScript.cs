using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Com.Voobox.Project.Scripts
{
    /// <summary>
    /// Orchestrates the Main Screen UI and transitions to the BossScene on start.
    /// </summary>
    public class MainScreenScript : MonoBehaviour
    {
        private const string BOSS_SCENE_NAME = "BossScene";

        [SerializeField] private Button m_startButton;

        private void Awake()
        {
            if (m_startButton != null)
            {
                m_startButton.onClick.AddListener(OnStartClicked);
            }
        }

        private void OnDestroy()
        {
            if (m_startButton != null)
            {
                m_startButton.onClick.RemoveListener(OnStartClicked);
            }
        }

        private void OnStartClicked()
        {
            SceneManager.LoadScene(BOSS_SCENE_NAME);
        }
    }
}
