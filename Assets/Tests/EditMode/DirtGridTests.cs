using Game.Core;
using Game.World;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public sealed class DirtGridTests
    {
        private static DirtGrid DefaultGrid() => DirtGrid.CreateFilled(TuningConfig.CreateDefault());

        [Test]
        public void CreateFilled_DefaultMap_HasSpecTotalDirt()
        {
            DirtGrid grid = DefaultGrid();

            Assert.AreEqual((64 * 64 - 25) * 10, grid.TotalDirt);
            Assert.AreEqual(new RectInt(29, 29, 5, 5), grid.BaseRect);
            Assert.AreEqual(new Vector2Int(31, 31), grid.BaseCenter);
            Assert.IsTrue(grid.IsOpen(grid.BaseCenter));
            Assert.IsTrue(grid.IsBase(grid.BaseCenter));
            Assert.IsFalse(grid.IsOpen(new Vector2Int(0, 0)));
        }

        [Test]
        public void Dig_RemovesUpToAvailable_AndOpensTileWhenEmpty()
        {
            DirtGrid grid = DefaultGrid();
            var cell = new Vector2Int(28, 31);
            int before = grid.TotalDirt;

            Assert.AreEqual(7, grid.Dig(cell, 7));
            Assert.IsFalse(grid.IsOpen(cell));
            Assert.AreEqual(3, grid.Dig(cell, 7));

            Assert.IsTrue(grid.IsOpen(cell));
            Assert.AreEqual(before - 10, grid.TotalDirt);
            Assert.AreEqual(0, grid.Dig(cell, 7));
        }

        [Test]
        public void Dig_OnBaseOrOutOfBounds_RemovesNothing()
        {
            DirtGrid grid = DefaultGrid();

            Assert.AreEqual(0, grid.Dig(grid.BaseCenter, 5));
            Assert.AreEqual(0, grid.Dig(new Vector2Int(64, 0), 5));
        }

        [Test]
        public void Fill_RefillsOpenTile_AndRaisesCellChanged()
        {
            DirtGrid grid = DefaultGrid();
            var cell = new Vector2Int(28, 31);
            grid.Dig(cell, 10);
            Vector2Int? changed = null;
            int version = grid.Version;
            grid.CellChanged += c => changed = c;

            Assert.IsTrue(grid.Fill(cell));

            Assert.AreEqual(10, grid.DirtAt(cell));
            Assert.AreEqual(cell, changed);
            Assert.Greater(grid.Version, version);
            Assert.AreEqual((64 * 64 - 25) * 10, grid.TotalDirt);
        }

        [Test]
        public void Fill_OnBaseOrOutOfBounds_ReturnsFalse()
        {
            DirtGrid grid = DefaultGrid();

            Assert.IsFalse(grid.Fill(grid.BaseCenter));
            Assert.IsFalse(grid.Fill(new Vector2Int(-1, 0)));
            Assert.IsFalse(grid.Fill(new Vector2Int(0, 0)));
            Assert.AreEqual(0, grid.DirtAt(grid.BaseCenter));
        }

        [Test]
        public void SnapshotRestore_RoundTrips()
        {
            DirtGrid grid = DefaultGrid();
            grid.Dig(new Vector2Int(28, 31), 4);
            byte[] snapshot = grid.Snapshot();
            DirtGrid other = DefaultGrid();
            bool reloaded = false;
            other.Reloaded += () => reloaded = true;

            Assert.IsTrue(other.Restore(snapshot));

            Assert.AreEqual(6, other.DirtAt(new Vector2Int(28, 31)));
            Assert.AreEqual(grid.TotalDirt, other.TotalDirt);
            Assert.IsTrue(reloaded);
        }

        [Test]
        public void Restore_RejectsWrongLengthOrBadValues()
        {
            DirtGrid grid = DefaultGrid();
            int total = grid.TotalDirt;
            byte[] tooHigh = grid.Snapshot();
            tooHigh[0] = 11;
            byte[] filledBase = grid.Snapshot();
            filledBase[31 * 64 + 31] = 5;

            Assert.IsFalse(grid.Restore(new byte[10]));
            Assert.IsFalse(grid.Restore(null));
            Assert.IsFalse(grid.Restore(tooHigh));
            Assert.IsFalse(grid.Restore(filledBase));
            Assert.AreEqual(total, grid.TotalDirt);
        }

        [Test]
        public void ResetFilled_RestoresStartingDirt()
        {
            DirtGrid grid = DefaultGrid();
            grid.Dig(new Vector2Int(28, 31), 10);

            grid.ResetFilled();

            Assert.AreEqual((64 * 64 - 25) * 10, grid.TotalDirt);
        }

        [Test]
        public void CellCenterAndWorldToCell_AreInverse()
        {
            Vector2Int[] cells = { new Vector2Int(0, 0), new Vector2Int(63, 63), new Vector2Int(32, 32) };

            foreach (Vector2Int cell in cells)
                Assert.AreEqual(cell, GridCoords.WorldToCell(GridCoords.CellCenter(cell, 64, 64), 64, 64));

            Assert.AreEqual(new Vector2(-31.5f, -31.5f), GridCoords.CellCenter(new Vector2Int(0, 0), 64, 64));
        }
    }
}
