using System.Collections.Generic;
using UnityEngine;

namespace Game.View
{
    /// <summary>
    /// Runtime-generated placeholder art, cached. Each sprite is one world unit across (pixels-per-unit = size),
    /// so callers size it with transform scale. White sprites are meant to be tinted by the SpriteRenderer.
    /// The cache outlives play sessions (statics survive when domain reload is off), so destroyed entries rebuild.
    /// </summary>
    public static class PlaceholderSprites
    {
        private const int MoundSize = 128;
        private const int MoundTierCount = 4;

        private static readonly Dictionary<int, Sprite> Circles = new Dictionary<int, Sprite>();
        private static readonly Dictionary<int, Sprite> Ants = new Dictionary<int, Sprite>();
        private static readonly Sprite[] Mounds = new Sprite[MoundTierCount];

        private static readonly Color MoundOuter = new Color(0.36f, 0.24f, 0.13f);
        private static readonly Color MoundInner = new Color(0.48f, 0.33f, 0.18f);
        private static readonly Color Hole = new Color(0.08f, 0.05f, 0.03f);
        private static readonly Color Spire = new Color(0.62f, 0.44f, 0.22f);

        /// <summary>A soft-edged white disc.</summary>
        public static Sprite Circle(int size)
        {
            if (Circles.TryGetValue(size, out Sprite cached) && cached != null) return cached;
            var pixels = new Color32[size * size];
            float r = size / 2f;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                pixels[y * size + x] = White(DiscAlpha(x, y, r, r, r));
            return Circles[size] = Build(pixels, size, "Circle");
        }

        /// <summary>A white top-down ant pointing up: abdomen, thorax, head, and six legs.</summary>
        public static Sprite Ant(int size)
        {
            if (Ants.TryGetValue(size, out Sprite cached) && cached != null) return cached;
            var pixels = new Color32[size * size];
            float s = size;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float a = Mathf.Max(
                    DiscAlpha(x, y, s * 0.5f, s * 0.25f, s * 0.2f),
                    DiscAlpha(x, y, s * 0.5f, s * 0.53f, s * 0.12f),
                    DiscAlpha(x, y, s * 0.5f, s * 0.76f, s * 0.13f));
                a = Mathf.Max(a, LegAlpha(x, y, s));
                pixels[y * size + x] = White(a);
            }
            return Ants[size] = Build(pixels, size, "Ant");
        }

        /// <summary>The colony mound for a growth tier (0-based): more entrances and a spire as it grows.</summary>
        public static Sprite Mound(int tier)
        {
            tier = Mathf.Clamp(tier, 0, MoundTierCount - 1);
            if (Mounds[tier] != null) return Mounds[tier];

            var pixels = new Color32[MoundSize * MoundSize];
            float c = MoundSize / 2f;
            int holes = tier + 1;
            for (int y = 0; y < MoundSize; y++)
            for (int x = 0; x < MoundSize; x++)
            {
                Color color = Color.Lerp(MoundOuter, MoundInner, DiscAlpha(x, y, c, c, c * 0.62f));
                color.a = DiscAlpha(x, y, c, c, c);
                for (int h = 0; h < holes; h++)
                {
                    float angle = h * Mathf.PI * 2f / holes + 0.4f;
                    float hx = c + Mathf.Cos(angle) * c * 0.48f, hy = c + Mathf.Sin(angle) * c * 0.48f;
                    color = Color.Lerp(color, Hole, DiscAlpha(x, y, hx, hy, c * 0.11f));
                }
                if (tier >= 3) color = Color.Lerp(color, Spire, DiscAlpha(x, y, c, c, c * 0.22f));
                pixels[y * MoundSize + x] = color;
            }
            return Mounds[tier] = Build(pixels, MoundSize, $"Mound{tier}");
        }

        private static float DiscAlpha(float x, float y, float cx, float cy, float radius)
        {
            float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(cx, cy));
            return Mathf.Clamp01(radius - d);
        }

        private static float LegAlpha(float x, float y, float s)
        {
            float best = 0f;
            for (int i = -1; i <= 1; i++)
            {
                float legY = s * (0.53f + i * 0.09f);
                float slant = i * s * 0.06f;
                for (int side = -1; side <= 1; side += 2)
                {
                    Vector2 a = new Vector2(s * 0.5f, legY);
                    Vector2 b = new Vector2(s * 0.5f + side * s * 0.42f, legY + slant);
                    float d = Mathf.Sqrt(World.SegmentMath.DistanceSq(new Vector2(x + 0.5f, y + 0.5f), a, b));
                    best = Mathf.Max(best, Mathf.Clamp01(s * 0.035f - d + 0.5f));
                }
            }
            return best;
        }

        private static Color32 White(float alpha) => new Color(1f, 1f, 1f, alpha);

        private static Sprite Build(Color32[] pixels, int size, string name)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = name,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
            };
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            sprite.name = name;
            return sprite;
        }
    }
}
