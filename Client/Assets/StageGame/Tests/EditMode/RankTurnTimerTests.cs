using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace StageGame.Tests
{
    public sealed class RatingSearchTests
    {
        [Test] public void AllAuthoredRatingsImproveImmediateAttackAndDefenseWhileBeginnersStillMiss()
        {
            var data=GameData.Load();
            foreach(var moves in new[]{new[]{0,6,1,6,2},new[]{2,3,2,4,2},new[]{0,6,1,6,2,5},new[]{2,3,2,4,2,4}})
            {
                var board=new ConnectFourBoard();
                foreach(int move in moves)Assert.IsTrue(board.TryDrop(move));
                int block=moves[0]==2?2:3;
                var counts=new int[15];
                for(int profile=0;profile<counts.Length;profile++)
                {
                    var stage=data.Stages[profile*2];
                    for(int seed=0;seed<256;seed++)
                        using(var search=new AiSearch(board,stage.aiDepth,stage.aiNodeBudget,stage.aiRandomPermille,seed,oversightPermille:stage.aiOversightPermille))
                        {
                            while(!search.Complete)search.Step(16);
                            Assert.IsTrue(board.CanDrop(search.ChosenColumn));
                            if(search.ChosenColumn==block)counts[profile]++;
                        }
                }
                Assert.That(counts[0],Is.InRange(150,190),"Beginners must keep occasional tactical mistakes.");
                for(int profile=1;profile<counts.Length;profile++)
                    Assert.GreaterOrEqual(counts[profile],counts[profile-1],"Immediate tactics regressed at rating "+data.Stages[profile*2].aiRating);
                for(int profile=2;profile<counts.Length;profile++)
                    Assert.AreEqual(256,counts[profile],"Rating 400 and above must take immediate wins and block a single immediate threat.");
            }
        }
        [TestCase(3,3,4)] [TestCase(3,3,2)] [TestCase(3,0,4)] [TestCase(3,6,2)]
        public void RatingsFrom400AvoidTheObservedSevenMoveForkEvenWhenChoosingRandomly(int first,int reply,int second)
        {
            var board=new ConnectFourBoard();foreach(int move in new[]{first,reply,second})Assert.IsTrue(board.TryDrop(move));
            var data=GameData.Load();
            for(int index=4;index<30;index+=2)
                for(int seed=0;seed<128;seed++)
                {
                    var stage=data.Stages[index];
                    using(var search=new AiSearch(board,stage.aiDepth,stage.aiNodeBudget,1000,seed,oversightPermille:stage.aiOversightPermille))
                    {
                        while(!search.Complete)
                        {
                            int before=search.NodesVisited;search.Step(8);
                            Assert.LessOrEqual(search.NodesVisited-before,8);
                        }
                        var after=board.Clone();Assert.IsTrue(after.TryDrop(search.ChosenColumn));
                        Assert.IsFalse(OpponentCanForceWinInTwo(after),"Rating "+stage.aiRating+", seed "+seed+", column "+search.ChosenColumn);
                        Assert.LessOrEqual(search.NodesVisited,stage.aiNodeBudget);
                        Assert.AreEqual(3,board.MoveCount);
                    }
                }
        }
        // Expected moves come from the independent bounded win/loss oracle in RatingCurveAudit,
        // not from AiSearch's heuristic. The first two are forced wins within five plies.
        [TestCase("3,4,1,0,0,5,4,6,2,1,1,3,4,6",new[]{4})]
        [TestCase("4,5,5,3,1,0,0,0,2,5,0,2,3,6",new[]{4})]
        [TestCase("2,0,2,4,1,0,4,0,0,2,2",new[]{0,1,2,5,6})]
        [TestCase("6,4,6,3,1,3,3,6,1,5,6,6,3,3,4,4,2",new[]{2,4})]
        public void HighestRatingFindsDeeperAttacksAndAvoidsSixPlyLosses(string moves,int[] bestColumns)
        {
            var board=new ConnectFourBoard();foreach(string move in moves.Split(','))Assert.IsTrue(board.TryDrop(int.Parse(move)));
            int count=board.MoveCount;var stage=GameData.Load().Stages[29];
            using(var search=new AiSearch(board,stage.aiDepth,stage.aiNodeBudget,stage.aiRandomPermille,997,oversightPermille:stage.aiOversightPermille))
            {
                while(!search.Complete)
                {
                    int before=search.NodesVisited;search.Step(16);Assert.LessOrEqual(search.NodesVisited-before,16);
                }
                CollectionAssert.Contains(bestColumns,search.ChosenColumn);
                Assert.LessOrEqual(search.NodesVisited,stage.aiNodeBudget);Assert.AreEqual(count,board.MoveCount);
            }
        }
        static bool OpponentCanForceWinInTwo(ConnectFourBoard board)
        {
            int opponent=board.CurrentPlayer;
            for(int move=0;move<7;move++)
            {
                var attack=board.Clone();if(!attack.TryDrop(move))continue;
                if(attack.Winner==opponent)return true;
                if(attack.IsFinished)continue;
                bool hasDefense=false;
                for(int reply=0;reply<7;reply++)
                {
                    var defense=attack.Clone();if(!defense.TryDrop(reply))continue;
                    if(defense.IsFinished){hasDefense=true;break;}
                    bool immediateWin=false;
                    for(int finish=0;finish<7;finish++)
                    {
                        var last=defense.Clone();if(last.TryDrop(finish)&&last.Winner==opponent){immediateWin=true;break;}
                    }
                    if(!immediateWin){hasDefense=true;break;}
                }
                if(!hasDefense)return true;
            }
            return false;
        }
        [Test] public void OversightRemainsLegalDeterministicAndRespectsTerminalBoards()
        {
            var board=new ConnectFourBoard();for(int n=0;n<6;n++)board.TryDrop(3);
            for(int seed=0;seed<32;seed++)
                using(var a=new AiSearch(board,1,100,1000,seed,oversightPermille:1000))
                using(var b=new AiSearch(board,1,100,1000,seed,oversightPermille:1000))
                {
                    a.Step();b.Step();Assert.AreEqual(a.ChosenColumn,b.ChosenColumn);
                    Assert.IsTrue(board.CanDrop(a.ChosenColumn));Assert.AreEqual(0,a.NodesVisited);
                }
            board=new ConnectFourBoard();foreach(int move in new[]{0,6,1,6,2,5,3})board.TryDrop(move);
            using(var done=new AiSearch(board,1,100,0,0,oversightPermille:1000)){done.Step();Assert.AreEqual(-1,done.ChosenColumn);}
            Assert.Throws<ArgumentOutOfRangeException>(()=>new AiSearch(board,1,100,0,0,oversightPermille:1001));
        }
    }
    public sealed partial class RankFlowTests
    {
        void AdvanceTimer(float seconds)=>Call("AdvanceTurnTimer",seconds);
        float Remaining=>Field<float>("turnSecondsRemaining");
        [UnityTest] public IEnumerator TurnTimerExpiresOnceInSelectedColumnAndResetsAfterAiAndUndo()
        {
            Call("OnApplicationFocus",true);Progress.Data.unlockedStage=30;app.StartStage(1);
            Assert.AreEqual(30,Remaining);app.SelectColumn(1);AdvanceTimer(29.5f);
            Assert.AreEqual(0,Board.MoveCount);Assert.AreEqual(.5f,Remaining,.001f);
            app.SelectColumn(4);AdvanceTimer(.5f);AdvanceTimer(100);
            Assert.AreEqual(1,Board.MoveCount);Assert.AreEqual(1,Board[4,0]);Assert.IsTrue(Field<bool>("busy"));
            Assert.IsTrue(app.musicSource.isPlaying);
            yield return AiReady();Assert.That(Remaining,Is.InRange(29f,30f));Assert.AreEqual(2,Board.MoveCount);
            AdvanceTimer(7);float remaining=Remaining;app.SelectColumn(2);Call("RefreshLanguage");Assert.AreEqual(remaining,Remaining);
            app.UndoTurn();Assert.AreEqual(0,Board.MoveCount);Assert.AreEqual(30,Remaining);
        }
        [UnityTest] public IEnumerator TurnTimerFallbackSkipsFullColumnsAndTimeoutWinUsesNormalResult()
        {
            Call("OnApplicationFocus",true);
            Inject(Replay(1,3,3,3,3,3,3));Set("selectedColumn",3);AdvanceTimer(30);
            Assert.AreEqual(7,Board.MoveCount);Assert.AreEqual(1,Board[2,0]);
            app.StartStage(0);Inject(Replay(1,0,6,1,6,2,5));app.SelectColumn(3);AdvanceTimer(30);
            Assert.AreEqual(1,Board.Winner);Assert.IsFalse(app.turnTimerLabel.transform.parent.gameObject.activeSelf);
            yield return ResultReady();int completed=Progress.Data.completedMatches;AdvanceTimer(100);
            Assert.AreEqual(completed,Progress.Data.completedMatches);Assert.AreEqual(7,Board.MoveCount);
        }
        [UnityTest] public IEnumerator TurnTimerPausesAcrossModalsBackgroundAndHintsWithoutRefreshingTheTurn()
        {
            Call("OnApplicationFocus",true);AdvanceTimer(10);Assert.AreEqual(20,Remaining);
            app.pause.onClick.Invoke();AdvanceTimer(90);Assert.AreEqual(20,Remaining);
            app.confirmLeave.onClick.Invoke();AdvanceTimer(90);Assert.AreEqual(20,Remaining);
            app.actionCancel.onClick.Invoke();app.confirmResume.onClick.Invoke();
            app.matchRetry.onClick.Invoke();AdvanceTimer(90);Assert.AreEqual(20,Remaining);app.actionCancel.onClick.Invoke();
            foreach(var overlay in new[]{app.settings,app.help,app.notices}){overlay.SetActive(true);AdvanceTimer(90);Assert.AreEqual(20,Remaining);overlay.SetActive(false);}
            Call("OnApplicationFocus",false);AdvanceTimer(90);Assert.AreEqual(20,Remaining);
            Call("OnApplicationFocus",true);Call("OnApplicationPause",true);AdvanceTimer(90);Assert.AreEqual(20,Remaining);Call("OnApplicationPause",false);
            var gate=Field<RewardGate>("rewardGate");Assert.IsTrue(gate.TryBegin(out int token));AdvanceTimer(90);Assert.AreEqual(20,Remaining);gate.Cancel();
            app.SetAdsForTests(new EditorTestAds{Result=AdResult.Completed});app.hint.onClick.Invoke();yield return AiReady();
            Assert.That(Remaining,Is.InRange(19.8f,20f));Assert.AreEqual(0,Board.MoveCount);
            app.StartStage(0);Assert.AreEqual(30,Remaining);app.ShowLobby();AdvanceTimer(100);Assert.AreEqual(0,Board.MoveCount);
        }
        [UnityTest] public IEnumerator TurnTimerUsesRealElapsedTimeAndFitsBothLanguages()
        {
            Call("OnApplicationFocus",true);app.StartStage(0);float oldScale=Time.timeScale;Time.timeScale=0;
            try{yield return new WaitForSecondsRealtime(.3f);Assert.That(Remaining,Is.InRange(29.3f,29.95f));}
            finally{Time.timeScale=oldScale;}
            Call("OnApplicationFocus",false);float frozen=Remaining;yield return new WaitForSecondsRealtime(.2f);Assert.AreEqual(frozen,Remaining);
            Call("OnApplicationFocus",true);Call("TickTurnTimer");Assert.AreEqual(frozen,Remaining,"Resume must not charge time spent out of focus.");
            foreach(string language in new[]{"ko","en"})
            {
                app.SetLanguage(language);app.StartStage(0);Call("CancelToast");
                foreach(var size in new[]{new Vector2Int(1080,1920),new Vector2Int(390,844),new Vector2Int(320,568)})
                    yield return Capture(language+"-"+size.x+"-timer",app,size.x,size.y,size.x!=1080,directory:"Evidence/turn-timer-rating-20261015",validate:()=>
                    {
                        Assert.IsTrue(app.turnTimerLabel.gameObject.activeInHierarchy);
                        Assert.LessOrEqual(app.turnTimerLabel.preferredWidth,app.turnTimerLabel.rectTransform.rect.width);
                        if(size.x==1080)AssertMatchFits(app);
                    });
                Set("turnSecondsRemaining",4f);Call("RefreshTurnTimer");
                yield return Capture(language+"-1080-timer-warning",app,1080,1920,false,directory:"Evidence/turn-timer-rating-20261015");
                app.ShowLobby();app.lobbyHelp.onClick.Invoke();
                StringAssert.Contains("30",app.help.transform.Find("Frame/DropCaption").GetComponent<UnityEngine.UI.Text>().text);
                yield return Capture(language+"-390-timer-help",app,390,844,true,directory:"Evidence/turn-timer-rating-20261015");
                app.helpClose.onClick.Invoke();
            }
        }
    }
}
