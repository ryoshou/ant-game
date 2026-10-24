using UnityEngine;

namespace Game.UI
{
    /// <summary>Normalised anchors (0..1 of the screen).</summary>
    public readonly struct AnchorRange
    {
        public readonly Vector2 Min;
        public readonly Vector2 Max;

        public AnchorRange(Vector2 min, Vector2 max)
        {
            Min = min;
            Max = max;
        }
    }

    /// <summary>
    /// Fits this RectTransform to <see cref="Screen.safeArea"/> so HUD controls stay clear of the notch / Dynamic
    /// Island and the home indicator. Re-applies when the safe area or screen size changes.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        private Rect appliedArea;
        private Vector2Int appliedScreen;

        public static AnchorRange AnchorsFor(Rect safeArea, int screenWidth, int screenHeight)
        {
            if (screenWidth <= 0 || screenHeight <= 0) return new AnchorRange(Vector2.zero, Vector2.one);
            var size = new Vector2(screenWidth, screenHeight);
            return new AnchorRange(safeArea.min / size, safeArea.max / size);
        }

        private void Start() => Apply();

        private void Update()
        {
            if (Screen.safeArea != appliedArea || Screen.width != appliedScreen.x || Screen.height != appliedScreen.y)
                Apply();
        }

        private void Apply()
        {
            appliedArea = Screen.safeArea;
            appliedScreen = new Vector2Int(Screen.width, Screen.height);
            AnchorRange anchors = AnchorsFor(appliedArea, Screen.width, Screen.height);
            var rect = (RectTransform)transform;
            rect.anchorMin = anchors.Min;
            rect.anchorMax = anchors.Max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
