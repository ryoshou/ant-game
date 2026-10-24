using System.Collections;
using System.IO;
using Game.Audio;
using Game.Juice;
using Game.Services;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Game.Tests
{
    public sealed class JuiceTests
    {
        private const int CrowdSize = 10;
        private const int MaxSquishCopies = 4;

        private string tempPath;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            tempPath = Path.Combine(Path.GetTempPath(), $"ant-juice-{System.Guid.NewGuid():N}.json");
            SaveStore.PathOverride = tempPath;
            yield return SceneManager.LoadSceneAsync("Game");
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            foreach (GameBootstrap b in Object.FindObjectsByType<GameBootstrap>(FindObjectsSortMode.None))
                Object.Destroy(b.gameObject);
            yield return null;
            SaveStore.PathOverride = null;
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }

        [UnityTest]
        public IEnumerator DragThroughCrowd_SplatsEveryAnt_ButCapsSquishSounds()
        {
            var bootstrap = Object.FindAnyObjectByType<GameBootstrap>();
            var splats = Object.FindAnyObjectByType<SplatPool>();
            var sfx = Object.FindAnyObjectByType<SfxPlayer>();
            Assert.IsNotNull(splats, "scene has no SplatPool");
            Assert.IsNotNull(sfx, "scene has no SfxPlayer");

            bootstrap.Upgrades.RestoreLevels(new[] { 0, 0, 0, 5, 0 });
            bootstrap.Colony.SetAntCount(CrowdSize);
            for (int i = 0; i < CrowdSize; i++) bootstrap.Colony.Ants[i].Position = new Vector2(i * 0.3f, 0f);

            int killed = bootstrap.Colony.KillWithin(new Vector2(-1f, 0f), new Vector2(4f, 0f), 0.6f);
            yield return null;

            Assert.AreEqual(CrowdSize, killed);
            Assert.AreEqual(CrowdSize, splats.ActiveCount);
            Assert.That(sfx.PlayingCopies(SfxId.Squish), Is.InRange(1, MaxSquishCopies));
        }
    }
}
