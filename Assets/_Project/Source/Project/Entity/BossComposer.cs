using Com.Voobox.Framework.Events;
using Com.Voobox.Project.Events.BossEvents;
using UnityEngine;

namespace Com.Voobox.Project.Entity
{
    public class BossComposer : MonoBehaviour
    {
        private void Awake()
        {
            EventBus.Subscribe<InitializeBossEvent>(InitializeBoss);
        }

        private void InitializeBoss(InitializeBossEvent args)
        {
            EventBus.Notify(new DoRaiseOfSwordsEvent());
        }
    }
}