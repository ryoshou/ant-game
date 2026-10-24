using System;
using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// One upgrade's numbers. Value at level L is <c>start + perLevel × L</c>, or <c>start × perLevel^L</c> when
    /// multiplicative. Cost of buying level L → L+1 is <c>baseCost × costGrowth^L</c>.
    /// </summary>
    [Serializable]
    public struct UpgradeDef
    {
        public float startValue;
        public float perLevel;
        public bool multiplicative;
        [Min(0)] public int maxLevel;
        [Min(0f)] public float baseCost;
        [Min(1f)] public float costGrowth;

        public UpgradeDef(float startValue, float perLevel, bool multiplicative, int maxLevel, float baseCost, float costGrowth)
        {
            this.startValue = startValue;
            this.perLevel = perLevel;
            this.multiplicative = multiplicative;
            this.maxLevel = maxLevel;
            this.baseCost = baseCost;
            this.costGrowth = costGrowth;
        }

        public float ValueAt(int level) =>
            multiplicative ? startValue * Mathf.Pow(perLevel, level) : startValue + perLevel * level;

        public int CostAt(int level) =>
            (int)Math.Round(baseCost * Math.Pow(costGrowth, level), MidpointRounding.AwayFromZero);
    }
}
