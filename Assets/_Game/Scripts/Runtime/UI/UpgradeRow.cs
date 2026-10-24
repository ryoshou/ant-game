using Game.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>One upgrade: name, level, current → next value, and a Buy button showing the cost (or MAX).</summary>
    public sealed class UpgradeRow : MonoBehaviour
    {
        private const int NameFontSize = 46;
        private const int DetailFontSize = 36;
        private const int BuyFontSize = 40;

        private static readonly string[] Names = { "Walk speed", "Dig speed", "Carry amount", "Colony size", "Hatch speed" };
        private static readonly string[] Units = { "tiles/s", "dirt/s", "dirt", "ants", "s/ant" };

        private GameBootstrap bootstrap;
        private UpgradeId id;
        private Text detail;
        private Text buyLabel;

        public Button BuyButton { get; private set; }
        public UpgradeId Id => id;

        public void Init(GameBootstrap owner, UpgradeId upgrade)
        {
            bootstrap = owner;
            id = upgrade;

            Text title = UiKit.Label("Name", transform, NameFontSize, TextAnchor.UpperLeft, UiKit.TextColor);
            UiKit.Place(title.rectTransform, new Vector2(0.03f, 0.5f), new Vector2(0.65f, 0.95f));
            title.text = Names[(int)id];

            detail = UiKit.Label("Detail", transform, DetailFontSize, TextAnchor.LowerLeft, UiKit.MutedTextColor);
            UiKit.Place(detail.rectTransform, new Vector2(0.03f, 0.05f), new Vector2(0.65f, 0.5f));

            BuyButton = UiKit.Button("Buy", transform, string.Empty, BuyFontSize, Buy, out buyLabel);
            UiKit.Place((RectTransform)BuyButton.transform, new Vector2(0.68f, 0.12f), new Vector2(0.97f, 0.88f));
            Refresh();
        }

        public void Refresh()
        {
            UpgradeBook upgrades = bootstrap.Upgrades;
            UpgradeDef def = bootstrap.Config.Get(id);
            int level = upgrades.Level(id);
            string unit = Units[(int)id];

            if (upgrades.IsMaxed(id))
            {
                detail.text = $"Lv {level}/{def.maxLevel}   {Format(def.ValueAt(level))} {unit}";
                buyLabel.text = "MAX";
                BuyButton.interactable = false;
                return;
            }

            int cost = upgrades.NextCost(id);
            detail.text = $"Lv {level}/{def.maxLevel}   {Format(def.ValueAt(level))} → {Format(def.ValueAt(level + 1))} {unit}";
            buyLabel.text = $"{cost:N0}";
            BuyButton.interactable = bootstrap.Wallet.Dirt >= cost;
        }

        private void Buy() => bootstrap.Upgrades.TryBuy(id, bootstrap.Wallet);

        private static string Format(float value) => Mathf.Approximately(value, Mathf.Round(value)) ? $"{value:0}" : $"{value:0.0}";
    }
}
