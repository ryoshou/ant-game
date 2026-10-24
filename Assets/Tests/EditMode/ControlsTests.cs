using System.Collections.Generic;
using System.Linq;
using Game.Controls;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public sealed class ControlsTests
    {
        private readonly List<GestureEvent> events = new List<GestureEvent>();

        private List<GestureKind> Kinds() => events.Select(e => e.Kind).ToList();

        [Test]
        public void OneFingerTap_BeginsThenEnds()
        {
            var tracker = new GestureTracker();

            tracker.Step(1, new Vector2(10, 20), default, events);
            Assert.AreEqual(new[] { GestureKind.StrokeBegin }, Kinds());
            Assert.AreEqual(new Vector2(10, 20), events[0].To);

            tracker.Step(0, default, default, events);
            Assert.AreEqual(new[] { GestureKind.StrokeEnd }, Kinds());
        }

        [Test]
        public void OneFingerDrag_EmitsMoveWithPreviousAndCurrent()
        {
            var tracker = new GestureTracker();
            tracker.Step(1, new Vector2(0, 0), default, events);

            tracker.Step(1, new Vector2(5, 0), default, events);

            Assert.AreEqual(new[] { GestureKind.StrokeMove }, Kinds());
            Assert.AreEqual(new Vector2(0, 0), events[0].From);
            Assert.AreEqual(new Vector2(5, 0), events[0].To);
        }

        [Test]
        public void OneFingerHeldStill_EmitsNothing()
        {
            var tracker = new GestureTracker();
            tracker.Step(1, new Vector2(3, 3), default, events);

            tracker.Step(1, new Vector2(3, 3), default, events);

            Assert.AreEqual(0, events.Count);
        }

        [Test]
        public void SecondFinger_CancelsStroke_ThenPans()
        {
            var tracker = new GestureTracker();
            tracker.Step(1, new Vector2(0, 0), default, events);
            Assert.AreEqual(new[] { GestureKind.StrokeBegin }, Kinds());

            tracker.Step(2, new Vector2(0, 0), new Vector2(100, 0), events);
            Assert.Contains(GestureKind.StrokeCancel, Kinds());

            tracker.Step(2, new Vector2(10, 0), new Vector2(110, 0), events);
            GestureEvent pan = events.Single(e => e.Kind == GestureKind.Pan);
            Assert.AreEqual(new Vector2(10, 0), pan.To - pan.From);
            Assert.IsFalse(Kinds().Contains(GestureKind.Pinch));
        }

        [Test]
        public void LiftingOneOfTwoFingers_DoesNotStartStroke()
        {
            var tracker = new GestureTracker();
            tracker.Step(2, new Vector2(0, 0), new Vector2(100, 0), events);

            tracker.Step(1, new Vector2(0, 0), default, events);
            Assert.AreEqual(0, events.Count);
            tracker.Step(1, new Vector2(30, 0), default, events);
            Assert.AreEqual(0, events.Count);

            tracker.Step(0, default, default, events);
            Assert.AreEqual(0, events.Count);
            tracker.Step(1, new Vector2(5, 5), default, events);
            Assert.AreEqual(new[] { GestureKind.StrokeBegin }, Kinds());
        }

        [Test]
        public void Pinch_ReportsDistanceRatio()
        {
            var tracker = new GestureTracker();
            tracker.Step(2, new Vector2(0, 0), new Vector2(100, 0), events);

            tracker.Step(2, new Vector2(-50, 0), new Vector2(150, 0), events);

            GestureEvent pinch = events.Single(e => e.Kind == GestureKind.Pinch);
            Assert.AreEqual(2f, pinch.Scale, 1e-4f);
            Assert.AreEqual(new Vector2(50, 0), pinch.To);
        }

        [Test]
        public void CellsAlong_SinglePoint_HitsCellUnderPoint()
        {
            var cells = new List<Vector2Int>();

            BrushStroke.CellsAlong(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), 0.4f, 64, 64, cells);

            Assert.AreEqual(new[] { new Vector2Int(32, 32) }, cells);
        }

        [Test]
        public void CellsAlong_LongSegment_HasNoGaps()
        {
            var cells = new List<Vector2Int> { new Vector2Int(-1, -1) };

            BrushStroke.CellsAlong(new Vector2(-20f, 0.5f), new Vector2(20f, 0.5f), 0.7f, 64, 64, cells);

            Assert.IsTrue(cells.All(c => c.y == 32), "only row 32 is within 0.7 of y = 0.5");
            Assert.AreEqual(cells.Count, cells.Distinct().Count());
            CollectionAssert.AreEquivalent(Enumerable.Range(11, 42).Select(x => new Vector2Int(x, 32)), cells);
        }

        [Test]
        public void CellsAlong_ClipsToMap()
        {
            var cells = new List<Vector2Int>();

            BrushStroke.CellsAlong(new Vector2(-40f, -40f), new Vector2(-31.5f, -31.5f), 0.4f, 64, 64, cells);

            Assert.AreEqual(new[] { new Vector2Int(0, 0) }, cells);
        }

        [Test]
        public void OrthoSize_ShowsRequestedTilesAcross()
        {
            Assert.AreEqual(16f, CameraMath.OrthoSizeForTilesAcross(18f, 0.5625f), 1e-4f);
            Assert.AreEqual(8f, CameraMath.ClampTilesAcross(2f, 8f, 64f));
            Assert.AreEqual(64f, CameraMath.ClampTilesAcross(99f, 8f, 64f));
        }

        [Test]
        public void ClampCenter_KeepsViewInsideMap()
        {
            // 18 tiles wide (half 9) × 32 tall (half 16) on a 64 × 64 map centred on the origin.
            Vector2 clamped = CameraMath.ClampCenter(new Vector2(100f, -100f), 16f, 0.5625f, 64, 64);

            Assert.AreEqual(23f, clamped.x, 1e-4f);
            Assert.AreEqual(-16f, clamped.y, 1e-4f);
        }

        [Test]
        public void ClampCenter_ViewLargerThanMap_CentresAxis()
        {
            Vector2 clamped = CameraMath.ClampCenter(new Vector2(5f, 5f), 64f, 0.5625f, 64, 64);

            Assert.AreEqual(Vector2.zero, clamped);
        }

        [Test]
        public void ZoomAround_KeepsFocusPointFixed()
        {
            var center = new Vector2(0f, 0f);
            var focus = new Vector2(4f, 2f);

            Vector2 zoomed = CameraMath.ZoomAround(center, focus, 10f, 5f);

            // The focus sat at offset (4, 2) of a 10-unit view; at 5 units it must sit at the same screen fraction.
            Assert.AreEqual((focus - center) / 10f, (focus - zoomed) / 5f);
        }
    }
}
