using Game.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>The upgrade list. Opens from the HUD; tapping outside the card closes it.</summary>
    public sealed class UpgradePanel : MonoBehaviour
    {
        private const float RowHeight = 170f;
        private const float RowGap = 16f;
        private const float CardPadding = 30f;
        private const float TitleHeight = 110f;
        private const int TitleFontSize = 56;

        [SerializeField] private GameBootstrap bootstrap;

        private readonly UpgradeRow[] rows = new UpgradeRow[TuningConfig.UpgradeCount];
        private GameObject root;

        public bool IsOpen => root != null && root.activeSelf;

        public UpgradeRow RowFor(UpgradeId id) => rows[(int)id];

        public void Toggle()
        {
            if (IsOpen) Close();
            else Open();
        }

        public void Open()
        {
            RefreshAll();
            root.SetActive(true);
        }

        public void Close() => root.SetActive(false);

        private void Start()
        {
            Image dim = UiKit.Panel("UpgradeOverlay", transform, UiKit.DimColor);
            UiKit.Place(dim.rectTransform, Vector2.zero, Vector2.one);
            dim.gameObject.AddComponent<Button>().onClick.AddListener(Close);
            root = dim.gameObject;

            float cardHeight = TitleHeight + rows.Length * (RowHeight + RowGap) + CardPadding * 2f;
            Image card = UiKit.Panel("Card", dim.transform, UiKit.BarColor);
            card.gameObject.AddComponent<ClickBlocker>(); // taps on the card must not reach the dim's Close
            RectTransform cardRect = UiKit.Place(card.rectTransform, new Vector2(0.05f, 0.5f), new Vector2(0.95f, 0.5f),
                new Vector2(0f, -cardHeight / 2f), new Vector2(0f, cardHeight / 2f));

            Text title = UiKit.Label("Title", cardRect, TitleFontSize, TextAnchor.MiddleCenter, UiKit.AccentColor);
            UiKit.Place(title.rectTransform, new Vector2(0f, 1f), Vector2.one,
                new Vector2(0f, -TitleHeight - CardPadding), new Vector2(0f, -CardPadding));
            title.text = "Upgrades";

            for (int i = 0; i < rows.Length; i++)
            {
                Image rowImage = UiKit.Panel($"Row{(UpgradeId)i}", cardRect, UiKit.ButtonColor * 0.7f);
                float top = -CardPadding - TitleHeight - i * (RowHeight + RowGap);
                UiKit.Place(rowImage.rectTransform, new Vector2(0f, 1f), Vector2.one,
                    new Vector2(CardPadding, top - RowHeight), new Vector2(-CardPadding, top));
                rows[i] = rowImage.gameObject.AddComponent<UpgradeRow>();
                rows[i].Init(bootstrap, (UpgradeId)i);
            }

            bootstrap.Wallet.Changed += OnWalletChanged;
            bootstrap.Upgrades.Bought += OnBought;
            bootstrap.ColonyReset += RefreshAll;
            root.SetActive(false);
        }

        private void OnDestroy()
        {
            if (bootstrap == null || bootstrap.Wallet == null) return;
            bootstrap.Wallet.Changed -= OnWalletChanged;
            bootstrap.Upgrades.Bought -= OnBought;
            bootstrap.ColonyReset -= RefreshAll;
        }

        private void OnWalletChanged(int _)
        {
            if (IsOpen) RefreshAll();
        }

        private void OnBought(UpgradeId _) => RefreshAll();

        private void RefreshAll()
        {
            for (int i = 0; i < rows.Length; i++) rows[i].Refresh();
        }
    }
}
