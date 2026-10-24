using UnityEngine;

namespace Game.Juice
{
    /// <summary>Every feel number: sound and haptic rate limits, shake, splats, tier-up. Tune by playing.</summary>
    [CreateAssetMenu(menuName = "Game/Juice Config", fileName = "JuiceConfig")]
    public sealed class JuiceConfig : ScriptableObject
    {
        [Header("Mixer")]
        [Min(1)] public int sfxVoices = 8;

        [Header("Squish")]
        [Min(0f)] public float squishSoundInterval = 0.04f;
        [Min(1)] public int squishMaxCopies = 4;
        public Vector2 squishPitchRange = new Vector2(0.9f, 1.15f);
        [Min(0f)] public float squishHapticInterval = 0.08f;
        [Min(0)] public int squishHapticMilliseconds = 12;
        [Range(0f, 1f)] public float squishHapticAmount = 0.45f;

        [Header("Shake (tiles)")]
        [Min(0f)] public float shakePerKill = 0.08f;
        [Min(0f)] public float shakeMax = 0.25f;
        [Min(0.01f)] public float shakeDecaySeconds = 0.15f;

        [Header("Splats")]
        [Min(0.1f)] public float splatLifetime = 3f;
        [Min(0f)] public float splatFadeSeconds = 1f;
        [Min(1)] public int splatPoolSize = 64;
        public Vector2 splatSizeTiles = new Vector2(0.7f, 1.1f);
        public Color splatColor = new Color(0.32f, 0.12f, 0.06f, 0.9f);

        [Header("Paint")]
        [Min(0f)] public float paintSoundInterval = 0.08f;
        public Vector2 paintPitchRange = new Vector2(0.85f, 1.1f);
        [Min(0f)] public float paintHapticInterval = 0.1f;
        [Min(0)] public int paintHapticMilliseconds = 6;
        [Range(0f, 1f)] public float paintHapticAmount = 0.2f;

        [Header("Tier up")]
        [Min(0)] public int tierBurstParticles = 24;
        [Min(0f)] public float tierBurstSpeedTiles = 4f;
        [Min(0.05f)] public float tierBurstSeconds = 0.6f;
        [Min(1f)] public float tierPopScale = 1.25f;
        [Min(0.05f)] public float tierPopSeconds = 0.3f;
        [Min(0)] public int tierHapticMilliseconds = 30;
        public Color tierBurstColor = new Color(0.95f, 0.75f, 0.3f, 1f);
    }
}
