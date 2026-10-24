namespace Game.Audio
{
    /// <summary>The starting sound design: one sfxr preset per SfxId. A new SfxPresets asset starts with these.</summary>
    public static class SfxPresetLibrary
    {
        public static SfxPreset[] Defaults() => new[]
        {
            Preset(SfxId.Squish, 53, 0.45f, new SfxrParams
            {
                wave = SfxrWave.Noise, baseFrequency = 0.5f, frequencySlide = -0.55f, sustain = 0.05f, sustainPunch = 0.35f,
                decay = 0.16f, lowPassCutoff = 0.3f, lowPassResonance = 0.6f, lowPassSweep = -0.25f,
            }),
            Preset(SfxId.Paint, 41, 0.25f, new SfxrParams
            {
                wave = SfxrWave.Noise, baseFrequency = 0.3f, frequencySlide = -0.45f, sustain = 0.04f, decay = 0.14f,
                lowPassCutoff = 0.4f, lowPassResonance = 0.2f,
            }),
            Preset(SfxId.TierUp, 83, 0.55f, new SfxrParams
            {
                wave = SfxrWave.Sine, baseFrequency = 0.55f, sustain = 0.12f, sustainPunch = 0.35f, decay = 0.5f,
                arpeggioMod = 0.5f, arpeggioSpeed = 0.6f,
            }),
            Preset(SfxId.Buy, 101, 0.4f, new SfxrParams
            {
                wave = SfxrWave.Square, baseFrequency = 0.5f, squareDuty = 0.35f, sustain = 0.06f, sustainPunch = 0.5f,
                decay = 0.36f, arpeggioMod = 0.4f, arpeggioSpeed = 0.62f,
            }),
            Preset(SfxId.ModeSwitch, 97, 0.3f, new SfxrParams
            {
                wave = SfxrWave.Square, baseFrequency = 0.62f, squareDuty = 0.2f, sustain = 0.02f, decay = 0.05f,
                highPassCutoff = 0.15f,
            }),
        };

        private static SfxPreset Preset(SfxId id, int seed, float volume, SfxrParams synth) =>
            new SfxPreset { id = id, seed = seed, volume = volume, synth = synth };
    }
}
