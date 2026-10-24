using System;

namespace Game.Core
{
    /// <summary>The colony's deposited dirt — the only currency.</summary>
    public sealed class Wallet
    {
        public int Dirt { get; private set; }

        /// <summary>Raised with the new balance after every change.</summary>
        public event Action<int> Changed;

        public void Add(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount), amount, "Cannot add negative dirt.");
            if (amount == 0) return;
            Dirt += amount;
            Changed?.Invoke(Dirt);
        }

        public bool TrySpend(int amount)
        {
            if (amount < 0 || amount > Dirt) return false;
            Dirt -= amount;
            Changed?.Invoke(Dirt);
            return true;
        }

        public void Reset()
        {
            Dirt = 0;
            Changed?.Invoke(Dirt);
        }
    }
}
