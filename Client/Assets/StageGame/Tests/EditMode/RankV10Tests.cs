using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace StageGame.Tests
{
    public sealed partial class RankFlowTests
    {
        double ActiveClock()=>(double)typeof(RankGameApp).GetMethod("ActiveTime",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(app,null);
        [UnityTest] public IEnumerator AiWaitOverlapsSearchAndExcludesPauseAndConfirmationTime()
        {
            double started=ActiveClock();float wall=Time.realtimeSinceStartup;app.SelectColumn(3);app.PlaceSelected();
            yield return new WaitForSecondsRealtime(.6f);Assert.AreEqual(1,Board.MoveCount);
            app.pause.onClick.Invoke();double paused=ActiveClock();yield return new WaitForSecondsRealtime(.4f);Assert.AreEqual(paused,ActiveClock(),.001);Assert.AreEqual(1,Board.MoveCount);
            app.confirmResume.onClick.Invoke();app.matchRetry.onClick.Invoke();paused=ActiveClock();yield return new WaitForSecondsRealtime(.4f);Assert.AreEqual(paused,ActiveClock(),.001);Assert.AreEqual(1,Board.MoveCount);
            app.actionCancel.onClick.Invoke();yield return AiReady();double duration=ActiveClock()-started;
            Assert.GreaterOrEqual(duration,.98);Assert.Less(duration,1.4);Assert.GreaterOrEqual(Time.realtimeSinceStartup-wall,1.78);Assert.AreEqual(2,Board.MoveCount);
            Progress.Data.unlockedStage=30;var stage=Data.Stages[29];stage.starter=-1;stage.openingMoves="";
            started=ActiveClock();app.StartStage(29);yield return AiReady();duration=ActiveClock()-started;Assert.GreaterOrEqual(duration,.98);Assert.Less(duration,1.4);Assert.AreEqual(1,Board.MoveCount);
            Debug.Log("[V10 AI] Highest-budget response active seconds="+duration+"; search and delay overlap.");
        }
        [UnityTest] public IEnumerator AiWaitingCancelsOnUndoRestartLeaveAndHintsHaveNoOneSecondDelay()
        {
            app.SelectColumn(3);app.PlaceSelected();yield return new WaitForSecondsRealtime(.6f);app.UndoTurn();yield return new WaitForSecondsRealtime(.6f);Assert.AreEqual(0,Board.MoveCount);
            app.SelectColumn(3);app.PlaceSelected();app.matchRetry.onClick.Invoke();app.actionAccept.onClick.Invoke();yield return new WaitForSecondsRealtime(1.1f);Assert.AreEqual(0,Board.MoveCount);
            app.SelectColumn(2);app.PlaceSelected();app.pause.onClick.Invoke();app.confirmLeave.onClick.Invoke();app.actionAccept.onClick.Invoke();int before=Board.MoveCount;yield return new WaitForSecondsRealtime(1.1f);Assert.AreEqual(before,Board.MoveCount);Assert.IsTrue(app.lobby.activeSelf);
            app.StartStage(0);app.SetAdsForTests(new EditorTestAds{Result=AdResult.Completed});float start=Time.realtimeSinceStartup;app.hint.onClick.Invoke();yield return AiReady();Assert.Less(Time.realtimeSinceStartup-start,.85f);Assert.IsTrue(Field<RewardGate>("rewardGate").Granted);Assert.AreEqual(0,Board.MoveCount);
        }
        [UnityTest] public IEnumerator Actual1080DesignFitsAllActionsBelowSharedTopGap()
        {
            const string evidence="Evidence/opening-ranks-20261015/regression";
            Assert.AreEqual(new Vector2(1080,1920),app.GetComponent<CanvasScaler>().referenceResolution);
            Assert.AreEqual(new Vector2(1080,1920),app.GetComponentInChildren<SafeAreaFitter>().referenceSize);
            Assert.AreEqual(78,app.matchTitle.fontSize);Assert.AreEqual(60,app.lobbyTitle.fontSize);
            Assert.AreEqual(RankUiMetrics.Point(704,704),app.boardView.sizeDelta);Assert.AreEqual(RankUiMetrics.Point(110,110),app.place.GetComponent<RectTransform>().sizeDelta);
            foreach(var image in app.turnToast.GetComponentsInChildren<Image>(true))Assert.IsFalse(image.enabled,"Only text may be rendered by the turn toast.");
            Assert.IsFalse(app.turnToast.blocksRaycasts);Assert.IsFalse(app.turnToast.interactable);
            float pauseTop=0;
            float Top(RectTransform rect){var corners=new Vector3[4];rect.GetWorldCorners(corners);return RectTransformUtility.WorldToScreenPoint(app.GetComponent<Canvas>().worldCamera,corners[2]).y;}
            yield return Capture("ko-1080-text-only-toast",app,1080,1920,true,directory:evidence);
            yield return new WaitForSecondsRealtime(.9f);
            yield return Capture("ko-1080-header-match",app,1080,1920,true,false,()=>
            {
                pauseTop=Top(app.pause.GetComponent<RectTransform>());AssertMatchFits(app);
            },directory:evidence);
            yield return Capture("ko-1080-plain-match",app,1080,1920,false,false,()=>
            {
                AssertMatchFits(app);
            },directory:evidence);
            app.ShowLobby();yield return Capture("ko-1080-header-lobby",app,1080,1920,true,false,()=>
            {
                Assert.AreEqual(pauseTop,Top(app.lobbyHelp.GetComponent<RectTransform>()),.1f);Assert.AreEqual(pauseTop,Top(app.lobbySettings.GetComponent<RectTransform>()),.1f);
            },directory:evidence);
            foreach(var size in new[]{new Vector2Int(1080,1920),new Vector2Int(390,844),new Vector2Int(320,568)})
                foreach(bool host in new[]{false,true})
                {
                    app.stageScroll.verticalNormalizedPosition=1;
                    yield return Capture("ko-"+size.x+(host?"-host":"-plain")+"-lobby",app,size.x,size.y,host,false,()=>
                    {
                        var corners=new Vector3[4];app.lobbySettings.GetComponent<RectTransform>().GetWorldCorners(corners);
                        float headerTop=RectTransformUtility.WorldToScreenPoint(app.GetComponent<Canvas>().worldCamera,corners[2]).y;
                        Assert.GreaterOrEqual(size.y-headerTop,64-.1f,"Saved lobby must keep a 64px top exclusion area even without host insets.");
                        app.stageScroll.viewport.GetWorldCorners(corners);
                        float listTop=RectTransformUtility.WorldToScreenPoint(app.GetComponent<Canvas>().worldCamera,corners[2]).y;
                        app.lobbySettings.GetComponent<RectTransform>().GetWorldCorners(corners);
                        Assert.Less(listTop,RectTransformUtility.WorldToScreenPoint(app.GetComponent<Canvas>().worldCamera,corners[0]).y,"Stage list must stay below header.");
                    },directory:evidence);
                    app.stageScroll.verticalNormalizedPosition=0;
                    yield return Capture("ko-"+size.x+(host?"-host":"-plain")+"-lobby-end",app,size.x,size.y,host,directory:evidence);
                }
            app.StartStage(0);Assert.AreSame(app.theme.Portrait("C01").intro,app.aiFace.sprite);Assert.IsFalse(app.rankHat.gameObject.activeSelf);
            Debug.Log("[1080 fit] Shared216design-unit top gap retained; all normal/result actions fit1080x1920 without scrolling.");
        }
    }
}
