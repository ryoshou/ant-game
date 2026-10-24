using System;
using Game.Core;
using UnityEngine;

namespace Game.World
{
    /// <summary>
    /// The map: a grid of tiles each holding 0..DirtPerTile dirt. A tile with 0 dirt is open ground the ants walk on.
    /// Base tiles are always open and can never be filled.
    /// </summary>
    public sealed class DirtGrid
    {
        private readonly byte[] cells;

        public DirtGrid(int width, int height, int dirtPerTile, RectInt baseRect)
        {
            if (width <= 0 || height <= 0) throw new ArgumentOutOfRangeException(nameof(width), "Grid must be non-empty.");
            if (dirtPerTile < 1 || dirtPerTile > byte.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(dirtPerTile), dirtPerTile, "Dirt per tile must be 1..255.");
            if (baseRect.xMin < 0 || baseRect.yMin < 0 || baseRect.xMax > width || baseRect.yMax > height || baseRect.width < 1 || baseRect.height < 1)
                throw new ArgumentOutOfRangeException(nameof(baseRect), baseRect, "Base must lie inside the grid.");

            Width = width;
            Height = height;
            DirtPerTile = dirtPerTile;
            BaseRect = baseRect;
            BaseCenter = new Vector2Int(baseRect.xMin + baseRect.width / 2, baseRect.yMin + baseRect.height / 2);
            cells = new byte[width * height];
            FillAllExceptBase();
        }

        public int Width { get; }
        public int Height { get; }
        public int DirtPerTile { get; }
        public RectInt BaseRect { get; }
        public Vector2Int BaseCenter { get; }
        public int TotalDirt { get; private set; }

        /// <summary>Incremented on every change; lets observers cheaply detect that the grid moved on.</summary>
        public int Version { get; private set; }

        /// <summary>One tile's dirt amount changed.</summary>
        public event Action<Vector2Int> CellChanged;

        /// <summary>The whole grid was replaced (restore or reset).</summary>
        public event Action Reloaded;

        public static DirtGrid CreateFilled(TuningConfig config)
        {
            int startX = (config.mapWidth - config.baseSize) / 2;
            int startY = (config.mapHeight - config.baseSize) / 2;
            var baseRect = new RectInt(startX, startY, config.baseSize, config.baseSize);
            return new DirtGrid(config.mapWidth, config.mapHeight, config.dirtPerTile, baseRect);
        }

        public bool InBounds(Vector2Int c) => c.x >= 0 && c.y >= 0 && c.x < Width && c.y < Height;

        public bool IsBase(Vector2Int c) => BaseRect.Contains(c);

        public int DirtAt(Vector2Int c) => InBounds(c) ? cells[Index(c)] : 0;

        public bool IsOpen(Vector2Int c) => InBounds(c) && cells[Index(c)] == 0;

        /// <summary>Removes up to <paramref name="maxAmount"/> dirt from a tile; returns how much was removed.</summary>
        public int Dig(Vector2Int c, int maxAmount)
        {
            if (maxAmount <= 0 || !InBounds(c)) return 0;
            int index = Index(c);
            int removed = Math.Min(cells[index], maxAmount);
            if (removed == 0) return 0;
            cells[index] = (byte)(cells[index] - removed);
            TotalDirt -= removed;
            Changed(c);
            return removed;
        }

        /// <summary>Refills a tile to full. False on base tiles, out of bounds, or a tile that is already full.</summary>
        public bool Fill(Vector2Int c)
        {
            if (!InBounds(c) || IsBase(c)) return false;
            int index = Index(c);
            if (cells[index] == DirtPerTile) return false;
            TotalDirt += DirtPerTile - cells[index];
            cells[index] = (byte)DirtPerTile;
            Changed(c);
            return true;
        }

        public void ResetFilled()
        {
            FillAllExceptBase();
            Version++;
            Reloaded?.Invoke();
        }

        public byte[] Snapshot() => (byte[])cells.Clone();

        /// <summary>Replaces every tile. False (and nothing changes) if the data doesn't fit this grid.</summary>
        public bool Restore(byte[] restored)
        {
            if (!IsValidSnapshot(restored)) return false;
            Buffer.BlockCopy(restored, 0, cells, 0, cells.Length);
            TotalDirt = 0;
            for (int i = 0; i < cells.Length; i++) TotalDirt += cells[i];
            Version++;
            Reloaded?.Invoke();
            return true;
        }

        private bool IsValidSnapshot(byte[] restored)
        {
            if (restored == null || restored.Length != cells.Length) return false;
            for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
            {
                byte dirt = restored[y * Width + x];
                if (dirt > DirtPerTile) return false;
                if (dirt != 0 && IsBase(new Vector2Int(x, y))) return false;
            }
            return true;
        }

        private void FillAllExceptBase()
        {
            TotalDirt = 0;
            for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
            {
                bool isBase = IsBase(new Vector2Int(x, y));
                cells[y * Width + x] = isBase ? (byte)0 : (byte)DirtPerTile;
                if (!isBase) TotalDirt += DirtPerTile;
            }
        }

        private void Changed(Vector2Int c)
        {
            Version++;
            CellChanged?.Invoke(c);
        }

        private int Index(Vector2Int c) => c.y * Width + c.x;
    }
}
