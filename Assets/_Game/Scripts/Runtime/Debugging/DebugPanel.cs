using System.Globalization;
using Game.Core;
using Game.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Debugging
{
    /// <summary>
    /// Hidden playtest panel (hold the dirt counter for 2 s): live stats, per-upgrade cost/growth/step fields that
    /// edit the runtime config clone, and shortcuts. "Copy tuning JSON" puts the clone on the clipboard.
    /// </summary>
    public sealed class DebugPanel : MonoBehaviour
    {
        private const float OpenHoldSeconds = 2f;
        private const float StatsRefreshSeconds = 0.25f;
        private const float FpsSmoothing = 0.1f;
        private const int DirtGift = 1000;
        private const int FontSize = 34;
        private const float RowHeight = 78f;
        private const float Gap = 10f;

        private static readonly string[] UpgradeNames = { "Walk", "Dig", "Carry", "Cap", "Hatch" };
        private static readonly string[] Columns = { "base cost", "growth", "per level" };

        [SerializeField] private GameBootstrap bootstrap;
        [SerializeField] private StatsTracker stats;
        [SerializeField] private HudView hud;

        private readonly InputField[,] fields = new InputField[TuningConfig.UpgradeCount, 3];
        private GameObject root;
        private Text statsLabel;
        private float nextStatsAt;
        private float smoothedFrameSeconds = 1f / 60f;

        public bool IsOpen => root != null && root.activeSelf;

        public void Open()
        {
            RefreshFields();
            root.SetActive(true);
        }

        public void Close() => root.SetActive(false);

        private void Start()
        {
            BuildLayout();
            root.SetActive(false);
            var press = hud.Counter.gameObject.AddComponent<LongPress>();
            press.HoldSeconds = OpenHoldSeconds;
            press.Held += Open;
        }

        private void Update()
        {
            smoothedFrameSeconds = Mathf.Lerp(smoothedFrameSeconds, Time.unscaledDeltaTime, FpsSmoothing);
            if (!IsOpen || Time.unscaledTime < nextStatsAt) return;
            nextStatsAt = Time.unscaledTime + StatsRefreshSeconds;
            statsLabel.text = StatsText();
        }

        private string StatsText()
        {
            var colony = bootstrap.Colony;
            return $"Upgrades {bootstrap.Upgrades.TotalLevels}   Ants {colony.Count}/{colony.Cap}   " +
                   $"FPS {1f / Mathf.Max(smoothedFrameSeconds, 1e-4f):0}\n" +
                   $"Income {stats.Stats.IncomePerSecond(Time.time):0.0}/s   " +
                   $"Avg trip {stats.Stats.AverageTripTiles:0.0} tiles   Dirt on map {bootstrap.Grid.TotalDirt:N0}";
        }

        private void BuildLayout()
        {
            Image dim = UiKit.Panel("DebugOverlay", transform, UiKit.DimColor);
            UiKit.Place(dim.rectTransform, Vector2.zero, Vector2.one);
            root = dim.gameObject;

            Image card = UiKit.Panel("Card", dim.transform, UiKit.BarColor);
            RectTransform cardRect = UiKit.Place(card.rectTransform, new Vector2(0.03f, 0.12f), new Vector2(0.97f, 0.92f));

            statsLabel = UiKit.Label("Stats", cardRect, FontSize, TextAnchor.UpperLeft, UiKit.AccentColor);
            UiKit.Place(statsLabel.rectTransform, new Vector2(0f, 1f), Vector2.one, new Vector2(24f, -150f), new Vector2(-24f, -20f));

            float y = -170f;
            AddHeaderRow(cardRect, y);
            for (int i = 0; i < TuningConfig.UpgradeCount; i++)
            {
                y -= RowHeight + Gap;
                AddUpgradeRow(cardRect, y, i);
            }

            y -= RowHeight + Gap * 4f;
            AddButtonRow(cardRect, ref y, ("+1000 dirt", () => bootstrap.Wallet.Add(DirtGift)),
                ("Fill colony", () => bootstrap.Colony.SetAntCount(bootstrap.Colony.Cap)));
            AddButtonRow(cardRect, ref y, ("Max colony size", MaxColonySize), ("Reset colony", bootstrap.ResetColony));
            AddButtonRow(cardRect, ref y, ("Copy tuning JSON", CopyTuning), ("Close", Close));
        }

        private void AddHeaderRow(RectTransform parent, float y)
        {
            for (int c = 0; c < Columns.Length; c++)
            {
                Text header = UiKit.Label($"Header{c}", parent, FontSize, TextAnchor.MiddleCenter, UiKit.MutedTextColor);
                PlaceCell(header.rectTransform, y, c + 1);
                header.text = Columns[c];
            }
        }

        private void AddUpgradeRow(RectTransform parent, float y, int upgrade)
        {
            Text name = UiKit.Label($"Name{upgrade}", parent, FontSize, TextAnchor.MiddleLeft, UiKit.TextColor);
            PlaceCell(name.rectTransform, y, 0);
            name.text = UpgradeNames[upgrade];

            for (int c = 0; c < Columns.Length; c++)
            {
                int column = c;
                InputField field = UiKit.NumberField($"Field{upgrade}_{c}", parent, FontSize,
                    text => ApplyField(upgrade, column, text));
                PlaceCell((RectTransform)field.transform, y, c + 1);
                fields[upgrade, c] = field;
            }
        }

        private static void PlaceCell(RectTransform rect, float y, int column)
        {
            const float columns = 4f;
            float x0 = column / columns, x1 = (column + 1) / columns;
            UiKit.Place(rect, new Vector2(x0, 1f), new Vector2(x1, 1f), new Vector2(Gap * 2f, y - RowHeight), new Vector2(-Gap, y));
        }

        private static void AddButtonRow(RectTransform parent, ref float y, (string caption, UnityEngine.Events.UnityAction action) left,
            (string caption, UnityEngine.Events.UnityAction action) right)
        {
            Button a = UiKit.Button(left.caption, parent, left.caption, FontSize, left.action, out _);
            Button b = UiKit.Button(right.caption, parent, right.caption, FontSize, right.action, out _);
            UiKit.Place((RectTransform)a.transform, new Vector2(0f, 1f), new Vector2(0.5f, 1f), new Vector2(24f, y - RowHeight), new Vector2(-Gap, y));
            UiKit.Place((RectTransform)b.transform, new Vector2(0.5f, 1f), Vector2.one, new Vector2(Gap, y - RowHeight), new Vector2(-24f, y));
            y -= RowHeight + Gap * 2f;
        }

        private void ApplyField(int upgrade, int column, string text)
        {
            UpgradeDef def = bootstrap.Config.upgrades[upgrade];
            if (float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float value) && IsValid(def, column, value))
            {
                if (column == 0) def.baseCost = value;
                else if (column == 1) def.costGrowth = value;
                else def.perLevel = value;
                bootstrap.Config.upgrades[upgrade] = def;
            }
            RefreshFields();
        }

        private static bool IsValid(UpgradeDef def, int column, float value)
        {
            switch (column)
            {
                case 0: return value >= 0f;
                case 1: return value >= 1f;
                default: return !def.multiplicative || value > 0f;
            }
        }

        private void RefreshFields()
        {
            for (int i = 0; i < TuningConfig.UpgradeCount; i++)
            {
                UpgradeDef def = bootstrap.Config.upgrades[i];
                fields[i, 0].SetTextWithoutNotify(def.baseCost.ToString("0.##", CultureInfo.InvariantCulture));
                fields[i, 1].SetTextWithoutNotify(def.costGrowth.ToString("0.###", CultureInfo.InvariantCulture));
                fields[i, 2].SetTextWithoutNotify(def.perLevel.ToString("0.###", CultureInfo.InvariantCulture));
            }
        }

        private void MaxColonySize()
        {
            int[] levels = bootstrap.Upgrades.SnapshotLevels();
            levels[(int)UpgradeId.ColonyCap] = bootstrap.Config.Get(UpgradeId.ColonyCap).maxLevel;
            bootstrap.Upgrades.RestoreLevels(levels);
        }

        private void CopyTuning()
        {
            GUIUtility.systemCopyBuffer = JsonUtility.ToJson(bootstrap.Config, true);
            Debug.Log("Tuning JSON copied to the clipboard.");
        }
    }
}
