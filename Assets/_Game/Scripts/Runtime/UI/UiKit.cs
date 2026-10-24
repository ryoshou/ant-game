using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// Builds placeholder uGUI elements in code (reference resolution 1080 × 1920). Keeps the scene to a Canvas
    /// plus components; all layout lives here and in the views that call it.
    /// </summary>
    public static class UiKit
    {
        public static readonly Color BarColor = new Color(0.08f, 0.05f, 0.03f, 0.85f);
        public static readonly Color ButtonColor = new Color(0.30f, 0.21f, 0.12f, 1f);
        public static readonly Color AccentColor = new Color(0.93f, 0.67f, 0.22f, 1f);
        public static readonly Color TextColor = new Color(0.98f, 0.94f, 0.86f, 1f);
        public static readonly Color MutedTextColor = new Color(0.75f, 0.68f, 0.58f, 1f);
        public static readonly Color DimColor = new Color(0f, 0f, 0f, 0.55f);

        private static Font font;

        public static Font Font => font != null ? font : font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        public static RectTransform Rect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = parent.gameObject.layer;
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            return rect;
        }

        /// <summary>Anchors in 0..1 of the parent, offsets in canvas units.</summary>
        public static RectTransform Place(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin = default, Vector2 offsetMax = default)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            return rect;
        }

        public static Image Panel(string name, Transform parent, Color color)
        {
            var image = Rect(name, parent).gameObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        public static Text Label(string name, Transform parent, int size, TextAnchor alignment, Color color)
        {
            var text = Rect(name, parent).gameObject.AddComponent<Text>();
            text.font = Font;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = color;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            Place(text.rectTransform, Vector2.zero, Vector2.one);
            return text;
        }

        public static Button Button(string name, Transform parent, string caption, int fontSize, UnityAction onClick,
            out Text label)
        {
            Image background = Panel(name, parent, ButtonColor);
            var button = background.gameObject.AddComponent<Button>();
            button.targetGraphic = background;
            button.onClick.AddListener(onClick);
            label = Label("Label", background.transform, fontSize, TextAnchor.MiddleCenter, TextColor);
            label.text = caption;
            return button;
        }
    }
}
