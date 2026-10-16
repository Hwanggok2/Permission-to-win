using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace StageGame.Tests
{
    public sealed partial class RankFlowTests
    {
        [UnityTest] public IEnumerator DestructiveActionsRequireConfirmationAndCancelPreservesSelectionAndAi()
        {
            app.SelectColumn(3);string id=Field<string>("matchId");var before=Board.Clone();
            app.matchRetry.onClick.Invoke();Assert.IsTrue(app.actionConfirm.activeSelf);Assert.AreEqual(Data.Text("confirm_retry_title"),app.actionQuestion.text);
            app.SelectColumn(2);app.PlaceSelected();app.UndoTurn();SameBoard(before,Board);Assert.AreEqual(id,Field<string>("matchId"));
            app.actionCancel.onClick.Invoke();Assert.IsTrue(app.movePreview.gameObject.activeSelf);Assert.AreEqual(3,Field<int>("selectedColumn"));Assert.IsFalse(app.actionConfirm.activeSelf);
            app.PlaceSelected();Assert.IsTrue(Field<bool>("busy"));app.matchRetry.onClick.Invoke();
            yield return new WaitForSecondsRealtime(.6f);Assert.AreEqual(1,Board.MoveCount);Assert.IsTrue(Field<bool>("busy"));
            app.actionCancel.onClick.Invoke();yield return AiReady();Assert.AreEqual(2,Board.MoveCount);
            app.matchRetry.onClick.Invoke();app.actionAccept.onClick.Invoke();string restarted=Field<string>("matchId");app.actionAccept.onClick.Invoke();
            Assert.AreNotEqual(id,restarted);Assert.AreEqual(restarted,Field<string>("matchId"));Assert.AreEqual(0,Board.MoveCount);Assert.AreEqual(0,Progress.Data.completedMatches);
            app.pause.onClick.Invoke();app.confirmLeave.onClick.Invoke();Assert.IsTrue(app.actionConfirm.activeSelf);Assert.AreEqual(Data.Text("confirm_exit_title"),app.actionQuestion.text);
            app.actionCancel.onClick.Invoke();Assert.IsTrue(app.confirm.activeSelf);Assert.IsTrue(app.match.activeSelf);
            app.confirmLeave.onClick.Invoke();app.actionAccept.onClick.Invoke();app.actionAccept.onClick.Invoke();Assert.IsTrue(app.lobby.activeSelf);Assert.IsFalse(app.match.activeSelf);
        }
        [UnityTest] public IEnumerator ResultRestartAndLobbyConfirmWhileNextStaysDirect()
        {
            Inject(Replay(1,0,6,1,6,2,5,3));Call("Finish");yield return ResultReady();string id=Field<string>("matchId");int count=Progress.Data.completedMatches;
            app.retry.onClick.Invoke();Assert.IsTrue(app.actionConfirm.activeSelf);app.actionCancel.onClick.Invoke();Assert.IsTrue(app.result.activeSelf);Assert.AreEqual(id,Field<string>("matchId"));
            app.resultLobby.onClick.Invoke();Assert.IsTrue(app.actionConfirm.activeSelf);app.actionCancel.onClick.Invoke();Assert.AreEqual(count,Progress.Data.completedMatches);
            app.nextStage.onClick.Invoke();Assert.IsFalse(app.actionConfirm.activeSelf);Assert.AreEqual(1,Field<int>("activeStage"));
            Inject(Replay(-1,0,6,1,6,2,5,3));Call("Finish");yield return ResultReady();
            app.retry.onClick.Invoke();app.actionAccept.onClick.Invoke();Assert.AreEqual(1,Field<int>("activeStage"));Assert.IsFalse(app.result.activeSelf);
            Inject(Replay(-1,0,6,1,6,2,5,3));Call("Finish");yield return ResultReady();
            app.resultLobby.onClick.Invoke();app.actionAccept.onClick.Invoke();Assert.IsTrue(app.lobby.activeSelf);
        }
        [UnityTest] public IEnumerator TurnToastNeverBlocksAndDoesNotOutliveItsTurnOrDuplicateOnLanguageHint()
        {
            Assert.IsTrue(app.turnToast.gameObject.activeSelf);Assert.IsFalse(app.turnToast.blocksRaycasts);Assert.IsFalse(app.turnToast.interactable);
            var initial=Field<Coroutine>("toastRoutine");app.SetLanguage("en");Assert.AreSame(initial,Field<Coroutine>("toastRoutine"));app.hint.onClick.Invoke();Assert.AreSame(initial,Field<Coroutine>("toastRoutine"));
            yield return new WaitForSecondsRealtime(3.1f);Assert.IsFalse(app.turnToast.gameObject.activeSelf);
            app.StartStage(0);Assert.IsFalse(app.turnToast.gameObject.activeSelf);app.SelectColumn(3);app.PlaceSelected();Assert.IsFalse(app.turnToast.gameObject.activeSelf);
            yield return AiReady();Assert.IsFalse(app.turnToast.gameObject.activeSelf);app.UndoTurn();Assert.IsFalse(app.turnToast.gameObject.activeSelf);
            app.matchRetry.onClick.Invoke();Assert.IsFalse(app.turnToast.gameObject.activeSelf);app.actionCancel.onClick.Invoke();Assert.IsFalse(app.turnToast.gameObject.activeSelf);
            app.StartStage(0);app.pause.onClick.Invoke();Assert.IsFalse(app.turnToast.gameObject.activeSelf);app.confirmResume.onClick.Invoke();Assert.IsFalse(app.turnToast.gameObject.activeSelf);
            app.StartStage(0);yield return Capture("en-390-no-repeat-guide",app,390,844,true);
        }
        [UnityTest] public IEnumerator DestroyedAppRejectsLateResultAndRewardCallbacksWithoutSaving()
        {
            Progress.Data.completedMatches=4;var ads=new PendingAds();app.SetAdsForTests(ads);Inject(Replay(1,0,6,1,6,2,5,3));Call("Finish");
            float end=Time.realtimeSinceStartup+3;while(ads.Interstitial==null&&Time.realtimeSinceStartup<end)yield return null;Assert.NotNull(ads.Interstitial);
            var progress=Progress;int before=progress.Data.lastInterstitialMatch;string save=PlayerPrefs.GetString(Key);int cancels=ads.Cancels;
            UnityEngine.Object.Destroy(app.gameObject);yield return null;Assert.Greater(ads.Cancels,cancels);
            ads.Interstitial(AdResult.Completed);ads.Interstitial(AdResult.Failed);ads.Interstitial(AdResult.Completed);Assert.AreEqual(before,progress.Data.lastInterstitialMatch);Assert.AreEqual(save,PlayerPrefs.GetString(Key));
        }
        [UnityTest] public IEnumerator CancelledHintCannotChangeConfirmationOrRestoredNotice()
        {
            var ads=new PendingAds();app.SetAdsForTests(ads);app.hint.onClick.Invoke();var old=ads.Reward;app.matchRetry.onClick.Invoke();Assert.IsTrue(app.actionConfirm.activeSelf);
            string question=app.actionQuestion.text,notice=app.matchNotice.text;old(AdResult.Completed);Assert.AreEqual(question,app.actionQuestion.text);app.actionCancel.onClick.Invoke();old(AdResult.Failed);Assert.AreEqual(notice,app.matchNotice.text);
            app.hint.onClick.Invoke();old(AdResult.Completed);Assert.IsTrue(Field<RewardGate>("rewardGate").Pending);ads.Reward(AdResult.Unavailable);Assert.IsFalse(Field<RewardGate>("rewardGate").Granted);yield return null;
        }
        [UnityTest] public IEnumerator SquareBoardsUniformPixelLinesTopHeadersAndScrollableActionsMeetHostContract()
        {
            Rect Pixels(RectTransform rect)
            {
                var corners=new Vector3[4];rect.GetWorldCorners(corners);var camera=app.GetComponent<Canvas>().worldCamera;
                var a=RectTransformUtility.WorldToScreenPoint(camera,corners[0]);var b=RectTransformUtility.WorldToScreenPoint(camera,corners[2]);return Rect.MinMaxRect(a.x,a.y,b.x,b.y);
            }
            foreach(var size in new[]{new Vector2Int(390,844),new Vector2Int(320,568),new Vector2Int(390,844)})
            {
                app.StartStage(0);yield return new WaitForSecondsRealtime(.9f);
                yield return Capture("geometry-top-"+size.x,app,size.x,size.y,true,false,()=>
                {
                    var pause=Pixels((RectTransform)app.pause.transform);Assert.AreEqual(size.y-123-8-216*size.x/1080f,pause.yMax,.25f);
                    Assert.AreEqual(pause.yMax,Pixels(app.matchScroll.viewport).yMax,.25f,"Scrolling must stay below the shared top exclusion area.");
                    Assert.AreEqual(app.boardView.rect.width,app.boardView.rect.height,.001f);
                    float dx=app.stones[1].rectTransform.anchoredPosition.x-app.stones[0].rectTransform.anchoredPosition.x,dy=app.stones[7].rectTransform.anchoredPosition.y-app.stones[0].rectTransform.anchoredPosition.y;Assert.AreEqual(dx,dy,.001f);
                    var grid=app.boardView.GetComponent<UniformBoardLines>();float pixels=Pixels(grid.vertical[0]).width;Assert.AreEqual(Mathf.Round(pixels),pixels,.01f);
                    foreach(var line in grid.vertical)Assert.AreEqual(pixels,Pixels(line).width,.01f);foreach(var line in grid.horizontal)Assert.AreEqual(pixels,Pixels(line).height,.01f);
                    Assert.GreaterOrEqual(Pixels((RectTransform)app.columns[0].transform).width,44);Assert.IsFalse(app.playerAvatar.gameObject.activeSelf);Assert.IsFalse(app.turnLabel.gameObject.activeSelf);
                });
                yield return Capture("geometry-bottom-"+size.x,app,size.x,size.y,true,true,()=>
                {
                    foreach(var button in new[]{app.undo,app.place,app.matchRetry}){var rect=Pixels((RectTransform)button.transform);Assert.GreaterOrEqual(rect.height,44);Assert.GreaterOrEqual(rect.yMin,34-.1f);Assert.LessOrEqual(rect.yMax,size.y-123+.1f);}
                    var place=Pixels((RectTransform)app.place.transform);Assert.AreEqual(place.width,place.height,.01f);Assert.AreSame(app.theme.roundPrimary,app.place.GetComponent<Image>().sprite);
                });
            }
            Assert.AreEqual(19,app.theme.portraits.Select(p=>p.characterId).Distinct().Count());Progress.Data.unlockedStage=30;
            foreach(string language in new[]{"ko","en"})
            {
                app.SetLanguage(language);for(int index=0;index<30;index++)
                {
                    app.StartStage(index);Call("CancelSearch");Call("CancelToast");
                    var portrait=app.theme.Portrait(Data.Stages[index].characterId,Data.Stages[index].id>=25);
                    Assert.AreSame(Data.Stages[index].starter==-1?portrait.thinking:portrait.intro,app.aiFace.sprite);
                    Assert.IsFalse(app.rankHat.gameObject.activeSelf);
                    foreach(string key in new[]{Data.Stages[index].aiIntroKey,Data.Stages[index].aiWinKey,Data.Stages[index].aiLoseKey}){app.aiSpeech.text=Data.Text(key);Assert.LessOrEqual(app.aiSpeech.preferredHeight,app.aiSpeech.rectTransform.rect.height+1);}
                    if(index%3==0)
                    {
                        Call("RefreshMatchHeader");
                        var face=app.aiFace.rectTransform;var name=app.aiName.rectTransform;
                        Assert.Less(face.anchoredPosition.x+face.rect.width/2,name.anchoredPosition.x-name.rect.width/2,"Portrait must not overlap the opponent name.");
                        Assert.IsTrue(app.aiFace.preserveAspect);
                        yield return Capture(language+"-320-character-"+(index+1).ToString("00"),app,320,568,true,directory:"Evidence/opening-ranks-20261015/screens");
                    }
                }
                app.StartStage(0);yield return SpeechComplete();Assert.AreEqual(Data.Text("story_first_intro"),app.aiSpeech.text);yield return Capture(language+"-320-story-exact",app,320,568,true);
            }
            Debug.Log("[Pixel art] Character portraits, both languages, exact first story and square host profiles completed.");
        }
    }
}
