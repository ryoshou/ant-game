using System;
using System.Collections.Generic;
using Game.World;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools.Constraints;
using NoAlloc = UnityEngine.TestTools.Constraints.Is;

namespace Game.Tests
{
    public sealed class GridSearchTests
    {
        private static DirtGrid SmallGrid() => new DirtGrid(9, 9, 10, new RectInt(3, 3, 3, 3));

        private static void Open(DirtGrid grid, int x, int y) => grid.Dig(new Vector2Int(x, y), 255);

        private static bool IsAdjacent(Vector2Int a, Vector2Int b) => Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y) == 1;

        [Test]
        public void FindDigTargets_FromBase_ReturnsRingAroundBase()
        {
            DirtGrid grid = SmallGrid();
            var search = new GridSearch(grid);
            var targets = new List<Vector2Int>();

            int count = search.FindDigTargets(grid.BaseCenter, 64, targets);

            Assert.AreEqual(12, count);
            Assert.AreEqual(12, targets.Count);
            foreach (Vector2Int t in targets)
            {
                Assert.IsFalse(grid.IsBase(t));
                Assert.IsFalse(grid.IsOpen(t));
                Assert.IsTrue(grid.BaseRect.Contains(t + Vector2Int.left) || grid.BaseRect.Contains(t + Vector2Int.right)
                              || grid.BaseRect.Contains(t + Vector2Int.up) || grid.BaseRect.Contains(t + Vector2Int.down));
            }
        }

        [Test]
        public void FindDigTargets_ReturnsNearestFirst()
        {
            DirtGrid grid = SmallGrid();
            Open(grid, 6, 4);
            Open(grid, 7, 4);
            var search = new GridSearch(grid);
            var targets = new List<Vector2Int>();

            search.FindDigTargets(new Vector2Int(7, 4), 3, targets);

            Assert.AreEqual(3, targets.Count);
            foreach (Vector2Int t in targets) Assert.IsTrue(IsAdjacent(t, new Vector2Int(7, 4)), t.ToString());
        }

        [Test]
        public void FindDigTargets_RespectsMaxCandidates()
        {
            DirtGrid grid = SmallGrid();
            var targets = new List<Vector2Int>();

            Assert.AreEqual(5, new GridSearch(grid).FindDigTargets(grid.BaseCenter, 5, targets));
            Assert.AreEqual(5, targets.Count);
        }

        [Test]
        public void FindDigTargets_NoDirtLeft_ReturnsZero()
        {
            DirtGrid grid = SmallGrid();
            for (int y = 0; y < 9; y++)
            for (int x = 0; x < 9; x++)
                Open(grid, x, y);
            var targets = new List<Vector2Int> { Vector2Int.one };

            Assert.AreEqual(0, new GridSearch(grid).FindDigTargets(grid.BaseCenter, 8, targets));
            Assert.AreEqual(0, targets.Count);
        }

        [Test]
        public void FindPathToDig_EndsAdjacentToTarget_AndIsContiguous()
        {
            DirtGrid grid = SmallGrid();
            Open(grid, 6, 4);
            Open(grid, 7, 4);
            Open(grid, 7, 5);
            var target = new Vector2Int(7, 6);
            var path = new List<Vector2Int>();
            var from = new Vector2Int(3, 3);

            Assert.IsTrue(new GridSearch(grid).FindPathToDig(from, target, path));

            Assert.Greater(path.Count, 0);
            Vector2Int previous = from;
            foreach (Vector2Int step in path)
            {
                Assert.IsTrue(IsAdjacent(previous, step), $"{previous} → {step}");
                Assert.IsTrue(grid.IsOpen(step));
                previous = step;
            }
            Assert.IsTrue(IsAdjacent(path[path.Count - 1], target));
        }

        [Test]
        public void FindPathToDig_AlreadyAdjacent_ReturnsEmptyPath()
        {
            DirtGrid grid = SmallGrid();
            var path = new List<Vector2Int> { Vector2Int.zero };

            Assert.IsTrue(new GridSearch(grid).FindPathToDig(new Vector2Int(5, 4), new Vector2Int(6, 4), path));
            Assert.AreEqual(0, path.Count);
        }

        [Test]
        public void FindPathHome_EndsOnBase()
        {
            DirtGrid grid = SmallGrid();
            Open(grid, 6, 4);
            Open(grid, 7, 4);
            var path = new List<Vector2Int>();

            Assert.IsTrue(new GridSearch(grid).FindPathHome(new Vector2Int(7, 4), path));

            Assert.AreEqual(new List<Vector2Int> { new Vector2Int(6, 4), new Vector2Int(5, 4) }, path);
        }

        [Test]
        public void FindPathHome_Blocked_ReturnsFalse()
        {
            DirtGrid grid = SmallGrid();
            Open(grid, 0, 0);
            Open(grid, 1, 0);
            var search = new GridSearch(grid);

            Assert.IsFalse(search.FindPathHome(new Vector2Int(0, 0), new List<Vector2Int>()));
            Assert.IsFalse(search.IsConnectedToBase(new Vector2Int(0, 0)));
            Assert.IsTrue(search.IsConnectedToBase(grid.BaseCenter));
        }

        [Test]
        public void Searches_DoNotAllocate()
        {
            DirtGrid grid = new DirtGrid(64, 64, 10, new RectInt(29, 29, 5, 5));
            var search = new GridSearch(grid);
            var targets = new List<Vector2Int>(16);
            var path = new List<Vector2Int>(256);
            for (int i = 0; i < 3; i++) RunSearches(grid, search, targets, path);

            Assert.That(() => RunSearches(grid, search, targets, path), NoAlloc.Not.AllocatingGCMemory());
        }

        [Test]
        public void AllocationConstraint_DetectsAllocation()
        {
            Assert.That(() => { _ = new int[8]; }, NoAlloc.AllocatingGCMemory());
        }

        private static void RunSearches(DirtGrid grid, GridSearch search, List<Vector2Int> targets, List<Vector2Int> path)
        {
            search.FindDigTargets(grid.BaseCenter, 8, targets);
            search.FindPathToDig(grid.BaseCenter, targets[0], path);
            search.FindPathHome(grid.BaseCenter, path);
            search.IsConnectedToBase(grid.BaseCenter);
        }
    }
}
