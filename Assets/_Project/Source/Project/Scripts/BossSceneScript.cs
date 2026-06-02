using Com.Voobox.Framework.Events;
using Com.Voobox.Project.Events.BossEvents;
using UnityEngine;

namespace Com.Voobox.Project.Scripts
{
    public class BossSceneScript : MonoBehaviour
    {
        void Start()
        {
            EventBus.Notify(new InitializeBossEvent());
        }
    }
}
