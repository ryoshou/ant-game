using System.IO;
using Game.Core;
using Game.Services;
using Game.Sim;
using Game.World;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Game.Tests
{
    public sealed class SaveTests
    {
        private TuningConfig config;
        private string tempPath;

        private sealed class World
        {
            public DirtGrid Grid;
            public Wallet Wallet;
            public UpgradeBook Upgrades;
            public Colony Colony;
        }

        [SetUp]
        public void SetUp()
        {
            config = TuningConfig.CreateDefault();
            tempPath = Path.Combine(Path.GetTempPath(), $"ant-save-test-{System.Guid.NewGuid():N}.json");
            SaveStore.PathOverride = tempPath;
        }

        [TearDown]
        public void TearDown()
        {
            SaveStore.PathOverride = null;
            if (File.Exists(tempPath)) File.Delete(tempPath);
            Object.DestroyImmediate(config);
        }

        private World NewWorld(DirtGrid grid)
        {
            var world = new World { Grid = grid, Wallet = new Wallet(), Upgrades = new UpgradeBook(config) };
            world.Colony = new Colony(grid, world.Upgrades, world.Wallet, config, 1);
            return world;
        }

        private World SmallWorld() => NewWorld(new DirtGrid(9, 9, 10, new RectInt(3, 3, 3, 3)));

        private static SaveData Capture(World w) => ColonySaveMapper.Capture(w.Grid, w.Wallet, w.Upgrades, w.Colony);

        private static bool Apply(SaveData data, World w) =>
            ColonySaveMapper.Apply(data, w.Grid, w.Wallet, w.Upgrades, w.Colony);

        [Test]
        public void CaptureApply_RoundTripsGridWalletLevelsAndAnts()
        {
            World source = SmallWorld();
            source.Grid.Dig(new Vector2Int(6, 4), 7);
            source.Wallet.Add(123);
            source.Upgrades.RestoreLevels(new[] { 1, 2, 3, 4, 5 });
            source.Colony.SetAntCount(9);
            string json = SaveCodec.Encode(Capture(source));

            World target = SmallWorld();
            SaveData decoded = SaveCodec.Decode(json, out bool wasCorrupt);

            Assert.IsFalse(wasCorrupt);
            Assert.IsTrue(Apply(decoded, target));
            Assert.AreEqual(3, target.Grid.DirtAt(new Vector2Int(6, 4)));
            Assert.AreEqual(source.Grid.TotalDirt, target.Grid.TotalDirt);
            Assert.AreEqual(123, target.Wallet.Dirt);
            Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }, target.Upgrades.SnapshotLevels());
            Assert.AreEqual(9, target.Colony.Count);
        }

        [Test]
        public void Decode_Garbage_IsCorrupt()
        {
            Assert.IsNull(SaveCodec.Decode("{not json", out bool wasCorrupt));
            Assert.IsTrue(wasCorrupt);
        }

        [Test]
        public void Decode_Empty_IsNotCorrupt()
        {
            Assert.IsNull(SaveCodec.Decode(string.Empty, out bool wasCorrupt));
            Assert.IsFalse(wasCorrupt);
            Assert.IsNull(SaveCodec.Decode(null, out wasCorrupt));
            Assert.IsFalse(wasCorrupt);
        }

        [Test]
        public void Decode_WrongGridLength_IsCorrupt()
        {
            SaveData small = Capture(SmallWorld());
            World big = NewWorld(DirtGrid.CreateFilled(config));
            int bigDirt = big.Grid.TotalDirt;
            big.Wallet.Add(50);

            Assert.IsFalse(Apply(small, big));
            Assert.AreEqual(bigDirt, big.Grid.TotalDirt);
            Assert.AreEqual(50, big.Wallet.Dirt);
        }

        [Test]
        public void Decode_WrongVersionOrLevelsLength_IsCorrupt()
        {
            SaveData data = Capture(SmallWorld());

            data.version = 2;
            Assert.IsNull(SaveCodec.Decode(SaveCodec.Encode(data), out bool wasCorrupt));
            Assert.IsTrue(wasCorrupt);

            data.version = 1;
            data.levels = new[] { 1, 2 };
            Assert.IsNull(SaveCodec.Decode(SaveCodec.Encode(data), out wasCorrupt));
            Assert.IsTrue(wasCorrupt);
        }

        [Test]
        public void Decode_BadBase64OrNegativeDirt_IsCorrupt()
        {
            SaveData data = Capture(SmallWorld());
            data.grid = "!!not base64!!";
            Assert.IsNull(SaveCodec.Decode(SaveCodec.Encode(data), out bool wasCorrupt));
            Assert.IsTrue(wasCorrupt);

            data = Capture(SmallWorld());
            data.dirt = -5;
            Assert.IsNull(SaveCodec.Decode(SaveCodec.Encode(data), out wasCorrupt));
            Assert.IsTrue(wasCorrupt);
        }

        [Test]
        public void Store_SaveThenLoad_RoundTrips()
        {
            World world = SmallWorld();
            world.Wallet.Add(42);

            SaveStore.Save(Capture(world));
            SaveData loaded = SaveStore.Load();

            Assert.IsNotNull(loaded);
            Assert.AreEqual(42, loaded.dirt);
            Assert.IsFalse(File.Exists(tempPath + ".tmp"));
        }

        [Test]
        public void Store_MissingFile_ReturnsNull()
        {
            Assert.IsNull(SaveStore.Load());
        }

        [Test]
        public void Store_CorruptFile_ReturnsNullAndWarns()
        {
            File.WriteAllText(tempPath, "garbage");
            LogAssert.Expect(LogType.Warning, "Save was corrupt; starting a fresh colony");

            Assert.IsNull(SaveStore.Load());
        }
    }
}
