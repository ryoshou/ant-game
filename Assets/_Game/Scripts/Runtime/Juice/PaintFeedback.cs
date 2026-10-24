using Game.Audio;
using Game.Controls;
using UnityEngine;

namespace Game.Juice
{
    /// <summary>A soft, rate-gated thud and tick while dirt is being painted back.</summary>
    public sealed class PaintFeedback : MonoBehaviour
    {
        private const int MaxCopies = 2;

        [SerializeField] private InputRouter router;
        [SerializeField] private JuiceConfig config;
        [SerializeField] private SfxPlayer sfx;

        private readonly RateGate soundGate = new RateGate();
        private readonly RateGate hapticGate = new RateGate();

        private void Start() => router.CellsPainted += OnCellsPainted;

        private void OnDestroy()
        {
            if (router != null) router.CellsPainted -= OnCellsPainted;
        }

        private void OnCellsPainted(int count)
        {
            float now = Time.unscaledTime;
            if (soundGate.TryPass(now, config.paintSoundInterval))
                sfx.Play(SfxId.Paint, MaxCopies, Random.Range(config.paintPitchRange.x, config.paintPitchRange.y));
            if (hapticGate.TryPass(now, config.paintHapticInterval))
                HapticsBridge.Play(HapticKind.Light, new HapticPulse(config.paintHapticMilliseconds, config.paintHapticAmount));
        }
    }
}
