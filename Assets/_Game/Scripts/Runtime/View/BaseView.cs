using System;
using Game.Core;
using UnityEngine;

namespace Game.View
{
    /// <summary>
    /// The colony mound. Grows with the growth tier but stays inside the base footprint — visual only.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class BaseView : MonoBehaviour
    {
        [SerializeField] private GameBootstrap bootstrap;
        [SerializeField] private float[] footprintFractionByTier = { 0.7f, 0.8f, 0.9f, 1f };
        [SerializeField] private int sortingOrder = 5;

        private SpriteRenderer spriteRenderer;
        private int tier = -1;

        /// <summary>Raised with the new tier when it changes after startup.</summary>
        public event Action<int> TierChanged;

        public int Tier => tier;

        private void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sortingOrder = sortingOrder;
            transform.position = Vector3.zero;
            Apply(CurrentTier(), raiseEvent: false);
        }

        private void LateUpdate()
        {
            int current = CurrentTier();
            if (current != tier) Apply(current, raiseEvent: true);
        }

        private int CurrentTier() =>
            GrowthTiers.TierFor(bootstrap.Upgrades.TotalLevels, bootstrap.Config.tierThresholds);

        private void Apply(int newTier, bool raiseEvent)
        {
            bool isGrowing = newTier > tier;
            tier = newTier;
            spriteRenderer.sprite = PlaceholderSprites.Mound(tier);
            int index = Mathf.Clamp(tier, 0, footprintFractionByTier.Length - 1);
            float size = bootstrap.Config.baseSize * footprintFractionByTier[index];
            transform.localScale = new Vector3(size, size, 1f);
            if (raiseEvent && isGrowing) TierChanged?.Invoke(tier);
        }
    }
}
