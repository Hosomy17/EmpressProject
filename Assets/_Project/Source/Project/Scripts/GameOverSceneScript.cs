using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Com.Voobox.Project.Scripts
{
    /// <summary>
    /// Orchestrates the Game Over scene UI and handles Continue (Yes/No) scene transitions.
    /// </summary>
    public class GameOverSceneScript : MonoBehaviour
    {
        private const string BOSS_SCENE_NAME = "BossScene";
        private const string MAIN_MENU_SCENE_NAME = "MainMenuScene";

        [SerializeField] private Button m_yesButton;
        [SerializeField] private Button m_noButton;

        private void Awake()
        {
            if (m_yesButton != null)
            {
                m_yesButton.onClick.AddListener(OnYesClicked);
            }

            if (m_noButton != null)
            {
                m_noButton.onClick.AddListener(OnNoClicked);
            }
        }

        private void OnDestroy()
        {
            if (m_yesButton != null)
            {
                m_yesButton.onClick.RemoveListener(OnYesClicked);
            }

            if (m_noButton != null)
            {
                m_noButton.onClick.RemoveListener(OnNoClicked);
            }
        }

        private void OnYesClicked()
        {
            SceneManager.LoadScene(BOSS_SCENE_NAME);
        }

        private void OnNoClicked()
        {
            SceneManager.LoadScene(MAIN_MENU_SCENE_NAME);
        }
    }
}
