using Game.Controls;
using UnityEngine;

namespace Game.Juice
{
    /// <summary>Small additive camera shake: kicks add amplitude up to a cap, which decays to zero.</summary>
    public sealed class CameraShake : MonoBehaviour
    {
        [SerializeField] private JuiceConfig config;
        [SerializeField] private CameraRig cameraRig;

        private float amplitude;

        public void Kick(float amount) => amplitude = Mathf.Min(config.shakeMax, amplitude + amount);

        private void Update()
        {
            if (amplitude <= 0f)
            {
                cameraRig.ShakeOffset = Vector2.zero;
                return;
            }
            cameraRig.ShakeOffset = Random.insideUnitCircle * amplitude;
            amplitude = Mathf.Max(0f, amplitude - config.shakeMax * Time.unscaledDeltaTime / config.shakeDecaySeconds);
        }
    }
}
