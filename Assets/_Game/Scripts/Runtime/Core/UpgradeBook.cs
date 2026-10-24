using System;

namespace Game.Core
{
    /// <summary>The colony's upgrade levels, their current values, and buying them with dirt.</summary>
    public sealed class UpgradeBook
    {
        private readonly TuningConfig config;
        private readonly int[] levels = new int[TuningConfig.UpgradeCount];

        public UpgradeBook(TuningConfig config)
        {
            this.config = config ? config : throw new ArgumentNullException(nameof(config));
        }

        public event Action<UpgradeId> Bought;

        public int TotalLevels { get; private set; }

        public int Level(UpgradeId id) => levels[(int)id];

        public bool IsMaxed(UpgradeId id) => Level(id) >= config.Get(id).maxLevel;

        /// <summary>Cost of the next level, or -1 when the upgrade is maxed.</summary>
        public int NextCost(UpgradeId id) => IsMaxed(id) ? -1 : config.Get(id).CostAt(Level(id));

        public float Value(UpgradeId id) => config.Get(id).ValueAt(Level(id));

        public bool TryBuy(UpgradeId id, Wallet wallet)
        {
            if (wallet == null) throw new ArgumentNullException(nameof(wallet));
            if (IsMaxed(id) || !wallet.TrySpend(NextCost(id))) return false;
            levels[(int)id]++;
            TotalLevels++;
            Bought?.Invoke(id);
            return true;
        }

        public int[] SnapshotLevels() => (int[])levels.Clone();

        public void RestoreLevels(int[] restored)
        {
            if (restored == null || restored.Length != levels.Length)
                throw new ArgumentException($"Expected {levels.Length} upgrade levels.", nameof(restored));

            TotalLevels = 0;
            for (int i = 0; i < levels.Length; i++)
            {
                int max = config.Get((UpgradeId)i).maxLevel;
                levels[i] = Math.Clamp(restored[i], 0, max);
                TotalLevels += levels[i];
            }
        }
    }
}
