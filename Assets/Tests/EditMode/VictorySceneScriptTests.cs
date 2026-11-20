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
    public class VictorySceneScriptTests
    {
        private const string VICTORY_SCENE_PATH = "Assets/_Project/Scenes/VictoryScene.unity";

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
        public void VictoryScene_IsRegisteredInEditorBuildSettings()
        {
            bool found = false;
            foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes)
            {
                if (buildScene.path == VICTORY_SCENE_PATH && buildScene.enabled)
                {
                    found = true;
                    break;
                }
            }

            Assert.IsTrue(found, $"Expected {VICTORY_SCENE_PATH} to be enabled in EditorBuildSettings.scenes.");
        }

        [Test]
        public void VictoryScene_ContainsRequiredHierarchyAndWiring()
        {
            m_openedScene = EditorSceneManager.OpenScene(VICTORY_SCENE_PATH, OpenSceneMode.Single);
            Assert.IsTrue(m_openedScene.IsValid(), "VictoryScene failed to open.");

            Camera mainCamera = Object.FindFirstObjectByType<Camera>();
            Assert.IsNotNull(mainCamera, "VictoryScene must contain a Camera.");

            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            Assert.IsNotNull(canvas, "VictoryScene must contain a Canvas.");
            Assert.AreEqual(RenderMode.ScreenSpaceOverlay, canvas.renderMode);

            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            Assert.IsNotNull(scaler, "Canvas must have a CanvasScaler.");
            Assert.AreEqual(CanvasScaler.ScaleMode.ScaleWithScreenSize, scaler.uiScaleMode);
            Assert.AreEqual(new Vector2(1920f, 1080f), scaler.referenceResolution);

            Assert.IsNotNull(canvas.GetComponent<GraphicRaycaster>(), "Canvas must have a GraphicRaycaster.");

            EventSystem eventSystem = Object.FindFirstObjectByType<EventSystem>();
            Assert.IsNotNull(eventSystem, "VictoryScene must contain an EventSystem.");
            Assert.IsNotNull(
                eventSystem.GetComponent<InputSystemUIInputModule>(),
                "EventSystem must use InputSystemUIInputModule.");

            Transform victoryTextTransform = canvas.transform.Find("VictoryText");
            Assert.IsNotNull(victoryTextTransform, "Canvas must contain a VictoryText child.");
            TMP_Text victoryText = victoryTextTransform.GetComponent<TMP_Text>();
            Assert.IsNotNull(victoryText, "VictoryText must have a TMP_Text component.");
            Assert.AreEqual("Victory", victoryText.text);
            Assert.GreaterOrEqual(victoryText.fontSize, 48f, "VictoryText should have a large font size.");
            Assert.IsNotNull(victoryText.font, "VictoryText must have a Font assigned.");

            Button mainMenuButton = AssertButton(canvas.transform, "MainMenuButton", "Main Menu");

            VictorySceneScript sceneScript = Object.FindFirstObjectByType<VictorySceneScript>();
            Assert.IsNotNull(sceneScript, "VictoryScene must contain a VictorySceneScript component.");

            FieldInfo mainMenuButtonField = typeof(VictorySceneScript).GetField(
                "m_mainMenuButton",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(mainMenuButtonField, "VictorySceneScript must declare m_mainMenuButton.");
            Assert.AreSame(mainMenuButton, mainMenuButtonField.GetValue(sceneScript),
                "VictorySceneScript.m_mainMenuButton must reference MainMenuButton.");
        }

        [Test]
        public void VictorySceneScript_AwakeAndOnDestroy_ExecuteSafely()
        {
            GameObject go = new GameObject("VictorySceneScriptUnitTest");
            try
            {
                GameObject buttonGo = new GameObject("MainMenuButton");
                buttonGo.transform.SetParent(go.transform);
                Button mainMenuButton = buttonGo.AddComponent<Button>();

                VictorySceneScript script = go.AddComponent<VictorySceneScript>();

                FieldInfo mainMenuButtonField = typeof(VictorySceneScript).GetField(
                    "m_mainMenuButton",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.IsNotNull(mainMenuButtonField);
                mainMenuButtonField.SetValue(script, mainMenuButton);

                MethodInfo awakeMethod = typeof(VictorySceneScript).GetMethod(
                    "Awake",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                MethodInfo onDestroyMethod = typeof(VictorySceneScript).GetMethod(
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

        private static Button AssertButton(Transform canvasTransform, string buttonName, string expectedLabel)
        {
            Transform buttonTransform = canvasTransform.Find(buttonName);
            Assert.IsNotNull(buttonTransform, $"Canvas must contain a {buttonName} child.");

            Button button = buttonTransform.GetComponent<Button>();
            Assert.IsNotNull(button, $"{buttonName} must have a Button component.");

            Image buttonImage = buttonTransform.GetComponent<Image>();
            Assert.IsNotNull(buttonImage, $"{buttonName} must have an Image component.");
            Assert.IsNotNull(buttonImage.sprite, $"{buttonName} Image must have a valid Sprite assigned.");

            TMP_Text buttonText = buttonTransform.GetComponentInChildren<TMP_Text>();
            Assert.IsNotNull(buttonText, $"{buttonName} must contain a TMP_Text label.");
            Assert.AreEqual(expectedLabel, buttonText.text);
            Assert.IsNotNull(buttonText.font, $"{buttonName} Text must have a Font assigned.");

            return button;
        }
    }
}
