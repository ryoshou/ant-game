using System;
using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// Every world and economy number. Defaults are the spec's playtest starting points
    /// (docs/superpowers/specs/2026-10-24-ant-farm-design.md, "Economy &amp; pacing").
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Tuning Config", fileName = "TuningConfig")]
    public sealed class TuningConfig : ScriptableObject
    {
        public const int UpgradeCount = 5;

        [Header("World")]
        [Min(8)] public int mapWidth = 64;
        [Min(8)] public int mapHeight = 64;
        [Range(1, 255)] public int dirtPerTile = 10;
        [Min(1)] public int baseSize = 5;

        [Header("Colony")]
        [Min(0)] public int startAnts = 3;
        [Min(1)] public int digCandidates = 8;
        [Min(0f)] public float depositSeconds = 0.5f;
        [Min(0.1f)] public float idleRecheckSeconds = 1f;

        [Header("Player")]
        [Min(0.1f)] public float killRadiusTiles = 0.6f;
        [Min(0.1f)] public float brushRadiusTiles = 0.7f;
        [Min(1f)] public float autosaveSeconds = 30f;
        [Tooltip("Show the one-time 'paint near the base' hint once the average trip exceeds this many tiles.")]
        [Min(1f)] public float paintHintTripTiles = 15f;

        [Header("Camera (tiles visible across the screen)")]
        [Min(1f)] public float defaultTilesAcross = 18f;
        [Min(1f)] public float minTilesAcross = 8f;
        [Min(1f)] public float maxTilesAcross = 64f;

        [Header("Upgrades (indexed by UpgradeId)")]
        public UpgradeDef[] upgrades = DefaultUpgrades();

        [Header("Growth tiers (by total upgrade levels)")]
        public int[] tierThresholds = { 0, 10, 30, 55 };
        public float[] tierAntScale = { 1f, 1.15f, 1.3f, 1.45f };
        public Color[] tierAntColor =
        {
            new Color(0.10f, 0.08f, 0.07f),
            new Color(0.05f, 0.04f, 0.04f),
            new Color(0.45f, 0.12f, 0.08f),
            new Color(0.85f, 0.65f, 0.15f),
        };

        public UpgradeDef Get(UpgradeId id)
        {
            int index = (int)id;
            if (upgrades == null || index < 0 || index >= upgrades.Length)
                throw new InvalidOperationException($"TuningConfig has no upgrade entry for {id}.");
            return upgrades[index];
        }

        public static TuningConfig CreateDefault() => CreateInstance<TuningConfig>();

        private static UpgradeDef[] DefaultUpgrades() => new[]
        {
            new UpgradeDef(2f, 0.2f, false, 15, 20f, 1.45f),  // WalkSpeed, tiles/s
            new UpgradeDef(2f, 0.4f, false, 15, 20f, 1.45f),  // DigSpeed, dirt/s
            new UpgradeDef(2f, 1f, false, 8, 40f, 1.70f),     // CarryAmount, dirt per trip
            new UpgradeDef(5f, 2f, false, 20, 50f, 1.50f),    // ColonyCap, ants
            new UpgradeDef(12f, 0.88f, true, 12, 30f, 1.50f), // HatchSpeed, seconds per ant
        };
    }
}
