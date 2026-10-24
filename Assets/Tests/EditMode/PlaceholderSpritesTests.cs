using Game.View;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public sealed class PlaceholderSpritesTests
    {
        [Test]
        public void Cache_RebuildsSpritesDestroyedSinceLastUse()
        {
            Object.DestroyImmediate(PlaceholderSprites.Ant(16));
            Object.DestroyImmediate(PlaceholderSprites.Circle(16));
            Object.DestroyImmediate(PlaceholderSprites.Mound(0));

            Assert.IsTrue(PlaceholderSprites.Ant(16) != null);
            Assert.IsTrue(PlaceholderSprites.Circle(16) != null);
            Assert.IsTrue(PlaceholderSprites.Mound(0) != null);
        }
    }
}
