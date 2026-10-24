using System;
using System.Collections.Generic;
using Game.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace Game.Controls
{
    /// <summary>
    /// Reads touches (mouse in the editor), parses them with <see cref="GestureTracker"/>, and applies them by mode:
    /// Kill squishes, Dirt paints, View pans. Two fingers (or the scroll wheel) always pan and zoom.
    /// </summary>
    public sealed class InputRouter : MonoBehaviour
    {
        private const float ScrollZoomStep = 1.1f;
        private const int ListCapacity = 64;

        [SerializeField] private GameBootstrap bootstrap;
        [SerializeField] private CameraRig cameraRig;

        private readonly GestureTracker tracker = new GestureTracker();
        private readonly List<GestureEvent> events = new List<GestureEvent>(8);
        private readonly List<Vector2Int> cells = new List<Vector2Int>(ListCapacity);
        private readonly List<RaycastResult> uiHits = new List<RaycastResult>(8);
        private PointerEventData pointerData;
        private bool isIgnoringStroke;

        public InteractMode Mode { get; private set; } = InteractMode.Kill;

        public event Action<InteractMode> ModeChanged;

        /// <summary>Number of tiles filled by one paint step (only raised when > 0).</summary>
        public event Action<int> CellsPainted;

        public void SetMode(InteractMode mode)
        {
            if (mode == Mode) return;
            Mode = mode;
            ModeChanged?.Invoke(mode);
        }

        private void OnEnable() => EnhancedTouchSupport.Enable();

        private void OnDisable() => EnhancedTouchSupport.Disable();

        private void Update()
        {
            ReadPointers(out int count, out Vector2 p0, out Vector2 p1);
            tracker.Step(count, p0, p1, events);
            for (int i = 0; i < events.Count; i++) Handle(events[i]);
            HandleScrollZoom();
        }

        private void ReadPointers(out int count, out Vector2 p0, out Vector2 p1)
        {
            p0 = p1 = default;
            var touches = Touch.activeTouches;
            count = Mathf.Min(touches.Count, 2);
            if (count > 0) p0 = touches[0].screenPosition;
            if (count > 1) p1 = touches[1].screenPosition;
            if (count > 0) return;

            Mouse mouse = Mouse.current;
            if (mouse == null || !mouse.leftButton.isPressed) return;
            count = 1;
            p0 = mouse.position.ReadValue();
        }

        private void HandleScrollZoom()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null) return;
            float scroll = mouse.scroll.ReadValue().y;
            if (Mathf.Approximately(scroll, 0f)) return;
            cameraRig.PinchZoom(mouse.position.ReadValue(), scroll > 0f ? ScrollZoomStep : 1f / ScrollZoomStep);
        }

        internal void Handle(GestureEvent e)
        {
            switch (e.Kind)
            {
                case GestureKind.StrokeBegin:
                    isIgnoringStroke = IsOverUi(e.To);
                    if (!isIgnoringStroke) ApplyStroke(e.To, e.To);
                    break;
                case GestureKind.StrokeMove:
                    if (!isIgnoringStroke) ApplyStroke(e.From, e.To);
                    break;
                case GestureKind.StrokeEnd:
                case GestureKind.StrokeCancel:
                    isIgnoringStroke = false;
                    break;
                case GestureKind.Pan:
                    cameraRig.Pan(e.To - e.From);
                    break;
                case GestureKind.Pinch:
                    cameraRig.PinchZoom(e.To, e.Scale);
                    break;
            }
        }

        /// <summary>Applies one stroke step between two screen points according to the current mode.</summary>
        internal void ApplyStroke(Vector2 fromScreen, Vector2 toScreen)
        {
            if (Mode == InteractMode.View)
            {
                if (fromScreen != toScreen) cameraRig.Pan(toScreen - fromScreen);
                return;
            }

            TuningConfig config = bootstrap.Config;
            Vector2 a = cameraRig.ScreenToWorld(fromScreen);
            Vector2 b = cameraRig.ScreenToWorld(toScreen);
            if (Mode == InteractMode.Kill)
            {
                bootstrap.Colony.KillWithin(a, b, config.killRadiusTiles);
                return;
            }

            BrushStroke.CellsAlong(a, b, config.brushRadiusTiles, bootstrap.Grid.Width, bootstrap.Grid.Height, cells);
            int painted = 0;
            for (int i = 0; i < cells.Count; i++)
                if (bootstrap.Grid.Fill(cells[i])) painted++;
            if (painted > 0) CellsPainted?.Invoke(painted);
        }

        private bool IsOverUi(Vector2 screen)
        {
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null) return false;
            pointerData ??= new PointerEventData(eventSystem);
            pointerData.position = screen;
            eventSystem.RaycastAll(pointerData, uiHits);
            return uiHits.Count > 0;
        }
    }
}
