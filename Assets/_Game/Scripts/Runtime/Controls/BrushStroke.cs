using System.Collections.Generic;
using Game.World;
using UnityEngine;

namespace Game.Controls
{
    public static class BrushStroke
    {
        /// <summary>
        /// Every in-bounds tile whose centre lies within <paramref name="radius"/> of the world segment, once each.
        /// Covers the whole segment, so a fast swipe between two frames leaves no gaps.
        /// </summary>
        public static void CellsAlong(Vector2 fromWorld, Vector2 toWorld, float radius, int width, int height,
            List<Vector2Int> cellsOut)
        {
            cellsOut.Clear();
            var pad = new Vector2(radius, radius);
            Vector2Int min = GridCoords.WorldToCell(Vector2.Min(fromWorld, toWorld) - pad, width, height);
            Vector2Int max = GridCoords.WorldToCell(Vector2.Max(fromWorld, toWorld) + pad, width, height);
            int xMin = Mathf.Max(min.x, 0), yMin = Mathf.Max(min.y, 0);
            int xMax = Mathf.Min(max.x, width - 1), yMax = Mathf.Min(max.y, height - 1);
            float radiusSq = radius * radius;

            for (int y = yMin; y <= yMax; y++)
            for (int x = xMin; x <= xMax; x++)
            {
                var cell = new Vector2Int(x, y);
                Vector2 center = GridCoords.CellCenter(cell, width, height);
                if (SegmentMath.DistanceSq(center, fromWorld, toWorld) <= radiusSq) cellsOut.Add(cell);
            }
        }
    }
}
