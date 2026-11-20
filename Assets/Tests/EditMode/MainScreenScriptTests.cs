using System.Reflection;
using Com.Voobox.Project.Scripts;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Com.Voobox.Project.Tests.EditMode
{
    public class MainScreenScriptTests
    {
        private const string MAIN_SCREEN_SCENE_PATH = "Assets/_Project/Scenes/MainMenuScene.unity";

        private Scene m_openedScene;

        [TearDown]
        public void TearDown()
        {
            if (m_openedScene.IsValid() && m_openedScene.isLoaded)
            {
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            }
        }

        [Test]
        public void MainScreenScene_IsRegisteredInEditorBuildSettings()
        {
            bool found = false;
            foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes)
            {
                if (buildScene.path == MAIN_SCREEN_SCENE_PATH && buildScene.enabled)
                {
                    found = true;
                    break;
                }
            }

            Assert.IsTrue(found, $"Expected {MAIN_SCREEN_SCENE_PATH} to be enabled in EditorBuildSettings.scenes.");
        }

        [Test]
        public void MainScreenScene_ContainsRequiredHierarchyAndWiring()
        {
            m_openedScene = EditorSceneManager.OpenScene(MAIN_SCREEN_SCENE_PATH, OpenSceneMode.Single);
            Assert.IsTrue(m_openedScene.IsValid(), "MainScreenScene failed to open.");

            Camera mainCamera = Object.FindFirstObjectByType<Camera>();
            Assert.IsNotNull(mainCamera, "MainScreenScene must contain a Camera.");

            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            Assert.IsNotNull(canvas, "MainScreenScene must contain a Canvas.");
            Assert.AreEqual(RenderMode.ScreenSpaceOverlay, canvas.renderMode);

            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            Assert.IsNotNull(scaler, "Canvas must have a CanvasScaler.");
            Assert.AreEqual(CanvasScaler.ScaleMode.ScaleWithScreenSize, scaler.uiScaleMode);
            Assert.AreEqual(new Vector2(1920f, 1080f), scaler.referenceResolution);

            Assert.IsNotNull(canvas.GetComponent<GraphicRaycaster>(), "Canvas must have a GraphicRaycaster.");

            EventSystem eventSystem = Object.FindFirstObjectByType<EventSystem>();
            Assert.IsNotNull(eventSystem, "MainScreenScene must contain an EventSystem.");
            Assert.IsNotNull(
                eventSystem.GetComponent<InputSystemUIInputModule>(),
                "EventSystem must use InputSystemUIInputModule.");

            Transform splashTransform = canvas.transform.Find("SplashImage");
            Assert.IsNotNull(splashTransform, "Canvas must contain a SplashImage child.");
            Image splashImage = splashTransform.GetComponent<Image>();
            Assert.IsNotNull(splashImage, "SplashImage must have an Image component.");
            Assert.IsNotNull(splashImage.sprite, "SplashImage must have a valid Sprite assigned.");

            Transform startButtonTransform = canvas.transform.Find("StartButton");
            Assert.IsNotNull(startButtonTransform, "Canvas must contain a StartButton child.");
            Button startButton = startButtonTransform.GetComponent<Button>();
            Assert.IsNotNull(startButton, "StartButton must have a Button component.");
            Image buttonImage = startButtonTransform.GetComponent<Image>();
            Assert.IsNotNull(buttonImage, "StartButton must have an Image component.");
            Assert.IsNotNull(buttonImage.sprite, "StartButton Image must have a valid Sprite assigned.");

            TMP_Text buttonText = startButtonTransform.GetComponentInChildren<TMP_Text>();
            Assert.IsNotNull(buttonText, "StartButton must contain a Text label.");
            Assert.AreEqual("START", buttonText.text);
            Assert.IsNotNull(buttonText.font, "StartButton Text must have a Font assigned.");

            MainScreenScript mainScreenScript = Object.FindFirstObjectByType<MainScreenScript>();
            Assert.IsNotNull(mainScreenScript, "MainScreenScene must contain a MainScreenScript component.");

            FieldInfo startButtonField = typeof(MainScreenScript).GetField(
                "m_startButton",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(startButtonField, "MainScreenScript must declare m_startButton.");
            Assert.AreSame(startButton, startButtonField.GetValue(mainScreenScript),
                "MainScreenScript.m_startButton must reference StartButton.");
        }

        [Test]
        public void MainScreenScript_AwakeAndOnDestroy_ExecuteSafely()
        {
            GameObject go = new GameObject("MainScreenScriptUnitTest");
            try
            {
                Button button = go.AddComponent<Button>();
                MainScreenScript script = go.AddComponent<MainScreenScript>();

                FieldInfo startButtonField = typeof(MainScreenScript).GetField(
                    "m_startButton",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.IsNotNull(startButtonField);
                startButtonField.SetValue(script, button);

                MethodInfo awakeMethod = typeof(MainScreenScript).GetMethod(
                    "Awake",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                MethodInfo onDestroyMethod = typeof(MainScreenScript).GetMethod(
                    "OnDestroy",
                    BindingFlags.Instance | BindingFlags.NonPublic);

                Assert.IsNotNull(awakeMethod);
                Assert.IsNotNull(onDestroyMethod);

                Assert.DoesNotThrow(() => awakeMethod.Invoke(script, null));
                Assert.DoesNotThrow(() => onDestroyMethod.Invoke(script, null));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
