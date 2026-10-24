using System;
using Game.Core;
using UnityEngine;

namespace Game.Services
{
    /// <summary>JSON ↔ SaveData. Never throws; unreadable or invalid input decodes to null with wasCorrupt set.</summary>
    public static class SaveCodec
    {
        public static string Encode(SaveData data) => JsonUtility.ToJson(data);

        /// <returns>The save, or null — wasCorrupt is false when there simply was no save yet.</returns>
        public static SaveData Decode(string json, out bool wasCorrupt)
        {
            wasCorrupt = false;
            if (string.IsNullOrEmpty(json)) return null;

            SaveData data = null;
            try
            {
                data = JsonUtility.FromJson<SaveData>(json);
            }
            catch (ArgumentException)
            {
                // Malformed JSON: treated as corrupt below.
            }

            if (data != null && IsValid(data)) return data;
            wasCorrupt = true;
            return null;
        }

        public static bool TryDecodeGrid(string base64, out byte[] cells)
        {
            cells = null;
            if (string.IsNullOrEmpty(base64)) return false;
            try
            {
                cells = Convert.FromBase64String(base64);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static bool IsValid(SaveData data) =>
            data.version == SaveData.CurrentVersion
            && data.width > 0 && data.height > 0
            && data.dirt >= 0 && data.antCount >= 0
            && data.levels != null && data.levels.Length == TuningConfig.UpgradeCount
            && TryDecodeGrid(data.grid, out byte[] cells) && cells.Length == data.width * data.height;
    }
}
