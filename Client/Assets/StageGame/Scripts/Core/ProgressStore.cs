using System;
using System.Collections.Generic;
using UnityEngine;

namespace StageGame
{
    [Serializable] public sealed class ProgressData
    {
        public int saveVersion = 1;
        public int unlockedStage = 1;
        public int highestClearedStage;
        public int completedMatches;
        public int lastInterstitialMatch;
        public long lastInterstitialUtc;
        public bool soundEnabled = true;
        public bool musicMuted; // Additive v1 setting: older saves default to music on.
        public string language = "ko";
        public List<string> processedMatchIds = new List<string>();
    }

    public sealed class ProgressStore
    {
        const string Key = "stagegame.progress.v1";
        public ProgressData Data { get; private set; }
        public string LoadWarning { get; private set; }
        public string SaveWarning { get; private set; }
        readonly HashSet<string> processedMatches;
        public ProgressStore(int stageCount)
        {
            try
            {
                Data = PlayerPrefs.HasKey(Key) ? JsonUtility.FromJson<ProgressData>(PlayerPrefs.GetString(Key)) : new ProgressData();
                if (Data == null || Data.saveVersion != 1 || Data.unlockedStage < 1 || Data.highestClearedStage < 0 || Data.highestClearedStage > stageCount || Data.completedMatches < 0 || Data.lastInterstitialMatch < 0 || Data.lastInterstitialMatch > Data.completedMatches || Data.lastInterstitialUtc < 0)
                    throw new FormatException("지원하지 않거나 손상된 저장 형식");
                Data.unlockedStage = Math.Min(stageCount, Data.unlockedStage);
                // Missing additive field in v1 saves is zero. Unlock30 proves only clears1..29.
                Data.highestClearedStage = Math.Max(Data.highestClearedStage, Data.unlockedStage - 1);
            }
            catch (Exception error)
            {
                // Keep the original for recovery; do not overwrite it until the player makes new progress.
                try { if (PlayerPrefs.HasKey(Key)) PlayerPrefs.SetString(Key + ".recovery", PlayerPrefs.GetString(Key)); }
                catch (Exception) { /* A storage failure must not prevent offline play. */ }
                Data = new ProgressData();
                LoadWarning = "error_load";
                Debug.LogWarning("[Save recovery] " + error.Message);
            }
            Data.language = StringTable.NormalizeLanguage(Data.language);
            if (Data.processedMatchIds == null) Data.processedMatchIds = new List<string>();
            processedMatches = new HashSet<string>(Data.processedMatchIds);
        }

        public bool RecordResult(string matchId, int stageId, bool won, int stageCount)
        {
            if (string.IsNullOrEmpty(matchId) || stageId < 1 || stageId > stageCount || !processedMatches.Add(matchId)) return false;
            Data.processedMatchIds.Add(matchId);
            Data.completedMatches++;
            if (won)
            {
                Data.highestClearedStage = Math.Max(Data.highestClearedStage, stageId);
                Data.unlockedStage = Math.Max(Data.unlockedStage, Math.Min(stageCount, stageId + 1));
            }
            Save();
            return true;
        }

        public bool Save()
        {
            try
            {
                PlayerPrefs.SetString(Key, JsonUtility.ToJson(Data)); PlayerPrefs.Save();
                SaveWarning = null;
                return true;
            }
            catch (Exception)
            {
                SaveWarning = "client_error_save";
                return false;
            }
        }
    }
}
