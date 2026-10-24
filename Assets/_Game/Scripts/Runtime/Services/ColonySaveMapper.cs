using System;
using Game.Core;
using Game.Sim;
using Game.World;

namespace Game.Services
{
    /// <summary>Copies the live colony into a SaveData and back.</summary>
    public static class ColonySaveMapper
    {
        public static SaveData Capture(DirtGrid grid, Wallet wallet, UpgradeBook upgrades, Colony colony) =>
            new SaveData
            {
                width = grid.Width,
                height = grid.Height,
                grid = Convert.ToBase64String(grid.Snapshot()),
                dirt = wallet.Dirt,
                levels = upgrades.SnapshotLevels(),
                antCount = colony.Count,
            };

        /// <summary>Restores a decoded save. False, with nothing changed, if it doesn't fit this map.</summary>
        public static bool Apply(SaveData data, DirtGrid grid, Wallet wallet, UpgradeBook upgrades, Colony colony)
        {
            if (data == null || data.width != grid.Width || data.height != grid.Height) return false;
            if (data.levels == null || data.levels.Length != TuningConfig.UpgradeCount) return false;
            if (!SaveCodec.TryDecodeGrid(data.grid, out byte[] cells) || !grid.Restore(cells)) return false;

            wallet.Reset();
            wallet.Add(Math.Max(0, data.dirt));
            upgrades.RestoreLevels(data.levels);
            colony.SetAntCount(data.antCount);
            return true;
        }
    }
}
