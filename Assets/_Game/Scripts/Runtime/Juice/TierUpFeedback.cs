using Game.Audio;
using Game.View;
using UnityEngine;

namespace Game.Juice
{
    /// <summary>When the colony reaches a new growth tier: a chime, a heavy buzz, a burst, and the mound pops.</summary>
    public sealed class TierUpFeedback : MonoBehaviour
    {
        private const int SpritePixels = 16;
        private const int SortingOrder = 20;
        private const float ParticleSizeTiles = 0.35f;

        [SerializeField] private BaseView baseView;
        [SerializeField] private JuiceConfig config;
        [SerializeField] private SfxPlayer sfx;

        private SpriteRenderer[] particles;
        private Vector2[] velocities;
        private float burstAge = float.MaxValue;
        private float popAge = float.MaxValue;
        private Vector3 baseScale;

        private void Awake()
        {
            int count = config.tierBurstParticles;
            particles = new SpriteRenderer[count];
            velocities = new Vector2[count];
            Sprite sprite = PlaceholderSprites.Circle(SpritePixels);
            for (int i = 0; i < count; i++)
            {
                var go = new GameObject("TierParticle");
                go.transform.SetParent(transform, false);
                particles[i] = go.AddComponent<SpriteRenderer>();
                particles[i].sprite = sprite;
                particles[i].sortingOrder = SortingOrder;
                particles[i].enabled = false;
            }
        }

        private void Start() => baseView.TierChanged += OnTierChanged;

        private void OnDestroy()
        {
            if (baseView != null) baseView.TierChanged -= OnTierChanged;
        }

        private void OnTierChanged(int tier)
        {
            sfx.Play(SfxId.TierUp, 1);
            HapticsBridge.Play(HapticKind.Heavy, new HapticPulse(config.tierHapticMilliseconds, 1f));

            Vector3 origin = baseView.transform.position;
            for (int i = 0; i < particles.Length; i++)
            {
                float angle = (i + Random.value * 0.5f) * Mathf.PI * 2f / particles.Length;
                velocities[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * config.tierBurstSpeedTiles * Random.Range(0.6f, 1.2f);
                particles[i].transform.localPosition = origin;
                particles[i].transform.localScale = Vector3.one * ParticleSizeTiles;
                particles[i].enabled = true;
            }
            burstAge = 0f;
            baseScale = baseView.transform.localScale;
            popAge = 0f;
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            UpdateBurst(dt);
            UpdatePop(dt);
        }

        private void UpdateBurst(float dt)
        {
            if (burstAge > config.tierBurstSeconds) return;
            burstAge += dt;
            bool isDone = burstAge > config.tierBurstSeconds;
            Color color = config.tierBurstColor;
            color.a *= 1f - Mathf.Clamp01(burstAge / config.tierBurstSeconds);
            for (int i = 0; i < particles.Length; i++)
            {
                if (isDone)
                {
                    particles[i].enabled = false;
                    continue;
                }
                Transform t = particles[i].transform;
                t.localPosition += (Vector3)(velocities[i] * dt);
                particles[i].color = color;
            }
        }

        private void UpdatePop(float dt)
        {
            if (popAge > config.tierPopSeconds) return;
            popAge += dt;
            float t = Mathf.Clamp01(popAge / config.tierPopSeconds);
            float eased = 1f - (1f - t) * (1f - t);
            baseView.transform.localScale = baseScale * Mathf.Lerp(config.tierPopScale, 1f, eased);
        }
    }
}
