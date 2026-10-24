using System.Collections;
using System.IO;
using Game.Services;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Game.Tests
{
    public sealed class GameSmokeTests
    {
        private const float SimulatedSeconds = 20f;
        private const float TimeScale = 4f;

        private string tempPath;

        [SetUp]
        public void SetUp()
        {
            tempPath = Path.Combine(Path.GetTempPath(), $"ant-smoke-{System.Guid.NewGuid():N}.json");
            SaveStore.PathOverride = tempPath;
        }

        /// <summary>Destroy the bootstrap first: leaving play mode would otherwise save it to the real save path.</summary>
        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Time.timeScale = 1f;
            foreach (GameBootstrap bootstrap in Object.FindObjectsByType<GameBootstrap>(FindObjectsSortMode.None))
                Object.Destroy(bootstrap.gameObject);
            yield return null;
            SaveStore.PathOverride = null;
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }

        [UnityTest]
        public IEnumerator GameScene_RunsAndColonyEarns()
        {
            yield return SceneManager.LoadSceneAsync("Game");
            var bootstrap = Object.FindAnyObjectByType<GameBootstrap>();
            Assert.IsNotNull(bootstrap, "Game scene has no GameBootstrap");

            Time.timeScale = TimeScale;
            yield return new WaitForSeconds(SimulatedSeconds);
            Time.timeScale = 1f;

            LogAssert.NoUnexpectedReceived();
            Assert.GreaterOrEqual(bootstrap.Colony.Count, 3);
            Assert.Greater(bootstrap.Wallet.Dirt, 0);
        }
    }
}
