using System;
using Com.Voobox.Framework.Events;
using Com.Voobox.Project.Events.BossEvents;
using Com.Voobox.Project.Events.SteamworksEvents;
using UnityEngine;

namespace Com.Voobox.Project.Scripts
{
    public class BossSceneScript : MonoBehaviour
    {
        private void Start()
        {
            EventBus.Notify(new InitializeBossEvent());
            
            try
            {
                Steamworks.SteamClient.Init( 480);
                EventBus.Notify(new InitializeSteamworksEvent());
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.ToString());
            }
        }

        private void Update()
        {
            if(Steamworks.SteamClient.IsValid)
                Steamworks.SteamClient.RunCallbacks();
        }
    }
}
