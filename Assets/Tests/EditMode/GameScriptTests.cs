using System.Reflection;
using Com.Voobox.Framework.Events;
using Com.Voobox.Project.Events.BossEvents;
using Com.Voobox.Project.Events.HeroEvents;
using Com.Voobox.Project.Systems;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Com.Voobox.Project.Tests.EditMode
{
    public class GameScriptTests
    {
        private static void InvokePrivateMethod(object target, string methodName)
        {
            var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, $"Method '{methodName}' not found on {target.GetType().Name}.");
            method.Invoke(target, null);
        }

        [Test]
        public void OnHeroDied_LogsMessageWhenSubscribed_AndStopsLoggingAfterOnDestroy()
        {
            var go = new GameObject("GameScriptSystemTest");
            var system = go.AddComponent<GameScript>();

            try
            {
                InvokePrivateMethod(system, "Awake");

                LogAssert.Expect(LogType.Log, "[GameScriptSystem] Hero died.");
                EventBus.Notify(new HeroDiedEvent());

                InvokePrivateMethod(system, "OnDestroy");
                Object.DestroyImmediate(go);
                go = null;

                EventBus.Notify(new HeroDiedEvent());
                LogAssert.NoUnexpectedReceived();
            }
            finally
            {
                if (go != null)
                {
                    Object.DestroyImmediate(go);
                }
            }
        }

        [Test]
        public void OnBossDied_LogsMessageWhenSubscribed_AndStopsLoggingAfterOnDestroy()
        {
            var go = new GameObject("GameScriptSystemTest");
            var system = go.AddComponent<GameScript>();

            try
            {
                InvokePrivateMethod(system, "Awake");

                LogAssert.Expect(LogType.Log, "[GameScriptSystem] Boss died.");
                EventBus.Notify(new BossDiedEvent());

                InvokePrivateMethod(system, "OnDestroy");
                Object.DestroyImmediate(go);
                go = null;

                EventBus.Notify(new BossDiedEvent());
                LogAssert.NoUnexpectedReceived();
            }
            finally
            {
                if (go != null)
                {
                    Object.DestroyImmediate(go);
                }
            }
        }
    }
}
