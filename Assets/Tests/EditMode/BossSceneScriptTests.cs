using System.Reflection;
using Com.Voobox.Framework.Events;
using Com.Voobox.Project.Events.BossEvents;
using Com.Voobox.Project.Scripts;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Com.Voobox.Project.Tests.EditMode
{
    public class BossSceneScriptTests
    {
        private GameObject m_sceneGo;
        private BossSceneScript m_bossSceneScript;
        private bool m_previousIgnoreFailingMessages;

        [SetUp]
        public void SetUp()
        {
            m_previousIgnoreFailingMessages = LogAssert.ignoreFailingMessages;
            LogAssert.ignoreFailingMessages = true;

            m_sceneGo = new GameObject("BossSceneScriptTest");
            m_bossSceneScript = m_sceneGo.AddComponent<BossSceneScript>();
        }

        [TearDown]
        public void TearDown()
        {
            if (m_sceneGo != null)
            {
                Object.DestroyImmediate(m_sceneGo);
                m_sceneGo = null;
            }

            m_bossSceneScript = null;
            LogAssert.ignoreFailingMessages = m_previousIgnoreFailingMessages;
        }

        private static void InvokePrivateMethod(object target, string methodName, params object[] args)
        {
            var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, $"Method '{methodName}' not found on {target.GetType().Name}.");
            method.Invoke(target, args);
        }

        [Test]
        public void Start_EmitsInitializeBossEvent()
        {
            int initializeBossCount = 0;
            void OnInitializeBoss(InitializeBossEvent _) => initializeBossCount++;

            EventBus.Subscribe<InitializeBossEvent>(OnInitializeBoss);
            try
            {
                InvokePrivateMethod(m_bossSceneScript, "Start");
                Assert.AreEqual(1, initializeBossCount);
            }
            finally
            {
                EventBus.Unsubscribe<InitializeBossEvent>(OnInitializeBoss);
            }
        }

        [Test]
        public void Start_CatchesSteamClientInitExceptionWithoutCrashing()
        {
            Assert.DoesNotThrow(() => InvokePrivateMethod(m_bossSceneScript, "Start"));
        }

        [Test]
        public void Update_ExecutesSafely_WhenSteamClientIsNotValid()
        {
            Assert.DoesNotThrow(() => InvokePrivateMethod(m_bossSceneScript, "Update"));
        }
    }
}
