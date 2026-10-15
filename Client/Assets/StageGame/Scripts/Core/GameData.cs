using System;
using UnityEngine;

namespace StageGame
{
    [Serializable] public sealed class StageDefinition
    {
        public int id, chapter, aiDepth, aiNodeBudget, aiRandomPermille, starter;
        public int aiRating, aiOversightPermille;
        public string titleKey, openingMoves;
        public string rankKey, descriptionKey, aiIntroKey, aiWinKey, aiLoseKey;
        public string characterId;
        public float difficultyStars;
    }
    [Serializable] public sealed class StageFile { public int schemaVersion; public StageDefinition[] stages; }
    [Serializable] public sealed class AdSettings
    {
        public int schemaVersion, interstitialGraceMatches, interstitialEveryMatches, interstitialCooldownSeconds, rewardedHintsPerMatch;
        public bool bannerLobby, bannerResult;
    }

    public sealed class GameData
    {
        public readonly StageDefinition[] Stages;
        public readonly AdSettings Ads;
        public readonly StringTable Strings;
        public string Text(string key) => Strings.Text(key);

        public GameData(string stagesJson, string adsJson, string stringsJson)
            : this(stagesJson, adsJson, new StringTable(stringsJson)) { }

        public GameData(string stagesJson, string adsJson, StringTable strings)
        {
            Strings = strings;
            var stageFile = JsonUtility.FromJson<StageFile>(stagesJson);
            Ads = JsonUtility.FromJson<AdSettings>(adsJson);
            if (stageFile == null || stageFile.schemaVersion != 1 || stageFile.stages == null || stageFile.stages.Length != 30)
                throw new FormatException("스테이지 데이터 버전/목록이 올바르지 않습니다.");
            if (Ads == null || Ads.schemaVersion != 1 || Ads.interstitialGraceMatches < 3 || Ads.interstitialEveryMatches < 2 || Ads.interstitialCooldownSeconds < 90 || Ads.rewardedHintsPerMatch != 1)
                throw new FormatException("광고 데이터가 계약 범위를 벗어났습니다.");
            Stages = stageFile.stages;
            for (int i = 0; i < Stages.Length; i++)
            {
                var stage = Stages[i];
                if (stage == null || stage.id != i + 1 || stage.chapter != i / 10 + 1 || !Strings.Contains(stage.titleKey) ||
                    stage.aiDepth < 1 || stage.aiDepth > 6 || stage.aiNodeBudget < 100 || stage.aiNodeBudget > 30000 ||
                    stage.aiRandomPermille < 0 || stage.aiRandomPermille > 1000 ||
                    stage.aiRating < 200 || stage.aiRating > 1600 || stage.aiRating % 100 != 0 ||
                    stage.aiOversightPermille < 0 || stage.aiOversightPermille > 1000 || (stage.starter != 1 && stage.starter != -1))
                    throw new FormatException($"스테이지 {i + 1} 필드가 올바르지 않습니다.");
                CreateBoard(stage);
            }
        }

        public static ConnectFourBoard CreateBoard(StageDefinition stage)
        {
            var board = new ConnectFourBoard(stage.starter);
            if (stage.openingMoves == null) throw new FormatException($"스테이지 {stage.id}: 시작 수열 누락");
            if (stage.openingMoves.Length == 0) return board;
            foreach (string token in stage.openingMoves.Split(','))
            {
                if (!int.TryParse(token, out int column) || !board.TryDrop(column) || board.IsFinished)
                    throw new FormatException($"스테이지 {stage.id}: 불법 또는 이미 종료된 시작 수열");
            }
            return board;
        }

        public void ValidateRankData()
        {
            float previousStars=0;
            foreach (var stage in Stages)
            {
                float stars=stage.difficultyStars;
                if (string.IsNullOrEmpty(stage.characterId) || stage.characterId.Length != 3 || stage.characterId[0] != 'C' ||
                    !int.TryParse(stage.characterId.Substring(1), out int character) || character < 1 || character > 19 ||
                    float.IsNaN(stars) || float.IsInfinity(stars) || stars<1 || stars>8 || stars*2!=Mathf.Round(stars*2) || stars<previousStars)
                    throw new FormatException($"Stage {stage.id}: invalid rank/star progression.");
                previousStars=stars;
                foreach (string key in new[] { stage.rankKey, stage.descriptionKey, stage.aiIntroKey, stage.aiWinKey, stage.aiLoseKey })
                    if (string.IsNullOrWhiteSpace(key) || !Strings.Contains(key)) throw new FormatException($"Stage {stage.id}: missing rank UI string reference.");
            }
        }

        public static GameData Load(StringTable strings = null)
        {
            string Read(string name)
            {
                var asset = Resources.Load<TextAsset>("GameData/" + name);
                if (asset == null) throw new FormatException("게임 데이터 누락: " + name);
                return asset.text;
            }
            return new GameData(Read("stages"), Read("ads"), strings ?? StringTable.Load());
        }
    }
}
