using System;
using System.Collections.Generic;
using System.Reflection;
using Com.Voobox.Framework.Events;
using Com.Voobox.Project.Events.SteamworksEvents;
using Com.Voobox.Project.Others;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Com.Voobox.Project.Tests.EditMode
{
    public class PlaceSteamNameTests
    {
        private GameObject m_placeSteamNameGo;
        private GameObject m_textGo;
        private PlaceSteamName m_placeSteamName;
        private TextMeshProUGUI m_nameText;

        [SetUp]
        public void SetUp()
        {
            m_textGo = new GameObject("SteamNameText");
            m_nameText = m_textGo.AddComponent<TextMeshProUGUI>();
            m_nameText.text = "InitialPlaceholder";

            m_placeSteamNameGo = new GameObject("PlaceSteamNameTest");
            m_placeSteamName = m_placeSteamNameGo.AddComponent<PlaceSteamName>();
            SetPrivateField(m_placeSteamName, "m_nameText", (TMP_Text)m_nameText);
        }

        [TearDown]
        public void TearDown()
        {
            if (m_placeSteamName != null)
            {
                MethodInfo method = typeof(PlaceSteamName).GetMethod(
                    "OnInitializeSteamworks",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                if (method != null)
                {
                    var handler = (Action<InitializeSteamworksEvent>)Delegate.CreateDelegate(
                        typeof(Action<InitializeSteamworksEvent>),
                        m_placeSteamName,
                        method);
                    EventBus.Unsubscribe(handler);
                }
            }

            if (m_placeSteamNameGo != null)
                Object.DestroyImmediate(m_placeSteamNameGo);
            if (m_textGo != null)
                Object.DestroyImmediate(m_textGo);
        }

        [Test]
        public void Awake_SubscribesToInitializeSteamworksEvent()
        {
            InvokePrivateMethod(m_placeSteamName, "Awake");

            FieldInfo subscribersField = typeof(EventBus).GetField(
                "m_subscribers",
                BindingFlags.Static | BindingFlags.NonPublic);
            Assert.IsNotNull(subscribersField, "EventBus.m_subscribers field not found.");

            var subscribers = subscribersField.GetValue(null) as Dictionary<Type, List<Delegate>>;
            Assert.IsNotNull(subscribers, "EventBus.m_subscribers dictionary is null.");
            Assert.IsTrue(
                subscribers.TryGetValue(typeof(InitializeSteamworksEvent), out List<Delegate> callbacks),
                "Expected InitializeSteamworksEvent to have subscribers after Awake().");

            bool foundSubscriber = false;
            foreach (Delegate callback in callbacks)
            {
                if (ReferenceEquals(callback.Target, m_placeSteamName) &&
                    callback.Method.Name == "OnInitializeSteamworks")
                {
                    foundSubscriber = true;
                    break;
                }
            }

            Assert.IsTrue(
                foundSubscriber,
                "Expected PlaceSteamName.OnInitializeSteamworks to be subscribed to InitializeSteamworksEvent.");
        }

        [Test]
        public void OnInitializeSteamworks_WhenEventNotified_AttemptsToAssignSteamName()
        {
            InvokePrivateMethod(m_placeSteamName, "Awake");

            try
            {
                EventBus.Notify(new InitializeSteamworksEvent());
                Assert.AreNotEqual(
                    "InitialPlaceholder",
                    m_nameText.text,
                    "Expected m_nameText.text to be updated when SteamClient.Name is available.");
            }
            catch (Exception ex) when (ex is NullReferenceException or InvalidOperationException)
            {
                // In headless EditMode without an initialized Steam client, Steamworks.SteamClient.Name
                // may throw when accessed inside OnInitializeSteamworks. Reaching this point confirms
                // EventBus.Notify invoked PlaceSteamName.OnInitializeSteamworks.
                StringAssert.Contains(
                    "OnInitializeSteamworks",
                    ex.StackTrace ?? string.Empty,
                    "Unexpected exception outside PlaceSteamName.OnInitializeSteamworks.");
            }
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Field '{fieldName}' not found on {target.GetType().Name}.");
            field.SetValue(target, value);
        }

        private static void InvokePrivateMethod(object target, string methodName, params object[] args)
        {
            MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, $"Method '{methodName}' not found on {target.GetType().Name}.");
            method.Invoke(target, args);
        }
    }
}
