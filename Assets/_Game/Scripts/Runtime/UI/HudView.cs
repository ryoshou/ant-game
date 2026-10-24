using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>Top bar: the dirt counter and the Upgrades button.</summary>
    public sealed class HudView : MonoBehaviour
    {
        private const float BarHeight = 150f;
        private const int CounterFontSize = 64;
        private const int ButtonFontSize = 44;

        [SerializeField] private GameBootstrap bootstrap;
        [SerializeField] private UpgradePanel upgradePanel;

        private Text dirtLabel;

        public Button UpgradesButton { get; private set; }

        /// <summary>The counter's rect, for gestures that target it (the debug long-press).</summary>
        public RectTransform Counter { get; private set; }

        private void Start()
        {
            Image bar = UiKit.Panel("TopBar", transform, UiKit.BarColor);
            UiKit.Place(bar.rectTransform, new Vector2(0f, 1f), Vector2.one, new Vector2(0f, -BarHeight), Vector2.zero);

            Image counter = UiKit.Panel("DirtCounter", bar.transform, Color.clear);
            Counter = UiKit.Place(counter.rectTransform, Vector2.zero, new Vector2(0.6f, 1f), new Vector2(40f, 0f));
            dirtLabel = UiKit.Label("Value", counter.transform, CounterFontSize, TextAnchor.MiddleLeft, UiKit.AccentColor);

            UpgradesButton = UiKit.Button("UpgradesButton", bar.transform, "Upgrades", ButtonFontSize, upgradePanel.Toggle, out _);
            UiKit.Place((RectTransform)UpgradesButton.transform, new Vector2(0.62f, 0.15f), new Vector2(0.97f, 0.85f));

            bootstrap.Wallet.Changed += SetDirt;
            SetDirt(bootstrap.Wallet.Dirt);
        }

        private void OnDestroy()
        {
            if (bootstrap != null && bootstrap.Wallet != null) bootstrap.Wallet.Changed -= SetDirt;
        }

        private void SetDirt(int dirt) => dirtLabel.text = $"Dirt  {dirt:N0}";
    }
}
