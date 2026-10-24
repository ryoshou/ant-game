using System.Linq;
using Game.Core;
using Game.Sim;
using Game.World;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools.Constraints;
using NoAlloc = UnityEngine.TestTools.Constraints.Is;

namespace Game.Tests
{
    public sealed class ColonyTests
    {
        private const float Step = 0.05f;

        private TuningConfig config;
        private DirtGrid grid;
        private UpgradeBook upgrades;
        private Wallet wallet;

        [SetUp]
        public void SetUp()
        {
            config = TuningConfig.CreateDefault();
            grid = new DirtGrid(9, 9, 10, new RectInt(3, 3, 3, 3));
            upgrades = new UpgradeBook(config);
            wallet = new Wallet();
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(config);

        private Colony NewColony(int startAnts)
        {
            config.startAnts = startAnts;
            return new Colony(grid, upgrades, wallet, config, 1);
        }

        private Vector2 Center(int x, int y) => GridCoords.CellCenter(new Vector2Int(x, y), grid.Width, grid.Height);

        private void Open(int x, int y) => grid.Dig(new Vector2Int(x, y), 255);

        private static void Run(Colony colony, float seconds)
        {
            for (float t = 0f; t < seconds; t += Step) colony.Tick(Step);
        }

        private static bool RunUntil(Colony colony, float maxSeconds, System.Func<bool> done)
        {
            for (float t = 0f; t < maxSeconds; t += Step)
            {
                colony.Tick(Step);
                if (done()) return true;
            }
            return false;
        }

        [Test]
        public void StartsWithStartAnts_AtBaseCenter()
        {
            Colony colony = NewColony(3);

            Assert.AreEqual(3, colony.Count);
            foreach (Ant ant in colony.Ants) Assert.AreEqual(Center(4, 4), ant.Position);
            Assert.AreEqual(3, colony.Ants.Select(a => a.Id).Distinct().Count());
        }

        [Test]
        public void Hatches_OneAntPerHatchInterval_UpToCap()
        {
            Colony colony = NewColony(3);
            int hatched = 0;
            colony.AntHatched += _ => hatched++;

            colony.Tick(12.01f);
            Assert.AreEqual(4, colony.Count);

            Run(colony, 60f);
            Assert.AreEqual(5, colony.Count);
            Assert.AreEqual(5, colony.Cap);
            Assert.AreEqual(2, hatched);
        }

        [Test]
        public void Ant_DigsAndDeposits_CarryAmount()
        {
            Colony colony = NewColony(1);
            int before = grid.TotalDirt;
            int deposited = 0;
            float trip = 0f;
            colony.Deposited += amount => deposited = amount;
            colony.TripCompleted += tiles => trip = tiles;

            Assert.IsTrue(RunUntil(colony, 20f, () => wallet.Dirt > 0));

            Assert.AreEqual(2, wallet.Dirt);
            Assert.AreEqual(2, deposited);
            Assert.Greater(trip, 0f);
            Assert.LessOrEqual(grid.TotalDirt, before - 2);
        }

        [Test]
        public void Kill_RemovesAnt_AndLosesCarriedDirt()
        {
            Colony colony = NewColony(1);
            Ant ant = colony.Ants[0];
            Ant killed = null;
            colony.AntKilled += a => killed = a;
            Assert.IsTrue(RunUntil(colony, 20f, () => ant.State == AntState.Returning && ant.Carrying > 0));

            Assert.IsTrue(colony.Kill(ant.Id));
            Run(colony, 5f);

            Assert.AreSame(ant, killed);
            Assert.AreEqual(0, colony.Count);
            Assert.AreEqual(0, wallet.Dirt);
        }

        [Test]
        public void Kill_UnknownId_ReturnsFalse()
        {
            Colony colony = NewColony(1);

            Assert.IsFalse(colony.Kill(12345));
            Assert.AreEqual(1, colony.Count);
        }

        [Test]
        public void KillWithin_LongSegment_KillsEveryAntOnIt()
        {
            Colony colony = NewColony(0);
            colony.SetAntCount(5);
            for (int i = 0; i < 5; i++) colony.Ants[i].Position = new Vector2(-3f + 1.5f * i, 0f);
            int killedEvents = 0;
            colony.AntKilled += _ => killedEvents++;

            int killed = colony.KillWithin(new Vector2(-4f, 0f), new Vector2(4f, 0f), 0.6f);

            Assert.AreEqual(5, killed);
            Assert.AreEqual(5, killedEvents);
            Assert.AreEqual(0, colony.Count);
        }

        [Test]
        public void KillWithin_MissesAntsOutsideRadius()
        {
            Colony colony = NewColony(0);
            colony.SetAntCount(1);
            colony.Ants[0].Position = new Vector2(0f, 1f);

            Assert.AreEqual(0, colony.KillWithin(new Vector2(-4f, 0f), new Vector2(4f, 0f), 0.6f));
            Assert.AreEqual(1, colony.Count);
        }

        [Test]
        public void FillingAntsCell_BuriesIt()
        {
            Colony colony = NewColony(1);
            Open(6, 4);
            colony.Ants[0].Position = Center(6, 4);
            int killed = 0;
            colony.AntKilled += _ => killed++;

            grid.Fill(new Vector2Int(6, 4));

            Assert.AreEqual(1, killed);
            Assert.AreEqual(0, colony.Count);
        }

        [Test]
        public void BlockedPath_AntReplans_AndStillDeposits()
        {
            Colony colony = NewColony(1);
            Open(6, 3); Open(7, 3); Open(7, 4); Open(7, 5); Open(6, 5);
            Ant ant = colony.Ants[0];
            ant.Position = Center(7, 4);
            ant.Enter(AntState.Returning);
            ant.Carrying = 2;
            colony.Tick(Step);

            grid.Fill(new Vector2Int(6, 3));
            grid.Fill(new Vector2Int(6, 5));
            Open(6, 5);

            Assert.IsTrue(RunUntil(colony, 30f, () => wallet.Dirt >= 2));
            Assert.AreEqual(1, colony.Count);
        }

        [Test]
        public void Colony_WalledInAnt_DigsOutAndGetsHome()
        {
            Colony colony = NewColony(1);
            Open(0, 0); Open(1, 0); Open(0, 1); Open(1, 1);
            Ant ant = colony.Ants[0];
            ant.Position = Center(0, 0);
            var search = new GridSearch(grid);
            Assert.IsFalse(search.IsConnectedToBase(new Vector2Int(0, 0)));

            bool home = RunUntil(colony, 60f, () => wallet.Dirt > 0);

            Assert.IsTrue(home, $"ant stuck in {ant.State} at {ant.Position}");
            Assert.IsTrue(search.IsConnectedToBase(new Vector2Int(0, 0)));
            Assert.AreNotEqual(AntState.DiggingOut, ant.State);
        }

        [Test]
        public void NoDirtLeft_AntsIdleNearBase_ThenResumeWhenDirtPainted()
        {
            for (int y = 0; y < 9; y++)
            for (int x = 0; x < 9; x++)
                Open(x, y);
            Colony colony = NewColony(3);

            Run(colony, 5f);
            foreach (Ant ant in colony.Ants)
            {
                Assert.AreEqual(AntState.Idle, ant.State);
                Vector2Int cell = GridCoords.WorldToCell(ant.Position, grid.Width, grid.Height);
                Assert.IsTrue(grid.IsOpen(cell));
            }

            grid.Fill(new Vector2Int(6, 4));
            Run(colony, 2f);

            Assert.IsTrue(colony.Ants.Any(a => a.State != AntState.Idle));
        }

        [Test]
        public void SetAntCount_ClampsToCap()
        {
            Colony colony = NewColony(0);

            colony.SetAntCount(99);

            Assert.AreEqual(5, colony.Count);
        }

        [Test]
        public void Tick_SteadyState_DoesNotAllocate()
        {
            grid = new DirtGrid(64, 64, 10, new RectInt(29, 29, 5, 5));
            Colony colony = NewColony(5);
            Run(colony, 30f);

            Assert.That(() => colony.Tick(Step), NoAlloc.Not.AllocatingGCMemory());
        }
    }
}
