using Game.Audio;
using Game.Controls;
using Game.Core;
using UnityEngine;

namespace Game.Juice
{
    /// <summary>Short blips for buying an upgrade and switching modes.</summary>
    public sealed class UiBlipFeedback : MonoBehaviour
    {
        private const int MaxCopies = 2;

        [SerializeField] private GameBootstrap bootstrap;
        [SerializeField] private InputRouter router;
        [SerializeField] private SfxPlayer sfx;

        private void Start()
        {
            bootstrap.Upgrades.Bought += OnBought;
            router.ModeChanged += OnModeChanged;
        }

        private void OnDestroy()
        {
            if (bootstrap != null && bootstrap.Upgrades != null) bootstrap.Upgrades.Bought -= OnBought;
            if (router != null) router.ModeChanged -= OnModeChanged;
        }

        private void OnBought(UpgradeId _) => sfx.Play(SfxId.Buy, MaxCopies);

        private void OnModeChanged(InteractMode _) => sfx.Play(SfxId.ModeSwitch, MaxCopies);
    }
}
