using Game.View;
using UnityEngine;

namespace Game.Juice
{
    /// <summary>Fixed pool of fading splat marks on the ground. When full, the oldest splat is reused.</summary>
    public sealed class SplatPool : MonoBehaviour
    {
        private const int SpritePixels = 32;
        private const int SortingOrder = 1;

        [SerializeField] private JuiceConfig config;

        private SpriteRenderer[] splats;
        private float[] bornAt;
        private int next;

        public int ActiveCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < splats.Length; i++) if (splats[i].enabled) count++;
                return count;
            }
        }

        private void Awake()
        {
            int size = Mathf.Max(1, config.splatPoolSize);
            splats = new SpriteRenderer[size];
            bornAt = new float[size];
            Sprite sprite = PlaceholderSprites.Circle(SpritePixels);
            for (int i = 0; i < size; i++)
            {
                var go = new GameObject("Splat");
                go.transform.SetParent(transform, false);
                splats[i] = go.AddComponent<SpriteRenderer>();
                splats[i].sprite = sprite;
                splats[i].sortingOrder = SortingOrder;
                splats[i].enabled = false;
            }
        }

        public void Spawn(Vector2 position)
        {
            SpriteRenderer splat = splats[next];
            bornAt[next] = Time.time;
            next = (next + 1) % splats.Length;

            float size = Random.Range(config.splatSizeTiles.x, config.splatSizeTiles.y);
            Transform t = splat.transform;
            t.localPosition = new Vector3(position.x, position.y, 0f);
            t.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
            t.localScale = new Vector3(size, size * Random.Range(0.7f, 1f), 1f);
            splat.color = config.splatColor;
            splat.enabled = true;
        }

        private void Update()
        {
            float now = Time.time;
            float fadeStart = config.splatLifetime - config.splatFadeSeconds;
            for (int i = 0; i < splats.Length; i++)
            {
                SpriteRenderer splat = splats[i];
                if (!splat.enabled) continue;
                float age = now - bornAt[i];
                if (age >= config.splatLifetime)
                {
                    splat.enabled = false;
                    continue;
                }
                if (age <= fadeStart || config.splatFadeSeconds <= 0f) continue;
                Color color = config.splatColor;
                color.a *= 1f - (age - fadeStart) / config.splatFadeSeconds;
                splat.color = color;
            }
        }
    }
}
