using System.Collections.Generic;
using UnityEngine;

namespace Game.Sim
{
    /// <summary>One worker ant. Owned and mutated by <see cref="Colony"/>; views only read it.</summary>
    public sealed class Ant
    {
        private const int PathCapacity = 128;

        internal readonly List<Vector2Int> Path = new List<Vector2Int>(PathCapacity);
        internal int PathIndex;
        internal bool NeedsPlan = true;
        internal float Timer;
        internal float DigProgress;
        internal float TripTiles;
        /// <summary>DirtGrid.OpenedTiles when DiggingOut last checked connectivity; -1 forces a check.</summary>
        internal int ConnectivityCheckedAt = -1;

        internal Ant(int id, Vector2 position)
        {
            Id = id;
            Position = position;
        }

        public int Id { get; }

        /// <summary>World position (one tile = one unit).</summary>
        public Vector2 Position { get; internal set; }

        public AntState State { get; private set; } = AntState.ToDig;
        public int Carrying { get; internal set; }
        public Vector2Int Target { get; internal set; }

        /// <summary>Facing direction in radians, for the view.</summary>
        public float Heading { get; internal set; }

        internal void Enter(AntState state)
        {
            State = state;
            NeedsPlan = true;
            Path.Clear();
            PathIndex = 0;
            Timer = 0f;
            DigProgress = 0f;
            ConnectivityCheckedAt = -1;
        }
    }
}
