using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Com.Voobox.Project.Scripts
{
    /// <summary>
    /// Orchestrates the Victory scene UI and handles Main Menu scene transition.
    /// </summary>
    public class VictorySceneScript : MonoBehaviour
    {
        private const string MAIN_MENU_SCENE_NAME = "MainMenuScene";

        [SerializeField] private Button m_mainMenuButton;

        private void Awake()
        {
            if (m_mainMenuButton != null)
            {
                m_mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            }
        }

        private void OnDestroy()
        {
            if (m_mainMenuButton != null)
            {
                m_mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
            }
        }

        private void OnMainMenuClicked()
        {
            SceneManager.LoadScene(MAIN_MENU_SCENE_NAME);
        }
    }
}
