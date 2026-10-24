using Game.Debugging;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// One-time tip, the first time trips get long: painting dirt near the base is the best way to speed the colony up.
    /// </summary>
    public sealed class PaintHint : MonoBehaviour
    {
        private const string ShownKey = "hint.paint.shown";
        private const string Message = "Tip: paint dirt near the base to speed them up";
        private const float VisibleSeconds = 4f;
        private const int FontSize = 44;
        private const float TopOffset = 170f;
        private const float Height = 110f;

        [SerializeField] private GameBootstrap bootstrap;
        [SerializeField] private StatsTracker stats;

        private GameObject banner;
        private float hideAt = -1f;
        private bool hasShown;

        public bool IsVisible => banner != null && banner.activeSelf;

        private void Start()
        {
            hasShown = PlayerPrefs.GetInt(ShownKey, 0) == 1;
            Image background = UiKit.Panel("PaintHint", transform, UiKit.BarColor);
            UiKit.Place(background.rectTransform, new Vector2(0.05f, 1f), new Vector2(0.95f, 1f),
                new Vector2(0f, -TopOffset - Height), new Vector2(0f, -TopOffset));
            background.raycastTarget = false;
            Text label = UiKit.Label("Text", background.transform, FontSize, TextAnchor.MiddleCenter, UiKit.TextColor);
            label.text = Message;
            banner = background.gameObject;
            banner.SetActive(false);
        }

        private void Update()
        {
            if (IsVisible)
            {
                if (Time.unscaledTime >= hideAt) banner.SetActive(false);
                return;
            }
            if (hasShown || stats.Stats.AverageTripTiles <= bootstrap.Config.paintHintTripTiles) return;

            hasShown = true;
            PlayerPrefs.SetInt(ShownKey, 1);
            PlayerPrefs.Save();
            hideAt = Time.unscaledTime + VisibleSeconds;
            banner.SetActive(true);
        }
    }
}
