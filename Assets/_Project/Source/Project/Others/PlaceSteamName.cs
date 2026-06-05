using System;
using Com.Voobox.Framework.Events;
using Com.Voobox.Project.Events.SteamworksEvents;
using TMPro;
using UnityEngine;

namespace Com.Voobox.Project.Others
{
    public class PlaceSteamName : MonoBehaviour
    {
        [SerializeField] private TMP_Text m_nameText;

        private void Awake()
        {
            EventBus.Subscribe<InitializeSteamworksEvent>(OnInitializeSteamworks);
        }

        private void OnInitializeSteamworks(InitializeSteamworksEvent args)
        {
            m_nameText.text = Steamworks.SteamClient.Name;
        }
    }
}
