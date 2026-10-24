using System;

namespace Game.Services
{
    /// <summary>The single save slot, serialised with JsonUtility. <c>grid</c> is Base64 of DirtGrid.Snapshot().</summary>
    [Serializable]
    public sealed class SaveData
    {
        public const int CurrentVersion = 1;

        public int version = CurrentVersion;
        public int width;
        public int height;
        public string grid;
        public int dirt;
        public int[] levels;
        public int antCount;
    }
}
