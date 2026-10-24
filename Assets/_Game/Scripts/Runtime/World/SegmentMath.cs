using UnityEngine;

namespace Game.World
{
    public static class SegmentMath
    {
        /// <summary>Squared distance from <paramref name="p"/> to the segment a→b (a point when a == b).</summary>
        public static float DistanceSq(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float lengthSq = ab.sqrMagnitude;
            float t = lengthSq > 0f ? Mathf.Clamp01(Vector2.Dot(p - a, ab) / lengthSq) : 0f;
            return (a + ab * t - p).sqrMagnitude;
        }
    }
}
