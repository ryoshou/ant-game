using UnityEngine;

namespace Game.World
{
    /// <summary>Tile ↔ world conversion. One tile is one world unit and the map is centred on the origin.</summary>
    public static class GridCoords
    {
        public static Vector2 CellCenter(Vector2Int c, int width, int height) =>
            new Vector2(c.x - width / 2f + 0.5f, c.y - height / 2f + 0.5f);

        public static Vector2Int WorldToCell(Vector2 p, int width, int height) =>
            new Vector2Int(Mathf.FloorToInt(p.x + width / 2f), Mathf.FloorToInt(p.y + height / 2f));
    }
}
