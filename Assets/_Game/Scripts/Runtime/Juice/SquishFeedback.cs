using Game.Audio;
using Game.Sim;
using UnityEngine;

namespace Game.Juice
{
    /// <summary>
    /// Every dead ant leaves a splat; sound, haptic and shake are rate-gated so a drag through a crowd stays punchy
    /// instead of turning into noise.
    /// </summary>
    public sealed class SquishFeedback : MonoBehaviour
    {
        [SerializeField] private GameBootstrap bootstrap;
        [SerializeField] private JuiceConfig config;
        [SerializeField] private SfxPlayer sfx;
        [SerializeField] private SplatPool splats;
        [SerializeField] private CameraShake shake;

        private readonly RateGate soundGate = new RateGate();
        private readonly RateGate hapticGate = new RateGate();

        private void Start() => bootstrap.Colony.AntKilled += OnAntKilled;

        private void OnDestroy()
        {
            if (bootstrap != null && bootstrap.Colony != null) bootstrap.Colony.AntKilled -= OnAntKilled;
        }

        private void OnAntKilled(Ant ant)
        {
            splats.Spawn(ant.Position);
            float now = Time.unscaledTime;
            if (soundGate.TryPass(now, config.squishSoundInterval))
            {
                float pitch = Random.Range(config.squishPitchRange.x, config.squishPitchRange.y);
                sfx.Play(SfxId.Squish, config.squishMaxCopies, pitch);
                shake.Kick(config.shakePerKill);
            }
            if (hapticGate.TryPass(now, config.squishHapticInterval))
                HapticsBridge.Play(HapticKind.Light, new HapticPulse(config.squishHapticMilliseconds, config.squishHapticAmount));
        }
    }
}
