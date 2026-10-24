using System.Collections.Generic;
using UnityEngine;

namespace Game.Controls
{
    /// <summary>
    /// Turns raw per-frame touch counts and positions into strokes (one finger) and pan/pinch (two fingers).
    /// A second finger cancels the stroke; after a two-finger gesture no stroke starts until every finger lifts.
    /// </summary>
    public sealed class GestureTracker
    {
        private int previousCount;
        private Vector2 previous0;
        private Vector2 previous1;
        private bool isStroking;
        private bool isLockedUntilRelease;

        public void Step(int touchCount, Vector2 p0, Vector2 p1, List<GestureEvent> eventsOut)
        {
            eventsOut.Clear();
            if (touchCount <= 0) StepNoFingers(eventsOut);
            else if (touchCount == 1) StepOneFinger(p0, eventsOut);
            else StepTwoFingers(p0, p1, eventsOut);

            previousCount = touchCount;
            previous0 = p0;
            previous1 = p1;
        }

        private void StepNoFingers(List<GestureEvent> eventsOut)
        {
            if (isStroking) eventsOut.Add(new GestureEvent(GestureKind.StrokeEnd, previous0, previous0));
            isStroking = false;
            isLockedUntilRelease = false;
        }

        private void StepOneFinger(Vector2 p0, List<GestureEvent> eventsOut)
        {
            if (isLockedUntilRelease) return;
            if (!isStroking)
            {
                isStroking = true;
                eventsOut.Add(new GestureEvent(GestureKind.StrokeBegin, p0, p0));
                return;
            }
            if (p0 != previous0) eventsOut.Add(new GestureEvent(GestureKind.StrokeMove, previous0, p0));
        }

        private void StepTwoFingers(Vector2 p0, Vector2 p1, List<GestureEvent> eventsOut)
        {
            if (isStroking)
            {
                isStroking = false;
                eventsOut.Add(new GestureEvent(GestureKind.StrokeCancel, previous0, previous0));
            }
            isLockedUntilRelease = true;
            if (previousCount < 2) return;

            Vector2 previousMid = (previous0 + previous1) * 0.5f;
            Vector2 mid = (p0 + p1) * 0.5f;
            if (mid != previousMid) eventsOut.Add(new GestureEvent(GestureKind.Pan, previousMid, mid));

            float previousDistance = Vector2.Distance(previous0, previous1);
            float distance = Vector2.Distance(p0, p1);
            if (previousDistance > 0f && !Mathf.Approximately(distance, previousDistance))
                eventsOut.Add(new GestureEvent(GestureKind.Pinch, mid, mid, distance / previousDistance));
        }
    }
}
