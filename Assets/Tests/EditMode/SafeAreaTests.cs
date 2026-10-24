using Game.UI;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public sealed class SafeAreaTests
    {
        [Test]
        public void AnchorsFor_NotchedPortraitPhone_InsetsTopAndBottom()
        {
            // 1206 × 2622 screen with a 186 px top inset and 102 px bottom inset (iPhone 17-like).
            var safe = new Rect(0f, 102f, 1206f, 2622f - 186f - 102f);

            AnchorRange anchors = SafeAreaFitter.AnchorsFor(safe, 1206, 2622);

            Assert.AreEqual(0f, anchors.Min.x, 1e-5f);
            Assert.AreEqual(102f / 2622f, anchors.Min.y, 1e-5f);
            Assert.AreEqual(1f, anchors.Max.x, 1e-5f);
            Assert.AreEqual((2622f - 186f) / 2622f, anchors.Max.y, 1e-5f);
        }

        [Test]
        public void AnchorsFor_FullScreen_IsUnitRect()
        {
            AnchorRange anchors = SafeAreaFitter.AnchorsFor(new Rect(0f, 0f, 1080f, 1920f), 1080, 1920);

            Assert.AreEqual(Vector2.zero, anchors.Min);
            Assert.AreEqual(Vector2.one, anchors.Max);
        }

        [Test]
        public void AnchorsFor_ZeroSizedScreen_IsUnitRect()
        {
            AnchorRange anchors = SafeAreaFitter.AnchorsFor(new Rect(0f, 0f, 0f, 0f), 0, 0);

            Assert.AreEqual(Vector2.zero, anchors.Min);
            Assert.AreEqual(Vector2.one, anchors.Max);
        }
    }
}
