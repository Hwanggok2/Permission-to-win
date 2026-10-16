using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace StageGame.Tests
{
    public sealed class RankFirstRulesAssetTests
    {
        [Test] public void SavedGuideAndCompactBackgroundsKeepInputSeparateFromDecoration()
        {
            var app=AssetDatabase.LoadAssetAtPath<GameObject>(Editor.RankUiBuilder.AppRootPath).GetComponent<RankGameApp>();
            Assert.AreSame(app.match.transform,app.turnToast.transform.parent);
            Assert.AreEqual(Color.white,app.turnToastLabel.color);
            Assert.IsFalse(app.turnToastLabel.raycastTarget);Assert.IsFalse(app.turnToast.blocksRaycasts);
            Assert.NotNull(app.turnToastLabel.GetComponent<Outline>());
            Assert.NotNull(app.matchHelp);CollectionAssert.Contains(app.soundButtons,app.matchHelp);
            var help=(RectTransform)app.matchHelp.transform;var pause=(RectTransform)app.pause.transform;
            Assert.AreEqual(pause.anchoredPosition.y,help.anchoredPosition.y);
            Assert.AreEqual(-pause.anchoredPosition.x,help.anchoredPosition.x);Assert.AreEqual(pause.sizeDelta,help.sizeDelta);
            Assert.AreEqual(app.theme.help,help.Find("Icon").GetComponent<Image>().sprite);
            Assert.AreEqual(RankUiMetrics.Point(64,64),help.Find("Backdrop").GetComponent<RectTransform>().sizeDelta);
            foreach(var button in new[]{app.undo,app.matchRetry})
            {
                Assert.AreEqual(RankUiMetrics.Point(224,110),((RectTransform)button.transform).sizeDelta);
                var backdrop=button.transform.Find("Backdrop").GetComponent<Image>();
                Assert.AreEqual(RankUiMetrics.Point(170,76),backdrop.rectTransform.sizeDelta);
                Assert.AreSame(backdrop,button.targetGraphic);Assert.IsFalse(backdrop.raycastTarget);
                Assert.IsTrue(button.GetComponent<Image>().raycastTarget);Assert.AreEqual(0,button.GetComponent<Image>().color.a);
            }
        }
    }
    public sealed partial class RankFlowTests
    {
        // Emulate process initialization while preserving PlayerPrefs, using Unity's actual launch hook.
        static void ResetAppSessionGuide() => typeof(RankGameApp).GetMethod("ResetSessionGuide",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,null);

        [UnityTest] public IEnumerator FirstLaunchRulesSuspendPlayAndAppearOnlyOnce()
        {
            app.ShowLobby();ResetAppSessionGuide();PlayerPrefs.DeleteKey(Key);PlayerPrefs.Save();SceneManager.LoadScene("Main");
            yield return null;yield return null;app=Object.FindFirstObjectByType<RankGameApp>();
            Call("OnApplicationFocus",true);
            Assert.IsTrue(app.help.activeSelf);Assert.IsTrue(app.match.activeSelf);Assert.IsFalse(app.turnToast.gameObject.activeSelf);
            Assert.AreEqual(30,Remaining);AdvanceTimer(90);app.SelectColumn(3);app.PlaceSelected();
            yield return new WaitForSecondsRealtime(.2f);
            Assert.AreEqual(30,Remaining);Assert.AreEqual(0,Board.MoveCount);Assert.AreEqual(0,Field<int>("speechVisible"));
            Assert.IsFalse(app.pause.interactable);Assert.IsFalse(app.matchRetry.interactable);Assert.IsFalse(app.hint.interactable);
            app.pause.onClick.Invoke();app.matchRetry.onClick.Invoke();app.hint.onClick.Invoke();
            Assert.IsFalse(app.confirm.activeSelf);Assert.IsFalse(app.actionConfirm.activeSelf);Assert.IsFalse(Field<RewardGate>("rewardGate").Pending);
            Assert.IsTrue(app.musicSource.isPlaying,"Rules must not stop background music.");
            foreach(string language in new[]{"ko","en"})
            {
                app.SetLanguage(language);
                Assert.AreEqual(Data.Text("help_connect_title"),app.help.transform.Find("Frame/ConnectTitle").GetComponent<Text>().text);
                Assert.AreEqual(Data.Text("help_drop_caption"),app.help.transform.Find("Frame/DropCaption").GetComponent<Text>().text);
                foreach(var size in new[]{new Vector2Int(390,664),new Vector2Int(390,844),new Vector2Int(454,768),new Vector2Int(1080,1920)})
                    yield return Capture(language+"-"+size.x+"x"+size.y+"-first-rules",app,size.x,size.y,scaledCanvas:true);
            }
            float remaining=Remaining;app.helpClose.onClick.Invoke();
            Assert.IsFalse(app.help.activeSelf);Assert.AreEqual(remaining,Remaining);
            Assert.IsTrue(app.turnToast.gameObject.activeSelf);Assert.AreEqual(Data.Text("select_column"),app.turnToastLabel.text);
            Assert.IsFalse(app.matchNotice.gameObject.activeSelf,"Do not duplicate the guide at the bottom.");
            app.SelectColumn(3);Assert.IsFalse(app.turnToast.gameObject.activeSelf);app.PlaceSelected();Assert.AreEqual(1,Board.MoveCount);
            app.ShowLobby();SceneManager.LoadScene("Main");yield return null;yield return null;
            app=Object.FindFirstObjectByType<RankGameApp>();Assert.IsTrue(app.lobby.activeSelf);Assert.IsFalse(app.help.activeSelf);
            app.lobbyHelp.onClick.Invoke();Assert.IsTrue(app.help.activeSelf);app.helpClose.onClick.Invoke();Assert.IsFalse(app.help.activeSelf);
        }
        [UnityTest] public IEnumerator WhitePlacementGuideIsCenteredAndCompactButtonsRetainClickablePadding()
        {
            foreach(string language in new[]{"ko","en"})
            foreach(var size in new[]{new Vector2Int(390,664),new Vector2Int(390,844),new Vector2Int(454,768),new Vector2Int(1080,1920)})
            {
                app.SetLanguage(language);ResetAppSessionGuide();app.StartStage(0);
                yield return Capture(language+"-"+size.x+"x"+size.y+"-white-guide",app,size.x,size.y,scaledCanvas:true,validate:()=>
                {
                    var canvas=app.GetComponent<Canvas>();var camera=canvas.worldCamera;
                    var center=RectTransformUtility.WorldToScreenPoint(camera,app.turnToastLabel.transform.position);
                    Assert.AreEqual(size.x/2f,center.x,1);Assert.AreEqual(size.y/2f,center.y,5);
                    Assert.AreEqual(Color.white,app.turnToastLabel.color);Assert.Greater(app.turnToast.alpha,.9f);
                    Assert.IsFalse(app.matchNotice.gameObject.activeSelf);AssertMatchFits(app);
                    // The transparent padding outside the smaller background still activates the same button.
                    var rect=(RectTransform)app.matchRetry.transform;
                    foreach(float fraction in new[]{-.46f,0f,.46f})
                    {
                        var point=rect.TransformPoint(new Vector3(rect.rect.width*fraction,0,0));
                        var pointer=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(camera,point)};
                        var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);Assert.IsNotEmpty(hits);
                        Assert.AreEqual(app.matchRetry.gameObject,ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject));
                    }
                });
            }
            yield return new WaitForSecondsRealtime(3.1f);Assert.IsFalse(app.turnToast.gameObject.activeSelf);
            app.matchRetry.onClick.Invoke();Assert.IsTrue(app.actionConfirm.activeSelf);app.actionAccept.onClick.Invoke();
            Assert.IsFalse(app.turnToast.gameObject.activeSelf);app.SelectColumn(2);app.PlaceSelected();Assert.IsFalse(app.turnToast.gameObject.activeSelf);
            app.UndoTurn();Assert.AreEqual(0,Board.MoveCount);Assert.IsFalse(app.turnToast.gameObject.activeSelf);
        }
        [UnityTest] public IEnumerator MatchHelpReopensRulesWithoutChangingSelectionTimerOrAi()
        {
            Call("OnApplicationFocus",true);AdvanceTimer(5);app.SelectColumn(3);var before=Board.Clone();float remaining=Remaining;
            app.matchHelp.onClick.Invoke();Assert.IsTrue(app.help.activeSelf);Assert.IsFalse(app.turnToast.gameObject.activeSelf);
            AdvanceTimer(90);app.SelectColumn(2);app.PlaceSelected();app.UndoTurn();
            SameBoard(before,Board);Assert.AreEqual(remaining,Remaining);Assert.AreEqual(3,Field<int>("selectedColumn"));
            Assert.IsTrue(app.musicSource.isPlaying);Assert.IsFalse(app.matchHelp.interactable);
            app.helpClose.onClick.Invoke();Assert.IsFalse(app.help.activeSelf);Assert.IsFalse(app.turnToast.gameObject.activeSelf);
            Assert.IsTrue(app.movePreview.gameObject.activeSelf);Assert.IsTrue(app.place.interactable);
            app.PlaceSelected();Assert.IsTrue(Field<bool>("busy"));app.matchHelp.onClick.Invoke();
            yield return new WaitForSecondsRealtime(1.2f);Assert.AreEqual(1,Board.MoveCount);Assert.IsTrue(app.help.activeSelf);
            app.helpClose.onClick.Invoke();yield return AiReady();Assert.AreEqual(2,Board.MoveCount);Assert.IsFalse(app.turnToast.gameObject.activeSelf);
            foreach(string language in new[]{"ko","en"})
            foreach(var size in new[]{new Vector2Int(390,664),new Vector2Int(390,844),new Vector2Int(454,768),new Vector2Int(1080,1920)})
            {
                app.SetLanguage(language);
                yield return Capture(language+"-"+size.x+"x"+size.y+"-match-help",app,size.x,size.y,scaledCanvas:true,validate:()=>
                {
                    var camera=app.GetComponent<Canvas>().worldCamera;
                    var helpCenter=RectTransformUtility.WorldToScreenPoint(camera,app.matchHelp.transform.position);
                    var pauseCenter=RectTransformUtility.WorldToScreenPoint(camera,app.pause.transform.position);
                    Assert.AreEqual(pauseCenter.y,helpCenter.y,.1f);Assert.Less(helpCenter.x,size.x/2f);AssertMatchFits(app);
                    var pointer=new PointerEventData(EventSystem.current){position=helpCenter};var hits=new List<RaycastResult>();
                    EventSystem.current.RaycastAll(pointer,hits);Assert.IsNotEmpty(hits);
                    Assert.AreEqual(app.matchHelp.gameObject,ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject));
                    ExecuteEvents.Execute(app.matchHelp.gameObject,pointer,ExecuteEvents.pointerClickHandler);
                    Assert.IsTrue(app.help.activeSelf);app.helpClose.onClick.Invoke();
                });
            }
            app.matchHelp.onClick.Invoke();yield return Capture("en-1080x1920-reopened-rules",app,1080,1920,scaledCanvas:true);
        }
        [UnityTest] public IEnumerator SessionGuideSurvivesSceneReloadAndReturnsOnlyAfterNewLaunch()
        {
            app.SelectColumn(3);app.PlaceSelected();yield return AiReady();
            Assert.IsFalse(app.turnToast.gameObject.activeSelf);Assert.AreEqual(30,Remaining,.1f);
            app.ShowLobby();app.StartStage(0);Assert.IsFalse(app.turnToast.gameObject.activeSelf);
            app.matchHelp.onClick.Invoke();app.helpClose.onClick.Invoke();Assert.IsFalse(app.turnToast.gameObject.activeSelf);
            Call("OnApplicationPause",true);Call("OnApplicationPause",false);Assert.IsFalse(app.turnToast.gameObject.activeSelf);
            app.ShowLobby();SceneManager.LoadScene("Main");yield return null;yield return null;app=Object.FindFirstObjectByType<RankGameApp>();
            Assert.IsTrue(app.lobby.activeSelf);Assert.IsFalse(app.help.activeSelf);app.StartStage(0);Assert.IsFalse(app.turnToast.gameObject.activeSelf);
            app.ShowLobby();ResetAppSessionGuide();SceneManager.LoadScene("Main");yield return null;yield return null;app=Object.FindFirstObjectByType<RankGameApp>();
            Assert.IsTrue(app.lobby.activeSelf);Assert.IsFalse(app.help.activeSelf);app.StartStage(0);
            Assert.IsTrue(app.turnToast.gameObject.activeSelf);app.SelectColumn(2);Assert.IsFalse(app.turnToast.gameObject.activeSelf);
            foreach(string language in new[]{"ko","en"})
            {
                app.SetLanguage(language);app.SelectColumn(4);
                Assert.IsEmpty(app.matchNotice.text);Assert.IsFalse(app.matchNotice.gameObject.activeSelf);Assert.IsTrue(app.movePreview.gameObject.activeSelf);
            }
            yield return Capture("en-390x664-selected-without-notice",app,390,664,scaledCanvas:true);
            Progress.Data.unlockedStage=2;app.StartStage(1);Assert.IsFalse(app.turnToast.gameObject.activeSelf);
        }
        [UnityTest] public IEnumerator AiOpeningDefersSessionGuideUntilHumanTurnButLeavingConsumesFirstMatch()
        {
            app.ShowLobby();ResetAppSessionGuide();Data.Stages[0].starter=-1;Data.Stages[0].openingMoves="";
            app.StartStage(0);Assert.IsFalse(app.turnToast.gameObject.activeSelf);yield return AiReady();
            Assert.AreEqual(1,Board.MoveCount);Assert.IsTrue(app.turnToast.gameObject.activeSelf);
            app.SelectColumn(3);app.PlaceSelected();yield return AiReady();Assert.IsFalse(app.turnToast.gameObject.activeSelf);
            app.ShowLobby();ResetAppSessionGuide();app.StartStage(0);app.ShowLobby();
            Data.Stages[0].starter=1;app.StartStage(0);Assert.IsFalse(app.turnToast.gameObject.activeSelf);
        }
    }
}
