using UnityEngine;

namespace Game.Debugging
{
    /// <summary>
    /// Live playtest numbers: income over the last 10 s and the average one-way trip over the last 20 trips.
    /// Ring buffers are preallocated; nothing allocates after construction.
    /// </summary>
    public sealed class ColonyStats
    {
        public const float IncomeWindowSeconds = 10f;
        public const int TripWindow = 20;
        private const int DepositCapacity = 1024;

        private readonly float[] depositTimes = new float[DepositCapacity];
        private readonly int[] depositAmounts = new int[DepositCapacity];
        private readonly float[] trips = new float[TripWindow];
        private int depositHead;
        private int depositCount;
        private int tripHead;
        private int tripCount;
        private float tripSum;

        public float AverageTripTiles => tripCount == 0 ? 0f : tripSum / tripCount;

        public void OnDeposit(int amount, float now)
        {
            depositTimes[depositHead] = now;
            depositAmounts[depositHead] = amount;
            depositHead = (depositHead + 1) % DepositCapacity;
            depositCount = Mathf.Min(depositCount + 1, DepositCapacity);
        }

        public float IncomePerSecond(float now)
        {
            int sum = 0;
            float since = now - IncomeWindowSeconds;
            for (int i = 0; i < depositCount; i++)
            {
                int index = (depositHead - 1 - i + DepositCapacity) % DepositCapacity;
                if (depositTimes[index] <= since) break;
                sum += depositAmounts[index];
            }
            return sum / IncomeWindowSeconds;
        }

        public void OnTrip(float tiles)
        {
            if (tripCount == TripWindow) tripSum -= trips[tripHead];
            else tripCount++;
            trips[tripHead] = tiles;
            tripSum += tiles;
            tripHead = (tripHead + 1) % TripWindow;
        }
    }
}
