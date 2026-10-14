using System;
using System.Collections.Generic;
using UnityEngine;

namespace StageGame
{
    [Serializable] public sealed class StageDefinition
    {
        public int id, chapter, aiDepth, aiNodeBudget, aiRandomPermille, starter;
        public string titleKey, openingMoves;
    }
    [Serializable] public sealed class StageFile { public int schemaVersion; public StageDefinition[] stages; }
    [Serializable] public sealed class StringEntry { public string key, ko; }
    [Serializable] public sealed class StringFile { public int schemaVersion; public StringEntry[] strings; }
    [Serializable] public sealed class AdSettings
    {
        public int schemaVersion, interstitialGraceMatches, interstitialEveryMatches, interstitialCooldownSeconds, rewardedHintsPerMatch;
        public bool bannerLobby, bannerResult;
    }

    public sealed class GameData
    {
        public readonly StageDefinition[] Stages;
        public readonly AdSettings Ads;
        readonly Dictionary<string, string> strings = new Dictionary<string, string>();
        public string Text(string key) => strings.TryGetValue(key, out string value) ? value : key;

        public GameData(string stagesJson, string adsJson, string stringsJson)
        {
            var stageFile = JsonUtility.FromJson<StageFile>(stagesJson);
            var stringFile = JsonUtility.FromJson<StringFile>(stringsJson);
            Ads = JsonUtility.FromJson<AdSettings>(adsJson);
            if (stageFile == null || stageFile.schemaVersion != 1 || stageFile.stages == null || stageFile.stages.Length == 0 || stageFile.stages.Length > 30)
                throw new FormatException("스테이지 데이터 버전/목록이 올바르지 않습니다.");
            if (stringFile == null || stringFile.schemaVersion != 1 || stringFile.strings == null)
                throw new FormatException("문자열 데이터가 올바르지 않습니다.");
            foreach (var entry in stringFile.strings)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.key) || string.IsNullOrWhiteSpace(entry.ko) || strings.ContainsKey(entry.key))
                    throw new FormatException("문자열 키/내용이 누락되거나 중복됩니다.");
                strings.Add(entry.key, entry.ko);
            }
            if (Ads == null || Ads.schemaVersion != 1 || Ads.interstitialGraceMatches < 3 || Ads.interstitialEveryMatches < 2 || Ads.interstitialCooldownSeconds < 90 || Ads.rewardedHintsPerMatch != 1)
                throw new FormatException("광고 데이터가 계약 범위를 벗어났습니다.");
            Stages = stageFile.stages;
            for (int i = 0; i < Stages.Length; i++)
            {
                var stage = Stages[i];
                if (stage == null || stage.id != i + 1 || stage.chapter != i / 10 + 1 || !strings.ContainsKey(stage.titleKey ?? "") ||
                    stage.aiDepth < 1 || stage.aiDepth > 6 || stage.aiNodeBudget < 100 || stage.aiNodeBudget > 30000 ||
                    stage.aiRandomPermille < 0 || stage.aiRandomPermille > 1000 || (stage.starter != 1 && stage.starter != -1))
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

        public static GameData Load()
        {
            string Read(string name)
            {
                var asset = Resources.Load<TextAsset>("GameData/" + name);
                if (asset == null) throw new FormatException("게임 데이터 누락: " + name);
                return asset.text;
            }
            return new GameData(Read("stages"), Read("ads"), Read("strings"));
        }
    }
}
