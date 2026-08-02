using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using TMPro;
using Studio.Runner3d.Features.RunnerPrototype.Presentation.Bootstrap;

namespace Studio.Runner3d.Features.RunnerPrototype.Tests.PlayMode
{
    public class RestartFlowPlayModeTests
    {
        private const string ScenePath = "Assets/_Project/Scenes/Gameplay/RunnerGameplay.unity";

        [UnityTest]
        public IEnumerator ClickingRestartAfterGameOver_ReloadsSceneAndResetsScoreAndPanel()
        {
            yield return LoadGameplayScene();

            var root = Object.FindObjectOfType<RunnerCompositionRoot>();
            Assert.IsNotNull(root, "RunnerCompositionRoot not found in loaded scene.");

            var flowField = typeof(RunnerCompositionRoot).GetField("_gameFlowService",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var flow = flowField.GetValue(root);
            var triggerGameOver = flow.GetType().GetMethod("TriggerGameOver");
            triggerGameOver.Invoke(flow, null);

            yield return null;
            yield return null;

            var scoreGO = GameObject.Find("ScoreText");
            var tmp = scoreGO.GetComponent<TMP_Text>();
            tmp.text = "99";

            var restartGO = GameObject.Find("RestartButton");
            var button = restartGO.GetComponent<Button>();

            int sceneCountBefore = SceneManager.sceneCount;

            button.onClick.Invoke();

            yield return null;
            yield return null;
            yield return null;

            var newRoot = Object.FindObjectOfType<RunnerCompositionRoot>();
            Assert.IsNotNull(newRoot, "RunnerCompositionRoot missing after restart.");

            var newScoreGO = GameObject.Find("ScoreText");
            var newTmp = newScoreGO.GetComponent<TMP_Text>();
            Assert.AreEqual("0", newTmp.text, "Score should reset to 0 after restart.");

            var newFlow = flowField.GetValue(newRoot);
            var stateProp = newFlow.GetType().GetProperty("State");
            string state = stateProp.GetValue(newFlow).ToString();
            Assert.AreEqual("Ready", state, "Game should be back at the start menu (Ready) after restart.");

            Assert.AreEqual(1, SceneManager.sceneCount, "Restart must not leave extra scenes loaded.");
            Assert.AreEqual(sceneCountBefore, SceneManager.sceneCount, "Scene count should be stable across restart.");
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            // The loaded gameplay scene brings in its own EventSystem/Camera; if left
            // loaded it leaks into later tests in the same run (stray raycasts, frustum
            // errors). Tear its objects down so this test stays self-contained.
            Scene scene = SceneManager.GetSceneByPath(ScenePath);
            if (scene.IsValid() && scene.isLoaded)
            {
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    Object.Destroy(root);
                }
            }

            yield return null;
        }

        private static IEnumerator LoadGameplayScene()
        {
            var loadOp = SceneManager.LoadSceneAsync(ScenePath, LoadSceneMode.Single);
            while (!loadOp.isDone)
            {
                yield return null;
            }

            yield return null;
        }
    }
}
