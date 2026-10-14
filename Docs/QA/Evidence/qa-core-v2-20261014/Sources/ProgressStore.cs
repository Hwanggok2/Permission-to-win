using System;
using UnityEngine;

namespace StageGame
{
    [Serializable] public sealed class ProgressData
    {
        public int saveVersion = 1;
        public int unlockedStage = 1;
        public int completedMatches;
        public int lastInterstitialMatch;
        public long lastInterstitialUtc;
        public bool soundEnabled = true;
    }

    public sealed class ProgressStore
    {
        const string Key = "stagegame.progress.v1";
        public ProgressData Data { get; private set; }
        public string LoadWarning { get; private set; }
        string processedMatch;
        public ProgressStore(int stageCount)
        {
            try
            {
                Data = PlayerPrefs.HasKey(Key) ? JsonUtility.FromJson<ProgressData>(PlayerPrefs.GetString(Key)) : new ProgressData();
                if (Data == null || Data.saveVersion != 1 || Data.unlockedStage < 1 || Data.completedMatches < 0 || Data.lastInterstitialMatch < 0 || Data.lastInterstitialMatch > Data.completedMatches || Data.lastInterstitialUtc < 0)
                    throw new FormatException("지원하지 않거나 손상된 저장 형식");
                Data.unlockedStage = Math.Min(stageCount, Data.unlockedStage);
            }
            catch (Exception error)
            {
                // Keep the original for recovery; do not overwrite it until the player makes new progress.
                if (PlayerPrefs.HasKey(Key)) PlayerPrefs.SetString(Key + ".recovery", PlayerPrefs.GetString(Key));
                Data = new ProgressData();
                LoadWarning = "저장을 복구하지 못해 처음부터 시작합니다. 원본은 보관했습니다. " + error.Message;
            }
        }

        public bool RecordResult(string matchId, int stageId, bool won, int stageCount)
        {
            if (string.IsNullOrEmpty(matchId) || matchId == processedMatch) return false;
            processedMatch = matchId;
            Data.completedMatches++;
            if (won) Data.unlockedStage = Math.Max(Data.unlockedStage, Math.Min(stageCount, stageId + 1));
            Save();
            return true;
        }

        public void Save() { PlayerPrefs.SetString(Key, JsonUtility.ToJson(Data)); PlayerPrefs.Save(); }
    }
}
