using System;
using System.IO;
using UnityEngine;

namespace Game.Services
{
    /// <summary>File-backed persistence for the single save slot. Writes go to a temp file first, then replace.</summary>
    public static class SaveStore
    {
        private const string FileName = "colony.json";
        private const string TempSuffix = ".tmp";

        /// <summary>Tests point this at a temp file; null means the real save location.</summary>
        internal static string PathOverride;

        private static string SavePath => PathOverride ?? Path.Combine(Application.persistentDataPath, FileName);

        /// <returns>The saved colony, or null when there is no save or it couldn't be read.</returns>
        public static SaveData Load()
        {
            string path = SavePath;
            if (!File.Exists(path)) return null;

            string json;
            try
            {
                json = File.ReadAllText(path);
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            {
                Debug.LogWarning($"Could not read the save at {path}: {e.Message}. Starting a fresh colony.");
                return null;
            }

            SaveData data = SaveCodec.Decode(json, out bool wasCorrupt);
            if (wasCorrupt) Debug.LogWarning("Save was corrupt; starting a fresh colony");
            return data;
        }

        public static void Save(SaveData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            string path = SavePath;
            string tempPath = path + TempSuffix;
            try
            {
                File.WriteAllText(tempPath, SaveCodec.Encode(data));
                if (File.Exists(path)) File.Delete(path);
                File.Move(tempPath, path);
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            {
                Debug.LogError($"Could not write the save to {path}: {e.Message}");
            }
        }
    }
}
