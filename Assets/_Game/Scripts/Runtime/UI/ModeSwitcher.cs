using Game.Controls;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>Bottom bar: Kill / Dirt / View. The current mode is always highlighted.</summary>
    public sealed class ModeSwitcher : MonoBehaviour
    {
        private const float BarHeight = 190f;
        private const int FontSize = 48;
        private const float Gap = 0.02f;

        private static readonly InteractMode[] Modes = { InteractMode.Kill, InteractMode.Dirt, InteractMode.View };
        private static readonly string[] Captions = { "Squish", "Dirt", "Look" };

        [SerializeField] private InputRouter router;

        private readonly Button[] buttons = new Button[3];

        public Button ButtonFor(InteractMode mode) => buttons[(int)mode];

        private void Start()
        {
            Image bar = UiKit.Panel("ModeBar", transform, UiKit.BarColor);
            UiKit.Place(bar.rectTransform, Vector2.zero, new Vector2(1f, 0f), Vector2.zero, new Vector2(0f, BarHeight));

            float width = (1f - Gap * (Modes.Length + 1)) / Modes.Length;
            for (int i = 0; i < Modes.Length; i++)
            {
                InteractMode mode = Modes[i];
                Button button = UiKit.Button($"{mode}Button", bar.transform, Captions[i], FontSize, () => router.SetMode(mode), out _);
                float x = Gap + i * (width + Gap);
                UiKit.Place((RectTransform)button.transform, new Vector2(x, 0.12f), new Vector2(x + width, 0.88f));
                buttons[(int)mode] = button;
            }

            router.ModeChanged += Highlight;
            Highlight(router.Mode);
        }

        private void OnDestroy()
        {
            if (router != null) router.ModeChanged -= Highlight;
        }

        private void Highlight(InteractMode current)
        {
            for (int i = 0; i < buttons.Length; i++)
                ((Image)buttons[i].targetGraphic).color = i == (int)current ? UiKit.AccentColor : UiKit.ButtonColor;
        }
    }
}
