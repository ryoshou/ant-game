using System.Collections.Generic;
using Game.Core;
using Game.Sim;
using UnityEngine;

namespace Game.View
{
    /// <summary>One pooled SpriteRenderer per live ant, synced from the colony every frame.</summary>
    public sealed class AntViews : MonoBehaviour
    {
        private const int SpritePixels = 32;
        private const int PoolCapacity = 64;

        [SerializeField] private GameBootstrap bootstrap;
        [SerializeField, Min(0.1f)] private float antLengthTiles = 0.8f;
        [SerializeField] private int sortingOrder = 10;

        private readonly Dictionary<int, SpriteRenderer> live = new Dictionary<int, SpriteRenderer>(PoolCapacity);
        private readonly Dictionary<int, int> seenFrame = new Dictionary<int, int>(PoolCapacity);
        private readonly Stack<SpriteRenderer> pool = new Stack<SpriteRenderer>(PoolCapacity);
        private readonly List<int> stale = new List<int>(PoolCapacity);
        private Sprite antSprite;

        private void Start() => antSprite = PlaceholderSprites.Ant(SpritePixels);

        private void LateUpdate()
        {
            TuningConfig config = bootstrap.Config;
            int tier = GrowthTiers.TierFor(bootstrap.Upgrades.TotalLevels, config.tierThresholds);
            float scale = antLengthTiles * config.tierAntScale[tier];
            Color color = config.tierAntColor[tier];
            int frame = Time.frameCount;

            IReadOnlyList<Ant> ants = bootstrap.Colony.Ants;
            for (int i = 0; i < ants.Count; i++)
            {
                Ant ant = ants[i];
                SpriteRenderer view = ViewFor(ant.Id);
                seenFrame[ant.Id] = frame;
                Transform t = view.transform;
                t.localPosition = new Vector3(ant.Position.x, ant.Position.y, 0f);
                t.localRotation = Quaternion.Euler(0f, 0f, ant.Heading * Mathf.Rad2Deg - 90f);
                t.localScale = new Vector3(scale, scale, 1f);
                view.color = color;
            }

            ReleaseStale(frame);
        }

        private SpriteRenderer ViewFor(int antId)
        {
            if (live.TryGetValue(antId, out SpriteRenderer view)) return view;
            view = pool.Count > 0 ? pool.Pop() : CreateView();
            view.gameObject.SetActive(true);
            live[antId] = view;
            return view;
        }

        private SpriteRenderer CreateView()
        {
            var go = new GameObject("Ant");
            go.transform.SetParent(transform, false);
            var view = go.AddComponent<SpriteRenderer>();
            view.sprite = antSprite;
            view.sortingOrder = sortingOrder;
            return view;
        }

        private void ReleaseStale(int frame)
        {
            stale.Clear();
            foreach (KeyValuePair<int, int> entry in seenFrame)
                if (entry.Value != frame) stale.Add(entry.Key);

            for (int i = 0; i < stale.Count; i++)
            {
                int id = stale[i];
                SpriteRenderer view = live[id];
                view.gameObject.SetActive(false);
                pool.Push(view);
                live.Remove(id);
                seenFrame.Remove(id);
            }
        }
    }
}
