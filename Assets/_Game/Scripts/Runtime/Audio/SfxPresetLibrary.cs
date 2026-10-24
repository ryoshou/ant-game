namespace Game.Audio
{
    /// <summary>The starting sound design: one sfxr preset per SfxId. A new SfxPresets asset starts with these.</summary>
    public static class SfxPresetLibrary
    {
        public static SfxPreset[] Defaults() => new[]
        {
            Preset(SfxId.Whoosh, 11, 0.35f, new SfxrParams
            {
                wave = SfxrWave.Noise, baseFrequency = 0.55f, frequencySlide = -0.25f, attack = 0.15f, sustain = 0.15f,
                decay = 0.25f, lowPassCutoff = 0.35f, lowPassSweep = 0.25f, lowPassResonance = 0.3f, highPassCutoff = 0.15f,
            }),
            Preset(SfxId.Boom, 23, 0.55f, new SfxrParams
            {
                wave = SfxrWave.Noise, baseFrequency = 0.2f, frequencySlide = -0.08f, sustain = 0.3f, sustainPunch = 0.55f,
                decay = 0.5f,
            }),
            Preset(SfxId.MegaBlast, 31, 0.8f, new SfxrParams
            {
                wave = SfxrWave.Noise, baseFrequency = 0.12f, frequencySlide = -0.04f, sustain = 0.45f, sustainPunch = 0.75f,
                decay = 0.7f, vibratoDepth = 0.25f, vibratoSpeed = 0.2f, phaserOffset = 0.25f, phaserSweep = -0.1f,
                lowPassCutoff = 0.8f, lowPassSweep = -0.1f,
            }),
            Preset(SfxId.Thud, 41, 0.4f, new SfxrParams
            {
                wave = SfxrWave.Noise, baseFrequency = 0.3f, frequencySlide = -0.45f, sustain = 0.04f, decay = 0.14f,
                lowPassCutoff = 0.4f, lowPassResonance = 0.2f,
            }),
            Preset(SfxId.Squish, 53, 0.35f, new SfxrParams
            {
                wave = SfxrWave.Noise, baseFrequency = 0.5f, frequencySlide = -0.55f, sustain = 0.05f, sustainPunch = 0.35f,
                decay = 0.16f, lowPassCutoff = 0.3f, lowPassResonance = 0.6f, lowPassSweep = -0.25f,
            }),
            Preset(SfxId.MultiKill, 61, 0.5f, new SfxrParams
            {
                wave = SfxrWave.Noise, baseFrequency = 0.18f, frequencySlide = -0.12f, sustain = 0.18f, sustainPunch = 0.6f,
                decay = 0.38f, repeatSpeed = 0.55f, phaserOffset = 0.3f, phaserSweep = -0.2f, lowPassCutoff = 0.7f,
            }),
            Preset(SfxId.WardBeep, 71, 0.4f, new SfxrParams
            {
                wave = SfxrWave.Square, baseFrequency = 0.6f, sustain = 0.09f, decay = 0.06f, arpeggioMod = -0.25f,
                arpeggioSpeed = 0.55f, highPassCutoff = 0.1f,
            }),
            Preset(SfxId.BreachAlarm, 79, 0.5f, new SfxrParams
            {
                wave = SfxrWave.Square, baseFrequency = 0.45f, squareDuty = 0.3f, vibratoDepth = 0.45f, vibratoSpeed = 0.45f,
                sustain = 0.4f, decay = 0.25f, lowPassCutoff = 0.7f,
            }),
            Preset(SfxId.ClearChime, 83, 0.5f, new SfxrParams
            {
                wave = SfxrWave.Sine, baseFrequency = 0.55f, sustain = 0.12f, sustainPunch = 0.35f, decay = 0.5f,
                arpeggioMod = 0.5f, arpeggioSpeed = 0.6f,
            }),
            Preset(SfxId.MegaReady, 89, 0.45f, new SfxrParams
            {
                wave = SfxrWave.Square, baseFrequency = 0.33f, frequencySlide = 0.22f, squareDuty = 0.4f, vibratoDepth = 0.2f,
                vibratoSpeed = 0.5f, sustain = 0.22f, decay = 0.3f, repeatSpeed = 0.55f, lowPassCutoff = 0.85f,
            }),
            Preset(SfxId.ButtonClick, 97, 0.3f, new SfxrParams
            {
                wave = SfxrWave.Square, baseFrequency = 0.62f, squareDuty = 0.2f, sustain = 0.02f, decay = 0.05f,
                highPassCutoff = 0.15f,
            }),
            Preset(SfxId.Purchase, 101, 0.45f, new SfxrParams
            {
                wave = SfxrWave.Square, baseFrequency = 0.5f, squareDuty = 0.35f, sustain = 0.06f, sustainPunch = 0.5f,
                decay = 0.36f, arpeggioMod = 0.4f, arpeggioSpeed = 0.62f,
            }),
            Preset(SfxId.WaveHorn, 103, 0.5f, new SfxrParams
            {
                wave = SfxrWave.Sawtooth, baseFrequency = 0.22f, frequencySlide = 0.03f, attack = 0.2f, sustain = 0.4f,
                decay = 0.35f, vibratoDepth = 0.08f, vibratoSpeed = 0.3f, lowPassCutoff = 0.45f, lowPassResonance = 0.3f,
            }),
            Preset(SfxId.GameOver, 107, 0.5f, new SfxrParams
            {
                wave = SfxrWave.Square, baseFrequency = 0.42f, frequencySlide = -0.22f, squareDuty = 0.4f, sustain = 0.45f,
                decay = 0.5f, arpeggioMod = -0.35f, arpeggioSpeed = 0.5f, lowPassCutoff = 0.6f,
            }),
        };

        private static SfxPreset Preset(SfxId id, int seed, float volume, SfxrParams synth) =>
            new SfxPreset { id = id, seed = seed, volume = volume, synth = synth };
    }
}
