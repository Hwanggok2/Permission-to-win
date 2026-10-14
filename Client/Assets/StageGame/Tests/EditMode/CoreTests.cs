using System;
using NUnit.Framework;
using UnityEngine;

namespace StageGame.Tests
{
    public sealed class CoreTests
    {
        static ConnectFourBoard Replay(params int[] moves)
        {
            var board = new ConnectFourBoard();
            foreach (int move in moves) Assert.IsTrue(board.TryDrop(move), "Unexpected illegal move: " + move);
            return board;
        }

        [Test] public void GravityAlternatesAndRejectsFullColumn()
        {
            var board = Replay(2, 2, 2, 2, 2, 2);
            Assert.AreEqual(1, board[2, 0]); Assert.AreEqual(-1, board[2, 1]);
            Assert.IsFalse(board.TryDrop(2)); Assert.IsFalse(board.TryDrop(-1)); Assert.IsFalse(board.TryDrop(7));
            Assert.AreEqual(6, board.MoveCount); Assert.AreEqual(1, board.CurrentPlayer);
        }
        [TestCase(new[] { 0, 6, 1, 6, 2, 5, 3 })]
        [TestCase(new[] { 0, 1, 0, 1, 0, 1, 0 })]
        [TestCase(new[] { 0, 1, 1, 2, 4, 2, 2, 3, 4, 3, 5, 3, 3 })]
        [TestCase(new[] { 6, 5, 5, 4, 2, 4, 4, 3, 2, 3, 1, 3, 3 })]
        public void FourDirectionsWinAndFinishedBoardRejectsInput(int[] moves)
        {
            var board = Replay(moves);
            Assert.AreEqual(1, board.Winner); Assert.AreEqual(4, board.WinningCells.Count);
            int count = board.MoveCount; Assert.IsFalse(board.TryDrop(6)); Assert.AreEqual(count, board.MoveCount);
        }
        [Test] public void CloneDoesNotMutateOriginal()
        {
            var board = Replay(3, 4); var clone = board.Clone(); clone.TryDrop(2);
            Assert.AreEqual(2, board.MoveCount); Assert.AreEqual(0, board[2, 0]);
        }
        [Test] public void AiTakesWinBeforeRandomMistake()
        {
            var board = Replay(0, 6, 1, 6, 2, 5);
            using (var search = new AiSearch(board, 1, 100, 1000, 7))
            {
                while (!search.Complete) search.Step(); Assert.AreEqual(3, search.ChosenColumn);
            }
        }
        [Test] public void AiBlocksImmediateLossEvenAtMaximumMistakes()
        {
            var board = Replay(6, 0, 6, 1, 5, 2);
            for (int seed = 0; seed < 20; seed++)
                using (var search = new AiSearch(board, 1, 100, 1000, seed))
                {
                    while (!search.Complete) search.Step(); Assert.AreEqual(3, search.ChosenColumn);
                }
        }
        [Test] public void AiRespectsNodeAndStepBudgetsAndLeavesBoardUntouched()
        {
            var board = new ConnectFourBoard();
            using (var search = new AiSearch(board, 6, 137, 0, 9))
            {
                int steps = 0;
                while (!search.Complete)
                {
                    int before = search.NodesVisited; search.Step(8);
                    Assert.LessOrEqual(search.NodesVisited - before, 8);
                    Assert.Less(++steps, 1000);
                }
                Assert.LessOrEqual(search.NodesVisited, 137); Assert.IsTrue(board.CanDrop(search.ChosenColumn));
                Assert.AreEqual(0, board.MoveCount);
            }
        }
        [Test] public void BundledDataReplaysEveryStageAndAiReturnsLegalMove()
        {
            var data = GameData.Load(); Assert.AreEqual(30, data.Stages.Length);
            foreach (var stage in data.Stages)
            {
                var board = GameData.CreateBoard(stage); Assert.IsFalse(board.IsFinished);
                using (var search = new AiSearch(board, stage.aiDepth, stage.aiNodeBudget, stage.aiRandomPermille, stage.id))
                {
                    while (!search.Complete) search.Step(256);
                    Assert.IsTrue(board.CanDrop(search.ChosenColumn), "Stage " + stage.id);
                    Assert.LessOrEqual(search.NodesVisited, stage.aiNodeBudget);
                }
            }
        }
        [TestCase("0,0,0,0,0,0,0")]
        [TestCase("0,6,1,6,2,5,3")]
        [TestCase("7")]
        [TestCase("a")]
        [TestCase("0,")]
        public void InvalidOrTerminalOpeningsFailLoudly(string opening)
        {
            Assert.Throws<FormatException>(() => GameData.CreateBoard(new StageDefinition { id = 1, starter = 1, openingMoves = opening }));
        }
        [Test] public void MalformedDataCannotSilentlyLoad()
        {
            string stages = Resources.Load<TextAsset>("GameData/stages").text;
            string ads = Resources.Load<TextAsset>("GameData/ads").text;
            string strings = Resources.Load<TextAsset>("GameData/strings").text;
            Assert.Throws<FormatException>(() => new GameData(stages.Replace("\"schemaVersion\": 1", "\"schemaVersion\": 99").Replace("\"schemaVersion\":1", "\"schemaVersion\":99"), ads, strings));
        }
        [TestCase(AdResult.Unavailable)] [TestCase(AdResult.Failed)] [TestCase(AdResult.Cancelled)]
        public void UnsuccessfulRewardCanRetryWithoutGrant(AdResult result)
        {
            var gate = new RewardGate(); Assert.IsTrue(gate.TryBegin(out int token));
            Assert.IsFalse(gate.Complete(token, result)); Assert.IsFalse(gate.Granted); Assert.IsFalse(gate.Pending);
            Assert.IsTrue(gate.TryBegin(out _));
        }
        [Test] public void RewardCompletesOnlyOnceAndRejectsStaleCallbacks()
        {
            var gate = new RewardGate(); Assert.IsTrue(gate.TryBegin(out int cancelled));
            Assert.IsFalse(gate.TryBegin(out _)); gate.Cancel();
            Assert.IsTrue(gate.TryBegin(out int active)); Assert.IsFalse(gate.Complete(cancelled, AdResult.Completed));
            Assert.IsTrue(gate.Pending); Assert.IsTrue(gate.Complete(active, AdResult.Completed));
            Assert.IsFalse(gate.Complete(active, AdResult.Completed)); Assert.IsFalse(gate.TryBegin(out _));
        }
        [Test] public void NoFillNeverRewardsAndEditorDuplicateOnlyGrantsOnce()
        {
            var gate = new RewardGate(); gate.TryBegin(out int token);
            new NoFillAds().ShowRewarded(result => Assert.IsFalse(gate.Complete(token, result)));
            gate.TryBegin(out token); int grants = 0;
            new EditorTestAds { Result = AdResult.Completed, DuplicateCallback = true }.ShowRewarded(result => { if (gate.Complete(token, result)) grants++; });
            Assert.AreEqual(1, grants);
        }
        [Test] public void AdPolicyRequiresGraceMatchesAndBothCooldowns()
        {
            var settings = new AdSettings { interstitialGraceMatches = 3, interstitialEveryMatches = 2, interstitialCooldownSeconds = 90 };
            var progress = new ProgressData { completedMatches = 3 };
            Assert.IsFalse(AdPolicy.Eligible(settings, progress, 1000));
            progress.completedMatches = 4; Assert.IsTrue(AdPolicy.Eligible(settings, progress, 1000));
            progress.lastInterstitialMatch = 4; progress.lastInterstitialUtc = 1000;
            progress.completedMatches = 5; Assert.IsFalse(AdPolicy.Eligible(settings, progress, 1200));
            progress.completedMatches = 6; Assert.IsFalse(AdPolicy.Eligible(settings, progress, 1089));
            Assert.IsTrue(AdPolicy.Eligible(settings, progress, 1090));
            Assert.IsFalse(AdPolicy.Eligible(settings, progress, 900));
        }
        [Test] public void SavedPrefabReferencesAndNestedButtonsValidate()
        {
            Editor.ProjectBuilder.Validate();
        }

        [Test] public void ProgressRejectsNonAdjacentAndReloadedDuplicateResults()
        {
            const string key = "stagegame.progress.v1";
            bool existed = PlayerPrefs.HasKey(key); string previous = PlayerPrefs.GetString(key, "");
            try
            {
                PlayerPrefs.DeleteKey(key);
                var progress = new ProgressStore(30);
                Assert.IsTrue(progress.RecordResult("A", 1, true, 30));
                Assert.IsTrue(progress.RecordResult("B", 2, false, 30));
                Assert.IsFalse(progress.RecordResult("A", 1, true, 30));
                Assert.IsFalse(new ProgressStore(30).RecordResult("A", 1, true, 30));
                Assert.AreEqual(2, new ProgressStore(30).Data.completedMatches);
            }
            finally { if (existed) PlayerPrefs.SetString(key, previous); else PlayerPrefs.DeleteKey(key); PlayerPrefs.Save(); }
        }

        [Test] public void TruncatedStageCatalogIsRejected()
        {
            string stages = "{\"schemaVersion\":1,\"stages\":[{\"id\":1,\"chapter\":1,\"titleKey\":\"stage_001\",\"aiDepth\":1,\"aiNodeBudget\":500,\"aiRandomPermille\":250,\"starter\":1,\"openingMoves\":\"\"}]}";
            Assert.Throws<FormatException>(() => new GameData(stages, Resources.Load<TextAsset>("GameData/ads").text, Resources.Load<TextAsset>("GameData/strings").text));
        }
    }
}
