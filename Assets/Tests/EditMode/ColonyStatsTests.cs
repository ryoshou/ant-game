using Game.Debugging;
using NUnit.Framework;
using UnityEngine.TestTools.Constraints;
using NoAlloc = UnityEngine.TestTools.Constraints.Is;

namespace Game.Tests
{
    public sealed class ColonyStatsTests
    {
        [Test]
        public void IncomePerSecond_AveragesLastTenSeconds()
        {
            var stats = new ColonyStats();
            stats.OnDeposit(10, 0f);
            stats.OnDeposit(10, 5f);

            Assert.AreEqual(2f, stats.IncomePerSecond(9f), 1e-4f);
            Assert.AreEqual(1f, stats.IncomePerSecond(12f), 1e-4f);
            Assert.AreEqual(0f, stats.IncomePerSecond(30f), 1e-4f);
        }

        [Test]
        public void AverageTripTiles_UsesLastTwentyTrips()
        {
            var stats = new ColonyStats();
            Assert.AreEqual(0f, stats.AverageTripTiles);

            for (int i = 0; i < 5; i++) stats.OnTrip(100f);
            for (int i = 0; i < 20; i++) stats.OnTrip(10f);

            Assert.AreEqual(10f, stats.AverageTripTiles, 1e-4f);
        }

        [Test]
        public void IncomePerSecond_SurvivesMoreDepositsThanBufferHolds()
        {
            var stats = new ColonyStats();
            for (int i = 0; i < 5000; i++) stats.OnDeposit(1, i * 0.001f);

            Assert.Greater(stats.IncomePerSecond(5f), 0f);
        }

        [Test]
        public void Stats_DoNotAllocate()
        {
            var stats = new ColonyStats();
            stats.OnDeposit(1, 0f);
            stats.OnTrip(1f);

            Assert.That(() =>
            {
                stats.OnDeposit(2, 1f);
                stats.OnTrip(3f);
                _ = stats.IncomePerSecond(2f);
                _ = stats.AverageTripTiles;
            }, NoAlloc.Not.AllocatingGCMemory());
        }
    }
}
