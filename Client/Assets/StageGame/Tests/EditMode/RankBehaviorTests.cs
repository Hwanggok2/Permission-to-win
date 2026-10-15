using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace StageGame.Tests
{
    public sealed partial class RankFlowTests
    {
        ConnectFourBoard Board => Field<ConnectFourBoard>("board");
        ProgressStore Progress => Field<ProgressStore>("progress");
        GameData Data => Field<GameData>("data");
        IEnumerator AiReady()
        {
            float end=Time.realtimeSinceStartup+2;
            while(Field<bool>("busy")&&Time.realtimeSinceStartup<end)yield return null;
            Assert.IsFalse(Field<bool>("busy"),"AI must finish or cancel within its active budget.");
        }
        static ConnectFourBoard Replay(int starter, params int[] moves)
        {
            var board=new ConnectFourBoard(starter);
            foreach(int move in moves)Assert.IsTrue(board.TryDrop(move));return board;
        }
        static void SameBoard(ConnectFourBoard expected,ConnectFourBoard actual)
        {
            Assert.AreEqual(expected.MoveCount,actual.MoveCount);Assert.AreEqual(expected.CurrentPlayer,actual.CurrentPlayer);Assert.AreEqual(expected.LastMove,actual.LastMove);
            Assert.AreEqual(expected.Winner,actual.Winner);CollectionAssert.AreEqual(expected.WinningCells,actual.WinningCells);
            for(int c=0;c<7;c++){Assert.AreEqual(expected.LandingRow(c),actual.LandingRow(c));for(int r=0;r<6;r++)Assert.AreEqual(expected[c,r],actual[c,r]);}
        }
        void Inject(ConnectFourBoard board){Call("CancelSearch");Set("board",board);Call("RefreshBoard");}
        GameObject Hit(Transform target)
        {
            Canvas.ForceUpdateCanvases();
            var pointer=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,target.position),button=PointerEventData.InputButton.Left};
            var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);Assert.IsNotEmpty(hits);
            return hits[0].gameObject;
        }
        [UnityTest] public IEnumerator SelectionIsOnlyPreviewAndPlaceCommitsOnce()
        {
            string id=Field<string>("matchId"),save=PlayerPrefs.GetString(Key);var before=Board.Clone();
            app.PlaceSelected();SameBoard(before,Board);Assert.IsFalse(app.place.interactable);
            app.SelectColumn(2);app.SelectColumn(4);SameBoard(before,Board);
            Assert.AreEqual(id,Field<string>("matchId"));Assert.AreEqual(save,PlayerPrefs.GetString(Key));
            Assert.AreEqual(app.stones[4].rectTransform.anchoredPosition,app.movePreview.rectTransform.anchoredPosition);Assert.IsTrue(app.place.interactable);
            app.PlaceSelected();app.PlaceSelected();app.SelectColumn(0);app.PlaceSelected();
            Assert.AreEqual(1,Board.MoveCount);Assert.AreEqual(1,Board[4,0]);Assert.IsFalse(app.movePreview.gameObject.activeSelf);
            yield return AiReady();Assert.AreEqual(2,Board.MoveCount);Assert.AreEqual(0,Progress.Data.completedMatches);
            Inject(Replay(1,2,2,2,2,2,2));app.SelectColumn(3);Assert.IsTrue(app.place.interactable);
            app.SelectColumn(2);Assert.IsFalse(app.place.interactable);Assert.IsFalse(app.movePreview.gameObject.activeSelf);app.PlaceSelected();Assert.AreEqual(6,Board.MoveCount);
            app.SelectColumn(-1);app.SelectColumn(7);Assert.IsFalse(app.place.interactable);
            app.pause.onClick.Invoke();app.SelectColumn(3);app.PlaceSelected();Assert.AreEqual(6,Board.MoveCount);
            app.confirmResume.onClick.Invoke();Call("OnApplicationPause",true);app.SelectColumn(3);app.PlaceSelected();Assert.AreEqual(6,Board.MoveCount);Call("OnApplicationPause",false);
        }
        [UnityTest] public IEnumerator UndoRestoresPairsAndCancelsInFlightAiWithoutChangingMatch()
        {
            var initial=Board.Clone();string id=Field<string>("matchId");
            app.SelectColumn(3);app.PlaceSelected();yield return AiReady();var pair=Board.Clone();
            app.SelectColumn(2);app.PlaceSelected();yield return AiReady();Assert.AreEqual(4,Board.MoveCount);
            app.UndoTurn();SameBoard(pair,Board);app.UndoTurn();SameBoard(initial,Board);app.UndoTurn();SameBoard(initial,Board);
            app.SelectColumn(1);app.PlaceSelected();int generation=Field<int>("searchGeneration");Assert.IsTrue(Field<bool>("busy"));
            app.UndoTurn();Assert.Greater(Field<int>("searchGeneration"),generation);SameBoard(initial,Board);
            yield return new WaitForSecondsRealtime(.9f);SameBoard(initial,Board);Assert.AreEqual(id,Field<string>("matchId"));
            Assert.AreEqual(0,Progress.Data.completedMatches);Assert.AreEqual(0,Progress.Data.lastInterstitialMatch);Assert.IsFalse(app.movePreview.gameObject.activeSelf);
            app.SelectColumn(5);app.PlaceSelected();yield return AiReady();app.UndoTurn();SameBoard(initial,Board);
        }
        [UnityTest] public IEnumerator OpeningAndAiStarterAreUndoFloorAndRetryIsFree()
        {
            Progress.Data.unlockedStage=30;
            foreach(int index in new[]{1,29})
            {
                app.StartStage(index);yield return AiReady();var opening=Board.Clone();Assert.GreaterOrEqual(opening.MoveCount,GameData.CreateBoard(Data.Stages[index]).MoveCount);
                Assert.IsFalse(app.undo.interactable);app.UndoTurn();SameBoard(opening,Board);
                int column=Enumerable.Range(0,7).First(Board.CanDrop);app.SelectColumn(column);app.PlaceSelected();app.UndoTurn();SameBoard(opening,Board);
                string id=Field<string>("matchId");app.matchRetry.onClick.Invoke();app.actionAccept.onClick.Invoke();yield return AiReady();
                var restarted=GameData.CreateBoard(Data.Stages[index]);
                if(restarted.CurrentPlayer==-1)Assert.IsTrue(restarted.TryDrop(Board.LastMove%7),"The new match may choose a different legal opening.");
                SameBoard(restarted,Board);Assert.AreNotEqual(id,Field<string>("matchId"));
            }
            // Exercise an AI-to-move opening even if the frozen stages all start on the player turn.
            var stage=Data.Stages[0];stage.starter=-1;stage.openingMoves="";app.StartStage(0);yield return AiReady();var aiOpening=Board.Clone();
            Assert.AreEqual(1,aiOpening.MoveCount);app.UndoTurn();SameBoard(aiOpening,Board);
            app.SelectColumn(3);app.PlaceSelected();app.UndoTurn();SameBoard(aiOpening,Board);Assert.AreEqual(0,Progress.Data.completedMatches);
        }
        [UnityTest] public IEnumerator FreshRunIsConsumedButExistingAndRecoverySavesStayInLobby()
        {
            Assert.IsTrue(app.match.activeSelf);Assert.AreEqual(0,Field<int>("activeStage"));Assert.IsTrue(PlayerPrefs.HasKey(Key));
            Assert.AreEqual(Data.Text("select_column"),app.matchNotice.text);
            foreach(string fixture in new[]{PlayerPrefs.GetString(Key),"{\"saveVersion\":1,\"unlockedStage\":12,\"completedMatches\":9,\"lastInterstitialMatch\":4,\"lastInterstitialUtc\":123,\"language\":\"en\",\"soundEnabled\":false,\"processedMatchIds\":[\"old\"]}","broken-save"})
            {
                app.ShowLobby();PlayerPrefs.SetString(Key,fixture);PlayerPrefs.Save();UnityEngine.SceneManagement.SceneManager.LoadScene("Main");
                yield return null;yield return null;app=UnityEngine.Object.FindFirstObjectByType<RankGameApp>();
                Assert.IsTrue(app.lobby.activeSelf);Assert.IsFalse(app.match.activeSelf);
                if(fixture=="broken-save"){Assert.AreEqual(fixture,PlayerPrefs.GetString(Key+".recovery"));Assert.AreEqual(fixture,PlayerPrefs.GetString(Key));Assert.AreEqual("error_load",Progress.LoadWarning);}
                else if(fixture.Contains("old")){Assert.AreEqual(12,Progress.Data.unlockedStage);Assert.AreEqual(9,Progress.Data.completedMatches);Assert.AreEqual(4,Progress.Data.lastInterstitialMatch);Assert.AreEqual(123,Progress.Data.lastInterstitialUtc);Assert.IsFalse(Progress.Data.soundEnabled);Assert.AreEqual("en",Progress.Data.language);Assert.IsFalse(Progress.RecordResult("old",1,true,30));}
            }
        }
        [UnityTest] public IEnumerator ThirtyCardsReachBothEndsAndDetailBlocksLobbyAndPreservesScroll()
        {
            app.ShowLobby();yield return null;app.stageScroll.verticalNormalizedPosition=1;
            // Validate the portrait layout, not the editor's default 640x480 Game view.
            yield return Capture("lobby-scroll-reachability",app,390,844,true,false,()=>
            {
                Rect visible=app.stageScroll.viewport.rect;
                Vector2 InViewport(int i)=>app.stageScroll.viewport.InverseTransformPoint(app.cards[i].transform.position);
                Assert.IsTrue(visible.Contains(InViewport(0)));Assert.IsTrue(visible.Contains(InViewport(3)));
                app.stageScroll.verticalNormalizedPosition=0;Canvas.ForceUpdateCanvases();Assert.IsTrue(visible.Contains(InViewport(28)));Assert.IsTrue(visible.Contains(InViewport(29)));
            });
            app.stageScroll.verticalNormalizedPosition=.43f;Canvas.ForceUpdateCanvases();float position=app.stageScroll.verticalNormalizedPosition;
            app.OpenDetail(29);Assert.IsTrue(Field<bool>("detailSliding"));Assert.IsFalse(app.play.interactable);
            foreach(var target in new[]{app.lobbyHelp.transform,app.lobbySettings.transform,app.multiplayer.transform})Assert.IsFalse(Hit(target).transform.IsChildOf(app.lobby.transform));
            app.play.onClick.Invoke();Assert.IsTrue(app.lobby.activeSelf);yield return new WaitForSecondsRealtime(.25f);
            Assert.IsFalse(app.play.interactable);Assert.IsFalse(app.following.interactable);app.play.onClick.Invoke();Assert.IsFalse(app.match.activeSelf);
            for(int i=29;i>=0;i--)
            {
                Assert.AreEqual(i,Field<int>("selected"));var opening=GameData.CreateBoard(Data.Stages[i]);
                for(int cell=0;cell<42;cell++){int owner=opening[cell%7,cell/7];Assert.AreEqual(owner!=0,app.detailStones[cell].enabled);if(owner!=0)Assert.AreSame(owner==1?app.theme.blackStone:app.theme.whiteStone,app.detailStones[cell].sprite);}
                Assert.AreEqual(Data.Stages[i].difficultyStars,app.detailStars.GetComponent<RankStarRating>().Value);
                if(i>0)app.previous.onClick.Invoke();
            }
            Assert.IsFalse(app.previous.interactable);Assert.IsTrue(app.play.interactable);
            Assert.AreEqual(app.detailClose.gameObject,ExecuteEvents.GetEventHandler<IPointerClickHandler>(Hit(app.detailClose.transform)));
            app.CloseDetail();Assert.IsTrue(app.detail.gameObject.activeSelf);Assert.IsFalse(Hit(app.lobbyHelp.transform).transform.IsChildOf(app.lobby.transform));
            yield return new WaitForSecondsRealtime(.25f);Assert.IsFalse(app.detail.gameObject.activeSelf);Assert.AreEqual(position,app.stageScroll.verticalNormalizedPosition,.001f);
            Assert.AreEqual(app.lobbyHelp.gameObject,ExecuteEvents.GetEventHandler<IPointerClickHandler>(Hit(app.lobbyHelp.transform)));
            Assert.IsFalse(app.multiplayer.interactable);
        }
        sealed class PendingAds:IAdService
        {
            public Action<AdResult> Reward,Interstitial;public int Rewards,Interstitials,Cancels;public float CalledAt;
            public string Status=>"test";
            public void ShowRewarded(Action<AdResult> callback){Rewards++;Reward=callback;}
            public void ShowInterstitial(Action<AdResult> callback){Interstitials++;CalledAt=Time.realtimeSinceStartup;Interstitial=callback;}
            public void CancelPending(){Cancels++;}
        }
        [UnityTest] public IEnumerator RewardCompletionIsOnceAndUndoKeepsConsumedHint()
        {
            app.SelectColumn(3);app.PlaceSelected();yield return AiReady();string id=Field<string>("matchId");
            var ads=new PendingAds();app.SetAdsForTests(ads);app.pause.onClick.Invoke();app.hint.onClick.Invoke();Assert.IsFalse(app.confirm.activeSelf);Assert.IsFalse(app.place.interactable);
            ads.Reward(AdResult.Completed);ads.Reward(AdResult.Completed);yield return AiReady();Assert.IsTrue(Field<RewardGate>("rewardGate").Granted);Assert.IsTrue(app.movePreview.gameObject.activeSelf);Assert.AreEqual(2,Board.MoveCount);
            app.UndoTurn();Assert.AreEqual(0,Board.MoveCount);Assert.IsTrue(Field<RewardGate>("rewardGate").Granted);Assert.IsFalse(app.movePreview.gameObject.activeSelf);Assert.IsFalse(app.hint.interactable);Assert.AreEqual(id,Field<string>("matchId"));
            app.hint.onClick.Invoke();Assert.AreEqual(1,ads.Rewards);Assert.AreEqual(0,Progress.Data.completedMatches);
        }
        [UnityTest] public IEnumerator PendingRewardCancelsOnUndoPauseRetryAndTimeout()
        {
            var ads=new PendingAds();app.SetAdsForTests(ads);
            app.SelectColumn(3);app.PlaceSelected();yield return AiReady();app.hint.onClick.Invoke();var old=ads.Reward;
            app.UndoTurn();old(AdResult.Completed);yield return null;Assert.IsFalse(Field<RewardGate>("rewardGate").Granted);Assert.AreEqual(0,Board.MoveCount);
            app.hint.onClick.Invoke();old=ads.Reward;Call("OnApplicationPause",true);old(AdResult.Completed);Call("OnApplicationPause",false);Assert.IsFalse(Field<RewardGate>("rewardGate").Granted);
            app.hint.onClick.Invoke();old=ads.Reward;app.StartStage(0);old(AdResult.Completed);Assert.IsFalse(Field<RewardGate>("rewardGate").Granted);
            app.hint.onClick.Invoke();old=ads.Reward;app.SelectColumn(2);app.PlaceSelected();Assert.AreEqual(0,Board.MoveCount);
            var activeGate=Field<RewardGate>("rewardGate");float timeoutAt=Time.realtimeSinceStartup+7;
            while(activeGate.Pending && Time.realtimeSinceStartup<timeoutAt)yield return null;
            Assert.AreSame(activeGate,Field<RewardGate>("rewardGate"),"Timeout must keep the current match reward gate.");
            old(AdResult.Completed);yield return null;
            Assert.NotNull(app.columns);Assert.NotNull(app.columns[0]);
            Assert.IsFalse(Field<RewardGate>("rewardGate").Granted);Assert.IsFalse(Field<RewardGate>("rewardGate").Pending);Assert.IsTrue(app.columns[0].interactable);Assert.Greater(ads.Cancels,0);
            app.SetAdsForTests(new NoFillAds());app.hint.onClick.Invoke();Assert.IsFalse(Field<RewardGate>("rewardGate").Granted);Assert.IsTrue(app.columns[0].interactable);
        }
        [UnityTest] public IEnumerator BothWinnersAnimatePauseRestoreThenAdsAndCorrectResultActions()
        {
            // This fixture tests victory presentation, independently of beginner tactical oversights.
            Data.Stages[0].aiOversightPermille=0;
            var trace=new List<string>{"side,event,elapsed,scale,adCalls"};
            foreach(int side in new[]{1,-1})
            {
                app.StartStage(0);Progress.Data.completedMatches=4;Progress.Data.lastInterstitialMatch=0;Progress.Data.lastInterstitialUtc=0;
                var ads=new PendingAds();app.SetAdsForTests(ads);Inject(Replay(side,0,6,1,6,2,5));int count=Board.MoveCount;
                var stone=app.stones[3].rectTransform;int sibling=stone.GetSiblingIndex();Vector2 position=stone.anchoredPosition;
                if(side==1){app.SelectColumn(3);app.PlaceSelected();}else Call("BeginThink",false);
                float until=Time.realtimeSinceStartup+2;while(!Field<bool>("resolving")&&Time.realtimeSinceStartup<until)yield return null;
                Assert.AreEqual(side,Board.Winner);float start=Time.realtimeSinceStartup;Assert.Greater(stone.localScale.x,1.4f);Assert.AreEqual(stone.parent.childCount-1,stone.GetSiblingIndex());
                trace.Add($"{side},enlarged,0,{stone.localScale.x},0");
                Call("OnApplicationPause",true);var scale=stone.localScale;yield return new WaitForSecondsRealtime(.15f);Assert.AreEqual(scale,stone.localScale);Call("OnApplicationPause",false);
                app.UndoTurn();app.PlaceSelected();app.matchRetry.onClick.Invoke();Call("Finish");Assert.AreEqual(count+1,Board.MoveCount);Assert.AreEqual(5,Progress.Data.completedMatches);
                while(!Field<bool>("winningRevealed")&&Time.realtimeSinceStartup-start<2)yield return null;
                Assert.AreEqual(Vector3.one,stone.localScale);Assert.Less(Vector2.Distance(position,stone.anchoredPosition),.001f);Assert.AreEqual(sibling,stone.GetSiblingIndex());Assert.AreEqual(0,ads.Interstitials);
                foreach(int cell in Board.WinningCells)Assert.AreSame(side==1?app.theme.blackWin:app.theme.whiteWin,app.stones[cell].sprite);
                trace.Add($"{side},landed,{Time.realtimeSinceStartup-start},{stone.localScale.x},{ads.Interstitials}");
                while(ads.Interstitials==0&&Time.realtimeSinceStartup-start<3)yield return null;
                Assert.AreEqual(1,ads.Interstitials);Assert.GreaterOrEqual(ads.CalledAt-start,1f);Assert.IsFalse(app.retry.interactable);
                ads.Interstitial(AdResult.Completed);ads.Interstitial(AdResult.Completed);yield return ResultReady();Assert.AreEqual(5,Progress.Data.lastInterstitialMatch);
                Assert.AreEqual(side==1,app.nextStage.gameObject.activeSelf);Assert.AreEqual(side==1?3:2,app.result.GetComponentsInChildren<Button>().Length);
                Assert.IsTrue(app.match.activeSelf);Assert.IsFalse(app.undo.gameObject.activeSelf);Assert.IsFalse(app.place.gameObject.activeSelf);Assert.IsFalse(app.matchRetry.gameObject.activeSelf);Assert.IsFalse(app.pause.gameObject.activeSelf);
                yield return SpeechComplete();Assert.AreEqual(Data.Text(side==1?Data.Stages[0].aiLoseKey:Data.Stages[0].aiWinKey),app.aiSpeech.text);
                trace.Add($"{side},result,{Time.realtimeSinceStartup-start},1,{ads.Interstitials}");
                if(side==1){app.nextStage.onClick.Invoke();app.nextStage.onClick.Invoke();Assert.AreEqual(1,Field<int>("activeStage"));}
                else {app.retry.onClick.Invoke();app.actionAccept.onClick.Invoke();}
                ads.Interstitial(AdResult.Completed);Assert.IsFalse(app.result.activeSelf);Assert.AreEqual(5,Progress.Data.completedMatches);
            }
            Directory.CreateDirectory("Evidence/rank-v10-20261015");File.WriteAllLines("Evidence/rank-v10-20261015/animation-timeline.csv",trace);
        }
        [UnityTest] public IEnumerator FinalStageAndAdTimeoutDoNotExposeNextOrAcceptLateCompletion()
        {
            Progress.Data.unlockedStage=30;Progress.Data.completedMatches=4;app.StartStage(29);
            var ads=new PendingAds();app.SetAdsForTests(ads);Inject(Replay(1,0,6,1,6,2,5));app.SelectColumn(3);app.PlaceSelected();yield return ResultReady();
            Assert.AreEqual(1,ads.Interstitials);Assert.IsFalse(app.nextStage.gameObject.activeSelf);Assert.AreEqual(2,app.result.GetComponentsInChildren<Button>().Length);
            int before=Progress.Data.lastInterstitialMatch;ads.Interstitial(AdResult.Completed);yield return null;Assert.AreEqual(before,Progress.Data.lastInterstitialMatch);Assert.AreEqual(5,Progress.Data.completedMatches);
            app.UndoTurn();Assert.IsTrue(Board.IsFinished);app.resultLobby.onClick.Invoke();app.actionAccept.onClick.Invoke();Assert.IsTrue(app.lobby.activeSelf);Assert.AreEqual(29,Field<int>("selected"));
        }
        [UnityTest] public IEnumerator CrossingWinsChooseOneContinuousLineThroughFinalStone()
        {
            foreach(int side in new[]{1,-1})foreach(bool mirror in new[]{false,true})
            {
                app.StartStage(0);var moves=new[]{1,0,6,2,2,1,1,0,4,0,0,1,1,6,5,5,3};if(mirror)moves=moves.Select(x=>6-x).ToArray();
                Inject(Replay(side,moves));Call("ShowWinLine");Assert.IsTrue(app.winLine.gameObject.activeSelf);
                var line=app.winLine.rectTransform;Vector2 axis=new Vector2(Mathf.Cos(line.eulerAngles.z*Mathf.Deg2Rad),Mathf.Sin(line.eulerAngles.z*Mathf.Deg2Rad));
                Vector2 a=line.anchoredPosition-axis*line.sizeDelta.x/2,b=line.anchoredPosition+axis*line.sizeDelta.x/2;
                int Index(Vector2 point)=>Enumerable.Range(0,42).First(i=>Vector2.Distance(app.stones[i].rectTransform.anchoredPosition,point)<.1f);
                int x=Index(a),y=Index(b),dx=y%7-x%7,dy=y/7-x/7,steps=Math.Max(Math.Abs(dx),Math.Abs(dy));
                Assert.IsTrue(dx==0||dy==0||Math.Abs(dx)==Math.Abs(dy));Assert.GreaterOrEqual(steps,3);bool includesLast=false;
                for(int n=0;n<=steps;n++){int c=x%7+Math.Sign(dx)*n,r=x/7+Math.Sign(dy)*n;Assert.AreEqual(side,Board[c,r]);includesLast|=r*7+c==Board.LastMove;}Assert.IsTrue(includesLast);
                yield return Capture("crossing-"+side+"-"+mirror,app,320,568,true);
            }
        }
        [UnityTest] public IEnumerator TopRowAnimationAvoidsStatusAndDrawKeepsTwoActions()
        {
            foreach(int starter in new[]{1,-1})
            {
                app.StartStage(0);var board=Replay(starter,3,2,3,3,1,3,2,3,6,5,5,3);Assert.AreEqual(5,board.LastMove/7);Inject(board);Call("Finish");
                Assert.IsFalse(app.turnLabel.gameObject.activeSelf);Call("OnApplicationPause",true);
                yield return Capture("top-row-animation-"+board.Winner,app,320,568,true);
                Call("OnApplicationPause",false);yield return ResultReady();
            }
            foreach(string language in new[]{"ko","en"})
            {
                app.SetLanguage(language);app.StartStage(0);Inject(Replay(1,0,2,2,2,2,2,2,0,0,0,0,0,1,3,3,3,3,3,3,1,1,1,1,1,4,6,6,6,6,6,6,4,4,4,4,4,5,5,5,5,5,5));
                Assert.IsTrue(Board.IsDraw);Call("Finish");yield return ResultReady();Assert.IsFalse(app.nextStage.gameObject.activeSelf);Assert.AreEqual(2,app.result.GetComponentsInChildren<Button>().Length);
                yield return Capture(language+"-320-draw",app,320,568,true);
            }
        }
    }
    public sealed class RankDataTests
    {
        [Test] public void MissingAndUnknownCharacterIdsAreRejected()
        {
            foreach(string value in new[]{null,"","C00","C20","C1","Cxx","unknown"})
            {
                var data=GameData.Load();data.Stages[0].characterId=value;
                Assert.Throws<FormatException>(()=>data.ValidateRankData());
            }
        }
        [TestCase("rankKey")][TestCase("descriptionKey")][TestCase("aiIntroKey")][TestCase("aiWinKey")][TestCase("aiLoseKey")]
        public void MissingBlankAndUnknownRankReferencesAreRejected(string field)
        {
            foreach(string value in new[]{null,""," ","missing-key"}){var data=GameData.Load();typeof(StageDefinition).GetField(field).SetValue(data.Stages[0],value);Assert.Throws<FormatException>(()=>data.ValidateRankData());}
        }
        [TestCase(0f)][TestCase(8.5f)][TestCase(1.25f)][TestCase(2f)]public void InvalidOrDecreasingStarsAreRejected(float stars)
        {var data=GameData.Load();data.Stages[0].difficultyStars=stars;Assert.Throws<FormatException>(()=>data.ValidateRankData());}
    }
}
