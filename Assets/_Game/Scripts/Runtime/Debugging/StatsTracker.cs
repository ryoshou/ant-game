using UnityEngine;

namespace Game.Debugging
{
    /// <summary>Feeds <see cref="ColonyStats"/> from colony events for the debug panel and the paint hint.</summary>
    public sealed class StatsTracker : MonoBehaviour
    {
        [SerializeField] private GameBootstrap bootstrap;

        public ColonyStats Stats { get; } = new ColonyStats();

        private void Start()
        {
            bootstrap.Colony.Deposited += OnDeposited;
            bootstrap.Colony.TripCompleted += Stats.OnTrip;
        }

        private void OnDestroy()
        {
            if (bootstrap == null || bootstrap.Colony == null) return;
            bootstrap.Colony.Deposited -= OnDeposited;
            bootstrap.Colony.TripCompleted -= Stats.OnTrip;
        }

        private void OnDeposited(int amount) => Stats.OnDeposit(amount, Time.time);
    }
}
