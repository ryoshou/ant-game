using System.Collections;
using System.IO;
using Game.Core;
using Game.Debugging;
using Game.Services;
using Game.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Game.Tests
{
    public sealed class DebugTests
    {
        private const string HintKey = "hint.paint.shown";

        private string tempPath;
        private int savedHintFlag;
        private GameBootstrap bootstrap;
        private DebugPanel panel;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            savedHintFlag = PlayerPrefs.GetInt(HintKey, 0);
            PlayerPrefs.DeleteKey(HintKey);
            tempPath = Path.Combine(Path.GetTempPath(), $"ant-debug-{System.Guid.NewGuid():N}.json");
            SaveStore.PathOverride = tempPath;
            yield return SceneManager.LoadSceneAsync("Game");
            yield return null;
            bootstrap = Object.FindAnyObjectByType<GameBootstrap>();
            panel = Object.FindAnyObjectByType<DebugPanel>();
            Assert.IsNotNull(panel, "scene has no DebugPanel");
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            foreach (GameBootstrap b in Object.FindObjectsByType<GameBootstrap>(FindObjectsSortMode.None))
                Object.Destroy(b.gameObject);
            yield return null;
            SaveStore.PathOverride = null;
            if (File.Exists(tempPath)) File.Delete(tempPath);
            PlayerPrefs.SetInt(HintKey, savedHintFlag);
        }

        private Button PanelButton(string caption)
        {
            foreach (Button button in panel.GetComponentsInChildren<Button>(true))
                if (button.name == caption) return button;
            Assert.Fail($"debug panel has no '{caption}' button");
            return null;
        }

        private InputField Field(int upgrade, int column)
        {
            foreach (InputField field in panel.GetComponentsInChildren<InputField>(true))
                if (field.name == $"Field{upgrade}_{column}") return field;
            Assert.Fail($"debug panel has no field {upgrade}/{column}");
            return null;
        }

        [UnityTest]
        public IEnumerator Panel_OpensHidden_AndGiftAddsDirt()
        {
            Assert.IsFalse(panel.IsOpen);
            panel.Open();
            int before = bootstrap.Wallet.Dirt;

            PanelButton("+1000 dirt").onClick.Invoke();
            yield return null;

            Assert.IsTrue(panel.IsOpen);
            Assert.AreEqual(before + 1000, bootstrap.Wallet.Dirt);
        }

        [UnityTest]
        public IEnumerator EditingBaseCost_ChangesRuntimeConfigOnly()
        {
            panel.Open();
            InputField walkCost = Field((int)UpgradeId.WalkSpeed, 0);

            walkCost.text = "30";
            walkCost.onEndEdit.Invoke("30");
            yield return null;

            Assert.AreEqual(30, bootstrap.Upgrades.NextCost(UpgradeId.WalkSpeed));
            Assert.AreEqual(30f, bootstrap.Config.upgrades[0].baseCost);
        }

        [UnityTest]
        public IEnumerator InvalidGrowth_IsRejected()
        {
            panel.Open();
            InputField walkGrowth = Field((int)UpgradeId.WalkSpeed, 1);

            walkGrowth.onEndEdit.Invoke("0.5");
            yield return null;

            Assert.AreEqual(1.45f, bootstrap.Config.upgrades[0].costGrowth, 1e-4f);
            Assert.AreEqual("1.45", walkGrowth.text);
        }

        [UnityTest]
        public IEnumerator MaxColonySize_SetsCapTo45()
        {
            panel.Open();

            PanelButton("Max colony size").onClick.Invoke();
            PanelButton("Fill colony").onClick.Invoke();
            yield return null;

            Assert.AreEqual(45, bootstrap.Colony.Cap);
            Assert.AreEqual(45, bootstrap.Colony.Count);
        }

        [UnityTest]
        public IEnumerator PaintHint_ShowsOnceWhenTripsGetLong()
        {
            var hint = Object.FindAnyObjectByType<PaintHint>();
            var stats = Object.FindAnyObjectByType<StatsTracker>();
            Assert.IsFalse(hint.IsVisible);

            for (int i = 0; i < ColonyStats.TripWindow; i++) stats.Stats.OnTrip(30f);
            yield return null;

            Assert.IsTrue(hint.IsVisible);
            Assert.AreEqual(1, PlayerPrefs.GetInt(HintKey, 0));
        }
    }
}
