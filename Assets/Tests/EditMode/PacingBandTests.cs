using Game.Core;
using Game.Sim;
using Game.World;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>Guards the spec's pacing target (25–35 upgrades by minute 10) with slack for crowding and kills.</summary>
    public sealed class PacingBandTests
    {
        private const float Step = 0.1f;
        private const float SessionSeconds = 600f;

        [Test, Category("Slow")]
        public void TenMinutes_GreedyCheapestBuyer_LandsInBand()
        {
            TuningConfig config = TuningConfig.CreateDefault();
            DirtGrid grid = DirtGrid.CreateFilled(config);
            var upgrades = new UpgradeBook(config);
            var wallet = new Wallet();
            var colony = new Colony(grid, upgrades, wallet, config, 1);
            int killed = 0, hatched = 0;
            colony.AntKilled += _ => killed++;
            colony.AntHatched += _ => hatched++;

            float nextPurchase = 1f;
            for (float t = 0f; t < SessionSeconds; t += Step)
            {
                colony.Tick(Step);
                if (t < nextPurchase) continue;
                nextPurchase += 1f;
                BuyCheapest(upgrades, wallet);
            }

            Debug.Log($"Pacing: {upgrades.TotalLevels} upgrades, {colony.Count}/{colony.Cap} ants " +
                      $"(hatched {hatched}, killed {killed}), {wallet.Dirt} dirt banked at 10:00");
            Assert.That(upgrades.TotalLevels, Is.InRange(20, 40));
            Object.DestroyImmediate(config);
        }

        private static void BuyCheapest(UpgradeBook upgrades, Wallet wallet)
        {
            UpgradeId? cheapest = null;
            int cheapestCost = int.MaxValue;
            for (int i = 0; i < TuningConfig.UpgradeCount; i++)
            {
                int cost = upgrades.NextCost((UpgradeId)i);
                if (cost < 0 || cost > wallet.Dirt || cost >= cheapestCost) continue;
                cheapest = (UpgradeId)i;
                cheapestCost = cost;
            }
            if (cheapest.HasValue) upgrades.TryBuy(cheapest.Value, wallet);
        }
    }
}
