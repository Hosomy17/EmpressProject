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
    public class GameOverSceneScriptTests
    {
        private const string GAME_OVER_SCENE_PATH = "Assets/_Project/Scenes/GameOverScene.unity";

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
        public void GameOverScene_IsRegisteredInEditorBuildSettings()
        {
            bool found = false;
            foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes)
            {
                if (buildScene.path == GAME_OVER_SCENE_PATH && buildScene.enabled)
                {
                    found = true;
                    break;
                }
            }

            Assert.IsTrue(found, $"Expected {GAME_OVER_SCENE_PATH} to be enabled in EditorBuildSettings.scenes.");
        }

        [Test]
        public void GameOverScene_ContainsRequiredHierarchyAndWiring()
        {
            m_openedScene = EditorSceneManager.OpenScene(GAME_OVER_SCENE_PATH, OpenSceneMode.Single);
            Assert.IsTrue(m_openedScene.IsValid(), "GameOverScene failed to open.");

            var mainCamera = Object.FindFirstObjectByType<Camera>();
            Assert.IsNotNull(mainCamera, "GameOverScene must contain a Camera.");

            var canvas = Object.FindFirstObjectByType<Canvas>();
            Assert.IsNotNull(canvas, "GameOverScene must contain a Canvas.");
            Assert.AreEqual(RenderMode.ScreenSpaceOverlay, canvas.renderMode);

            var scaler = canvas.GetComponent<CanvasScaler>();
            Assert.IsNotNull(scaler, "Canvas must have a CanvasScaler.");
            Assert.AreEqual(CanvasScaler.ScaleMode.ScaleWithScreenSize, scaler.uiScaleMode);
            Assert.AreEqual(new Vector2(1920f, 1080f), scaler.referenceResolution);

            Assert.IsNotNull(canvas.GetComponent<GraphicRaycaster>(), "Canvas must have a GraphicRaycaster.");

            var eventSystem = Object.FindFirstObjectByType<EventSystem>();
            Assert.IsNotNull(eventSystem, "GameOverScene must contain an EventSystem.");
            Assert.IsNotNull(
                eventSystem.GetComponent<InputSystemUIInputModule>(),
                "EventSystem must use InputSystemUIInputModule.");

            Transform continueTextTransform = canvas.transform.Find("ContinueText");
            Assert.IsNotNull(continueTextTransform, "Canvas must contain a ContinueText child.");
            var continueText = continueTextTransform.GetComponent<TMP_Text>();
            Assert.IsNotNull(continueText, "ContinueText must have a TMP_Text component.");
            Assert.AreEqual("Continue?", continueText.text);
            Assert.GreaterOrEqual(continueText.fontSize, 48f, "ContinueText should have a large font size.");
            Assert.IsNotNull(continueText.font, "ContinueText must have a Font assigned.");

            Button yesButton = AssertButton(canvas.transform, "YesButton", "Yes");
            Button noButton = AssertButton(canvas.transform, "NoButton", "No");

            var sceneScript = Object.FindFirstObjectByType<GameOverSceneScript>();
            Assert.IsNotNull(sceneScript, "GameOverScene must contain a GameOverSceneScript component.");

            FieldInfo yesButtonField = typeof(GameOverSceneScript).GetField(
                "m_yesButton",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(yesButtonField, "GameOverSceneScript must declare m_yesButton.");
            Assert.AreSame(yesButton, yesButtonField.GetValue(sceneScript),
                "GameOverSceneScript.m_yesButton must reference YesButton.");

            FieldInfo noButtonField = typeof(GameOverSceneScript).GetField(
                "m_noButton",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(noButtonField, "GameOverSceneScript must declare m_noButton.");
            Assert.AreSame(noButton, noButtonField.GetValue(sceneScript),
                "GameOverSceneScript.m_noButton must reference NoButton.");
        }

        [Test]
        public void GameOverSceneScript_AwakeAndOnDestroy_ExecuteSafely()
        {
            var go = new GameObject("GameOverSceneScriptUnitTest");
            try
            {
                var yesGo = new GameObject("Yes");
                yesGo.transform.SetParent(go.transform);
                var yesButton = yesGo.AddComponent<Button>();

                var noGo = new GameObject("No");
                noGo.transform.SetParent(go.transform);
                var noButton = noGo.AddComponent<Button>();

                var script = go.AddComponent<GameOverSceneScript>();

                FieldInfo yesButtonField = typeof(GameOverSceneScript).GetField(
                    "m_yesButton",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                FieldInfo noButtonField = typeof(GameOverSceneScript).GetField(
                    "m_noButton",
                    BindingFlags.Instance | BindingFlags.NonPublic);

                Assert.IsNotNull(yesButtonField);
                Assert.IsNotNull(noButtonField);
                yesButtonField.SetValue(script, yesButton);
                noButtonField.SetValue(script, noButton);

                MethodInfo awakeMethod = typeof(GameOverSceneScript).GetMethod(
                    "Awake",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                MethodInfo onDestroyMethod = typeof(GameOverSceneScript).GetMethod(
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

            var button = buttonTransform.GetComponent<Button>();
            Assert.IsNotNull(button, $"{buttonName} must have a Button component.");

            var buttonImage = buttonTransform.GetComponent<Image>();
            Assert.IsNotNull(buttonImage, $"{buttonName} must have an Image component.");
            Assert.IsNotNull(buttonImage.sprite, $"{buttonName} Image must have a valid Sprite assigned.");

            var buttonText = buttonTransform.GetComponentInChildren<TMP_Text>();
            Assert.IsNotNull(buttonText, $"{buttonName} must contain a TMP_Text label.");
            Assert.AreEqual(expectedLabel, buttonText.text);
            Assert.IsNotNull(buttonText.font, $"{buttonName} Text must have a Font assigned.");

            return button;
        }
    }
}
