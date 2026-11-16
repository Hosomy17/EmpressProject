using System;
using Com.Voobox.Framework.Events;
using Com.Voobox.Project.Events.BossEvents;
using Com.Voobox.Project.Events.HeroEvents;
using UnityEngine;

namespace Com.Voobox.Project.Systems
{
    public class GameScriptSystem : MonoBehaviour
    {

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
        }

        private void OnBossDied(BossDiedEvent args)
        {
            Debug.Log("[GameScriptSystem] Boss died.");
        }
    }
}
