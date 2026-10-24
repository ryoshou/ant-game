using Game.Audio;
using NUnit.Framework;

namespace Game.Tests
{
    public sealed class ScaffoldTests
    {
        [Test]
        public void SfxrSynth_RendersNonEmptyClip()
        {
            float[] samples = SfxrSynth.Render(new SfxrParams(), 1);

            Assert.Greater(samples.Length, 0);
        }
    }
}
