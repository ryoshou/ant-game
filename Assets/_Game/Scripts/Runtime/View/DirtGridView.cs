using Game.World;
using UnityEngine;

namespace Game.View
{
    /// <summary>Draws the whole dirt grid as one point-filtered texture, one pixel per tile.</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class DirtGridView : MonoBehaviour
    {
        private const float GrainStrength = 0.06f;

        [SerializeField] private GameBootstrap bootstrap;
        [SerializeField] private Color openColor = new Color(0.86f, 0.76f, 0.58f);
        [SerializeField] private Color dirtColor = new Color(0.40f, 0.27f, 0.15f);
        [SerializeField] private Color baseColor = new Color(0.20f, 0.13f, 0.07f);

        private DirtGrid grid;
        private Texture2D texture;
        private Sprite sprite;
        private Color32[] pixels;
        private bool isDirty;

        private void Start()
        {
            grid = bootstrap.Grid;
            texture = new Texture2D(grid.Width, grid.Height, TextureFormat.RGBA32, false)
            {
                name = "DirtGrid",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
            };
            pixels = new Color32[grid.Width * grid.Height];
            sprite = Sprite.Create(texture, new Rect(0, 0, grid.Width, grid.Height), new Vector2(0.5f, 0.5f), 1f);
            GetComponent<SpriteRenderer>().sprite = sprite;
            transform.position = Vector3.zero;

            RepaintAll();
            grid.CellChanged += OnCellChanged;
            grid.Reloaded += RepaintAll;
        }

        private void LateUpdate()
        {
            if (!isDirty) return;
            texture.SetPixels32(pixels);
            texture.Apply(false);
            isDirty = false;
        }

        private void OnDestroy()
        {
            if (grid != null)
            {
                grid.CellChanged -= OnCellChanged;
                grid.Reloaded -= RepaintAll;
            }
            if (sprite != null) Destroy(sprite);
            if (texture != null) Destroy(texture);
        }

        private void OnCellChanged(Vector2Int cell)
        {
            pixels[cell.y * grid.Width + cell.x] = ColorFor(cell);
            isDirty = true;
        }

        private void RepaintAll()
        {
            for (int y = 0; y < grid.Height; y++)
            for (int x = 0; x < grid.Width; x++)
                pixels[y * grid.Width + x] = ColorFor(new Vector2Int(x, y));
            isDirty = true;
        }

        private Color32 ColorFor(Vector2Int cell)
        {
            if (grid.IsBase(cell)) return baseColor;
            float fill = grid.DirtAt(cell) / (float)grid.DirtPerTile;
            Color color = Color.Lerp(openColor, dirtColor, fill);
            float grain = 1f - GrainStrength * Grain(cell);
            return new Color(color.r * grain, color.g * grain, color.b * grain, 1f);
        }

        /// <summary>Stable 0..1 per-tile noise so the dirt doesn't look like flat paint.</summary>
        private static float Grain(Vector2Int cell)
        {
            uint h = (uint)(cell.x * 73856093) ^ (uint)(cell.y * 19349663);
            h ^= h >> 13;
            h *= 0x5bd1e995;
            return (h & 0xFF) / 255f;
        }
    }
}
