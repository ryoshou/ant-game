using System.Collections.Generic;

namespace Game.Core
{
    public static class GrowthTiers
    {
        /// <summary>0-based tier: the highest index whose threshold <paramref name="totalLevels"/> has reached.</summary>
        public static int TierFor(int totalLevels, IReadOnlyList<int> thresholds)
        {
            int tier = 0;
            for (int i = 1; i < thresholds.Count; i++)
            {
                if (totalLevels < thresholds[i]) break;
                tier = i;
            }
            return tier;
        }
    }
}
