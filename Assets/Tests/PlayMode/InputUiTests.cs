using System.Collections;
using System.IO;
using Game.Controls;
using Game.Core;
using Game.Services;
using Game.UI;
using Game.World;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Game.Tests
{
    public sealed class InputUiTests
    {
        private string tempPath;
        private GameBootstrap bootstrap;
        private InputRouter router;
        private Camera cam;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            tempPath = Path.Combine(Path.GetTempPath(), $"ant-input-{System.Guid.NewGuid():N}.json");
            SaveStore.PathOverride = tempPath;
            yield return SceneManager.LoadSceneAsync("Game");
            yield return null;
            bootstrap = Object.FindAnyObjectByType<GameBootstrap>();
            router = Object.FindAnyObjectByType<InputRouter>();
            cam = Object.FindAnyObjectByType<CameraRig>().Camera;
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

        private Vector2 ScreenOf(Vector2 world) => cam.WorldToScreenPoint(new Vector3(world.x, world.y, 0f));

        private Vector2 ScreenOfCell(Vector2Int cell) =>
            ScreenOf(GridCoords.CellCenter(cell, bootstrap.Grid.Width, bootstrap.Grid.Height));

        [UnityTest]
        public IEnumerator ModeButtons_SwitchModeAndHighlight()
        {
            var switcher = Object.FindAnyObjectByType<ModeSwitcher>();
            Assert.AreEqual(InteractMode.Kill, router.Mode);

            switcher.ButtonFor(InteractMode.Dirt).onClick.Invoke();
            yield return null;

            Assert.AreEqual(InteractMode.Dirt, router.Mode);
            Assert.AreEqual(UiKit.AccentColor, switcher.ButtonFor(InteractMode.Dirt).targetGraphic.color);
            Assert.AreEqual(UiKit.ButtonColor, switcher.ButtonFor(InteractMode.Kill).targetGraphic.color);
        }

        [UnityTest]
        public IEnumerator DirtMode_StrokeFillsOpenTiles()
        {
            var cell = new Vector2Int(bootstrap.Grid.BaseRect.xMin - 1, bootstrap.Grid.BaseCenter.y);
            bootstrap.Grid.Dig(cell, 255);
            int painted = 0;
            router.CellsPainted += n => painted += n;
            router.SetMode(InteractMode.Dirt);

            Vector2 screen = ScreenOfCell(cell);
            router.ApplyStroke(screen, screen);
            yield return null;

            Assert.AreEqual(bootstrap.Grid.DirtPerTile, bootstrap.Grid.DirtAt(cell));
            Assert.GreaterOrEqual(painted, 1);
        }

        [UnityTest]
        public IEnumerator KillMode_TapKillsAntUnderFinger()
        {
            router.SetMode(InteractMode.Kill);
            int before = bootstrap.Colony.Count;
            Vector2 screen = ScreenOf(bootstrap.Colony.Ants[0].Position);

            router.ApplyStroke(screen, screen);
            yield return null;

            Assert.Less(bootstrap.Colony.Count, before);
        }

        [UnityTest]
        public IEnumerator StrokeStartingOnUi_IsIgnored()
        {
            router.SetMode(InteractMode.Kill);
            var underBar = new Vector2(Screen.width * 0.5f, 10f);
            Vector2 world = cam.ScreenToWorldPoint(new Vector3(underBar.x, underBar.y, 10f));
            bootstrap.Colony.Ants[0].Position = world;
            int before = bootstrap.Colony.Count;

            router.Handle(new GestureEvent(GestureKind.StrokeBegin, underBar, underBar));
            router.Handle(new GestureEvent(GestureKind.StrokeEnd, underBar, underBar));
            yield return null;

            Assert.AreEqual(before, bootstrap.Colony.Count);
        }

        [UnityTest]
        public IEnumerator ViewMode_DragPansCamera()
        {
            router.SetMode(InteractMode.View);
            Vector3 before = cam.transform.position;
            var from = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

            router.ApplyStroke(from, from + new Vector2(100f, 0f));
            yield return null;

            Assert.Less(cam.transform.position.x, before.x);
        }

        [UnityTest]
        public IEnumerator UpgradePanel_BuyButtonSpendsDirt()
        {
            var hud = Object.FindAnyObjectByType<HudView>();
            var panel = Object.FindAnyObjectByType<UpgradePanel>();
            bootstrap.Wallet.Add(100);

            hud.UpgradesButton.onClick.Invoke();
            yield return null;
            Assert.IsTrue(panel.IsOpen);
            UpgradeRow row = panel.RowFor(UpgradeId.WalkSpeed);
            Assert.IsTrue(row.BuyButton.interactable);

            int dirtBefore = bootstrap.Wallet.Dirt;
            row.BuyButton.onClick.Invoke();
            yield return null;

            Assert.AreEqual(1, bootstrap.Upgrades.Level(UpgradeId.WalkSpeed));
            Assert.AreEqual(dirtBefore - 20, bootstrap.Wallet.Dirt);
        }
    }
}
