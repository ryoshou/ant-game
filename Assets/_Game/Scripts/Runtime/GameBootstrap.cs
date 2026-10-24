using System;
using Game.Core;
using Game.Services;
using Game.Sim;
using Game.World;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Owns the model (grid, colony, economy), ticks it, and saves it. Everything else reads it through these getters.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public sealed class GameBootstrap : MonoBehaviour
    {
        private const int TargetFrameRate = 60;

        [SerializeField] private TuningConfig config;

        private float autosaveTimer;

        /// <summary>Runtime copy of the tuning asset; the debug panel edits this, never the asset.</summary>
        public TuningConfig Config { get; private set; }
        public DirtGrid Grid { get; private set; }
        public Colony Colony { get; private set; }
        public UpgradeBook Upgrades { get; private set; }
        public Wallet Wallet { get; private set; }

        /// <summary>Raised after <see cref="ResetColony"/> replaced levels and ants without per-item events.</summary>
        public event Action ColonyReset;

        private void Awake()
        {
            if (config == null)
            {
                Debug.LogError("GameBootstrap needs a TuningConfig asset assigned.", this);
                enabled = false;
                return;
            }

            Application.targetFrameRate = TargetFrameRate;
            Config = Instantiate(config);
            Wallet = new Wallet();
            Upgrades = new UpgradeBook(Config);
            Grid = DirtGrid.CreateFilled(Config);
            Colony = new Colony(Grid, Upgrades, Wallet, Config, Environment.TickCount);
            LoadSave();
        }

        private void Update()
        {
            Colony.Tick(Time.deltaTime);
            autosaveTimer += Time.unscaledDeltaTime;
            if (autosaveTimer < Config.autosaveSeconds) return;
            autosaveTimer = 0f;
            Save();
        }

        private void OnApplicationPause(bool isPaused)
        {
            if (isPaused) Save();
        }

        private void OnApplicationQuit() => Save();

        private void OnDestroy()
        {
            if (Config != null) Destroy(Config);
        }

        public void Save()
        {
            if (Grid == null) return;
            SaveStore.Save(ColonySaveMapper.Capture(Grid, Wallet, Upgrades, Colony));
        }

        /// <summary>Fresh filled map, no upgrades, no dirt, starting ants — then saves.</summary>
        public void ResetColony()
        {
            Grid.ResetFilled();
            Upgrades.RestoreLevels(new int[TuningConfig.UpgradeCount]);
            Wallet.Reset();
            Colony.SetAntCount(Config.startAnts);
            Save();
            ColonyReset?.Invoke();
        }

        private void LoadSave()
        {
            SaveData save = SaveStore.Load();
            if (save == null) return;
            if (!ColonySaveMapper.Apply(save, Grid, Wallet, Upgrades, Colony))
                Debug.LogWarning("The save doesn't fit this map; starting a fresh colony.");
        }
    }
}
