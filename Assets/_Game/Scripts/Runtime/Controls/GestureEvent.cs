using UnityEngine;

namespace Game.Controls
{
    public enum GestureKind
    {
        StrokeBegin,
        StrokeMove,
        StrokeEnd,
        StrokeCancel,
        Pan,
        Pinch,
    }

    /// <summary>
    /// One parsed gesture step, in screen space. Strokes and Pan use From → To; Pinch uses To as the screen
    /// centre and Scale as this frame's finger distance ÷ last frame's.
    /// </summary>
    public readonly struct GestureEvent
    {
        public readonly GestureKind Kind;
        public readonly Vector2 From;
        public readonly Vector2 To;
        public readonly float Scale;

        public GestureEvent(GestureKind kind, Vector2 from, Vector2 to, float scale = 1f)
        {
            Kind = kind;
            From = from;
            To = to;
            Scale = scale;
        }
    }
}
