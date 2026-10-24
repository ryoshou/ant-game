using UnityEngine;

namespace Game.Controls
{
    /// <summary>Orthographic camera framing for a map centred on the origin. Aspect is width ÷ height.</summary>
    public static class CameraMath
    {
        public static float OrthoSizeForTilesAcross(float tiles, float aspect) => tiles / aspect / 2f;

        public static float ClampTilesAcross(float tiles, float min, float max) => Mathf.Clamp(tiles, min, max);

        /// <summary>Keeps the view inside the map; centres an axis the view is wider than.</summary>
        public static Vector2 ClampCenter(Vector2 center, float orthoSize, float aspect, int mapWidth, int mapHeight) =>
            new Vector2(ClampAxis(center.x, orthoSize * aspect, mapWidth), ClampAxis(center.y, orthoSize, mapHeight));

        /// <summary>New camera centre that keeps <paramref name="focusWorld"/> at the same screen spot.</summary>
        public static Vector2 ZoomAround(Vector2 center, Vector2 focusWorld, float oldOrtho, float newOrtho) =>
            focusWorld + (center - focusWorld) * (newOrtho / oldOrtho);

        private static float ClampAxis(float value, float halfView, int mapSize)
        {
            float halfMap = mapSize / 2f;
            if (halfView >= halfMap) return 0f;
            return Mathf.Clamp(value, -halfMap + halfView, halfMap - halfView);
        }
    }
}
