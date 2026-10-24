using System;
using System.Collections.Generic;
using Game.Core;
using Game.World;
using UnityEngine;

namespace Game.Sim
{
    /// <summary>
    /// The worker ants: hatching, their per-tick behaviour (<see cref="AntBrain"/>), and the player killing or
    /// burying them. Pure C#; <c>GameBootstrap</c> ticks it.
    /// </summary>
    public sealed class Colony
    {
        private const int CandidateCapacity = 32;
        private const int AntCapacity = 64;

        internal readonly DirtGrid Grid;
        internal readonly UpgradeBook Upgrades;
        internal readonly TuningConfig Config;
        internal readonly GridSearch Search;
        internal readonly System.Random Random;
        internal readonly List<Vector2Int> Candidates = new List<Vector2Int>(CandidateCapacity);

        private readonly Wallet wallet;
        private readonly List<Ant> ants = new List<Ant>(AntCapacity);
        private int nextId;
        private float hatchTimer;

        public Colony(DirtGrid grid, UpgradeBook upgrades, Wallet wallet, TuningConfig config, int seed)
        {
            Grid = grid ?? throw new ArgumentNullException(nameof(grid));
            Upgrades = upgrades ?? throw new ArgumentNullException(nameof(upgrades));
            this.wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
            Config = config ? config : throw new ArgumentNullException(nameof(config));
            Search = new GridSearch(grid);
            Random = new System.Random(seed);
            grid.CellChanged += OnCellChanged;
            for (int i = 0; i < config.startAnts; i++) Spawn();
        }

        /// <summary>Raised when an ant dies — squished or buried. Its Position is where it died.</summary>
        public event Action<Ant> AntKilled;
        public event Action<Ant> AntHatched;
        /// <summary>Dirt amount added to the wallet by one ant's deposit.</summary>
        public event Action<int> Deposited;
        /// <summary>One-way length, in tiles, of the trip whose load was just deposited.</summary>
        public event Action<float> TripCompleted;

        public IReadOnlyList<Ant> Ants => ants;
        public int Count => ants.Count;
        public int Cap => Mathf.RoundToInt(Upgrades.Value(UpgradeId.ColonyCap));

        public void Tick(float dt)
        {
            if (!(dt > 0f)) return;
            TickHatching(dt);
            for (int i = 0; i < ants.Count; i++) AntBrain.Step(ants[i], dt, this);
        }

        public bool Kill(int antId)
        {
            for (int i = 0; i < ants.Count; i++)
            {
                if (ants[i].Id != antId) continue;
                RemoveAt(i);
                return true;
            }
            return false;
        }

        /// <summary>Kills every ant within <paramref name="radius"/> of the segment a→b; returns how many died.</summary>
        public int KillWithin(Vector2 a, Vector2 b, float radius)
        {
            float radiusSq = radius * radius;
            int killed = 0;
            for (int i = ants.Count - 1; i >= 0; i--)
            {
                if (SegmentMath.DistanceSq(ants[i].Position, a, b) > radiusSq) continue;
                RemoveAt(i);
                killed++;
            }
            return killed;
        }

        /// <summary>Replaces every ant with <paramref name="count"/> fresh ones at the base (clamped to the cap).</summary>
        public void SetAntCount(int count)
        {
            ants.Clear();
            hatchTimer = 0f;
            int target = Mathf.Clamp(count, 0, Cap);
            for (int i = 0; i < target; i++) Spawn();
        }

        internal Vector2Int CellOf(Ant ant) => GridCoords.WorldToCell(ant.Position, Grid.Width, Grid.Height);

        internal Vector2 CenterOf(Vector2Int cell) => GridCoords.CellCenter(cell, Grid.Width, Grid.Height);

        internal void CompleteDeposit(Ant ant)
        {
            int load = ant.Carrying;
            ant.Carrying = 0;
            if (load <= 0) return;
            wallet.Add(load);
            Deposited?.Invoke(load);
            TripCompleted?.Invoke(ant.TripTiles);
        }

        private void TickHatching(float dt)
        {
            if (Count >= Cap)
            {
                hatchTimer = 0f;
                return;
            }

            hatchTimer += dt;
            float interval = Mathf.Max(0.01f, Upgrades.Value(UpgradeId.HatchSpeed));
            while (hatchTimer >= interval && Count < Cap)
            {
                hatchTimer -= interval;
                AntHatched?.Invoke(Spawn());
            }
        }

        private Ant Spawn()
        {
            var ant = new Ant(nextId++, CenterOf(Grid.BaseCenter));
            ants.Add(ant);
            return ant;
        }

        private void RemoveAt(int index)
        {
            Ant ant = ants[index];
            ants.RemoveAt(index);
            AntKilled?.Invoke(ant);
        }

        /// <summary>Painting fills a tile completely; any ant standing in it is buried.</summary>
        private void OnCellChanged(Vector2Int cell)
        {
            if (Grid.DirtAt(cell) != Grid.DirtPerTile) return;
            for (int i = ants.Count - 1; i >= 0; i--)
                if (CellOf(ants[i]) == cell) RemoveAt(i);
        }
    }
}
