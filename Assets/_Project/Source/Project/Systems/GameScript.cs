using System;
using Com.Voobox.Framework.Events;
using Com.Voobox.Project.Events.BossEvents;
using Com.Voobox.Project.Events.HeroEvents;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Com.Voobox.Project.Systems
{
    public class GameScript : MonoBehaviour
    {
        private const string GAME_OVER_SCENE_NAME = "GameOverScene";
        private const string VICTORY_SCENE_NAME = "VictoryScene";

        private Action<string> m_loadScene;

        public void SetSceneLoader(Action<string> loadScene)
        {
            m_loadScene = loadScene;
        }

        private void Awake()
        {
            EventBus.Subscribe<HeroDiedEvent>(OnHeroDied);
            EventBus.Subscribe<BossDiedEvent>(OnBossDied);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<HeroDiedEvent>(OnHeroDied);
            EventBus.Unsubscribe<BossDiedEvent>(OnBossDied);
        }

        private void OnHeroDied(HeroDiedEvent args)
        {
            Debug.Log("[GameScriptSystem] Hero died.");
            LoadScene(GAME_OVER_SCENE_NAME);
        }

        private void OnBossDied(BossDiedEvent args)
        {
            Debug.Log("[GameScriptSystem] Boss died.");
            LoadScene(VICTORY_SCENE_NAME);
        }

        private void LoadScene(string sceneName)
        {
            if (m_loadScene != null)
            {
                m_loadScene(sceneName);
                return;
            }

            if (Application.isPlaying)
            {
                SceneManager.LoadScene(sceneName);
            }
        }
    }
}
