using System.Reflection;
using Com.Voobox.Project.Systems;
using NUnit.Framework;
using UnityEngine;

namespace Com.Voobox.Project.Tests.EditMode
{
    public class PauseControllerTests
    {
        private GameObject m_go;
        private PauseController m_controller;
        private float m_initialTimeScale;

        [SetUp]
        public void SetUp()
        {
            m_initialTimeScale = Time.timeScale;
            Time.timeScale = 1f;

            m_go = new GameObject("PauseControllerTest");
            m_controller = m_go.AddComponent<PauseController>();
        }

        [TearDown]
        public void TearDown()
        {
            if (m_go != null)
            {
                Object.DestroyImmediate(m_go);
                m_go = null;
            }

            Time.timeScale = m_initialTimeScale;
        }

        private static void InvokePrivateMethod(object target, string methodName)
        {
            MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, $"Method '{methodName}' not found on {target.GetType().Name}.");
            method.Invoke(target, null);
        }

        [Test]
        public void Pause_SetsIsPausedTrue_AndZeroesTimeScale()
        {
            m_controller.Pause();

            Assert.IsTrue(m_controller.IsPaused);
            Assert.AreEqual(0f, Time.timeScale);
        }

        [Test]
        public void Pause_WhenAlreadyPaused_IsIdempotent()
        {
            Time.timeScale = 0.5f;
            m_controller.Pause();
            m_controller.Pause();

            Assert.IsTrue(m_controller.IsPaused);
            Assert.AreEqual(0f, Time.timeScale);

            m_controller.Resume();
            Assert.AreEqual(0.5f, Time.timeScale);
        }

        [Test]
        public void Resume_RestoresPreviousTimeScale()
        {
            Time.timeScale = 0.5f;
            m_controller.Pause();

            Assert.AreEqual(0f, Time.timeScale);

            m_controller.Resume();

            Assert.IsFalse(m_controller.IsPaused);
            Assert.AreEqual(0.5f, Time.timeScale);
        }

        [Test]
        public void Resume_WhenNotPaused_IsIdempotent()
        {
            Time.timeScale = 0.75f;
            m_controller.Resume();

            Assert.IsFalse(m_controller.IsPaused);
            Assert.AreEqual(0.75f, Time.timeScale);
        }

        [Test]
        public void TogglePause_AlternatesPausedAndResumedStates()
        {
            m_controller.TogglePause();
            Assert.IsTrue(m_controller.IsPaused);
            Assert.AreEqual(0f, Time.timeScale);

            m_controller.TogglePause();
            Assert.IsFalse(m_controller.IsPaused);
            Assert.AreEqual(1f, Time.timeScale);
        }

        [Test]
        public void LateUpdate_WhenPaused_EnforcesZeroTimeScale()
        {
            m_controller.Pause();
            Time.timeScale = 1f;

            InvokePrivateMethod(m_controller, "LateUpdate");

            Assert.AreEqual(0f, Time.timeScale);
        }

        [Test]
        public void LateUpdate_WhenNotPaused_DoesNotModifyTimeScale()
        {
            Time.timeScale = 0.5f;

            InvokePrivateMethod(m_controller, "LateUpdate");

            Assert.AreEqual(0.5f, Time.timeScale);
        }

        [Test]
        public void OnDestroy_WhenPaused_ResetsIsPausedAndTimeScaleToOne()
        {
            Time.timeScale = 0.5f;
            m_controller.Pause();
            Assert.AreEqual(0f, Time.timeScale);

            InvokePrivateMethod(m_controller, "OnDestroy");

            Assert.IsFalse(m_controller.IsPaused);
            Assert.AreEqual(1f, Time.timeScale);
        }

        [Test]
        public void OnDestroy_WhenNotPaused_DoesNotModifyTimeScale()
        {
            Time.timeScale = 0.5f;

            InvokePrivateMethod(m_controller, "OnDestroy");

            Assert.IsFalse(m_controller.IsPaused);
            Assert.AreEqual(0.5f, Time.timeScale);
        }
    }
}
