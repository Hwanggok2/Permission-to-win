using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace StageGame.Tests
{
    public sealed partial class RankFlowTests
    {
        [Serializable] sealed class DesktopViewportRecord
        {
            public string name;
            public float canvasWidth,canvasHeight,contentWidth,listHeight,minimumTap;
        }
        [Serializable] sealed class DesktopViewportReport { public List<DesktopViewportRecord> screens=new List<DesktopViewportRecord>(); }

        Rect ScreenBounds(RectTransform rect)
        {
            var corners=new Vector3[4];rect.GetWorldCorners(corners);var camera=app.GetComponent<Canvas>().worldCamera;
            var lo=RectTransformUtility.WorldToScreenPoint(camera,corners[0]);var hi=RectTransformUtility.WorldToScreenPoint(camera,corners[2]);
            return Rect.MinMaxRect(lo.x,lo.y,hi.x,hi.y);
        }
        void AssertClickable(Button button)
        {
            var pointer=new PointerEventData(EventSystem.current){position=ScreenBounds((RectTransform)button.transform).center};
            var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);Assert.IsNotEmpty(hits,button.name);
            Assert.AreEqual(button.gameObject,ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject),button.name);
        }
        DesktopViewportRecord MeasureDesktop(string name,Vector2Int canvasSize)
        {
            var content=app.GetComponentInChildren<SafeAreaFitter>().content;var bounds=ScreenBounds(content);
            Assert.AreEqual(canvasSize.x/2f,bounds.center.x,1);Assert.LessOrEqual(bounds.width,canvasSize.x+1);
            var record=new DesktopViewportRecord{name=name,canvasWidth=canvasSize.x,canvasHeight=canvasSize.y,contentWidth=bounds.width,minimumTap=float.MaxValue};
            foreach(var button in (app.lobby.activeSelf?app.lobby:app.match).GetComponentsInChildren<Button>())
            {
                var box=ScreenBounds((RectTransform)button.transform);
                record.minimumTap=Mathf.Min(record.minimumTap,box.width,box.height);
            }
            Assert.GreaterOrEqual(record.minimumTap,43.9f,name+" touch size");
            if(app.lobby.activeSelf)
            {
                record.listHeight=ScreenBounds(app.stageScroll.viewport).height;
                Assert.Greater(record.listHeight,40,name+" stage list collapsed");
                var list=ScreenBounds(app.stageScroll.viewport);var title=ScreenBounds(app.lobbyTitle.rectTransform);
                var bottom=ScreenBounds((RectTransform)app.multiplayer.transform);
                Assert.Less(list.yMax,title.yMin);Assert.Greater(list.yMin,bottom.yMax);
                var help=ScreenBounds((RectTransform)app.lobbyHelp.transform);
                var settings=ScreenBounds((RectTransform)app.lobbySettings.transform);
                Assert.Greater(help.yMin,list.yMax);Assert.Greater(settings.yMin,list.yMax);
                Assert.AreEqual(help.center.y,settings.center.y,.1f);
                AssertClickable(app.lobbyHelp);AssertClickable(app.lobbySettings);
            }
            return record;
        }
        [UnityTest] public IEnumerator DesktopAndMobileCanvasSizesKeepListsInputAndScrollingUsable()
        {
            var report=new DesktopViewportReport();
            // Actual canvas CSS sizes emitted by host-layout.js, including a tall desktop window.
            foreach(string language in new[]{"ko","en"})
            foreach(var item in new[]{
                ("desktop-1280x720",new Vector2Int(405,720)),("desktop-1920x1080",new Vector2Int(480,1080)),
                ("desktop-2556x1338",new Vector2Int(480,1338)),("desktop-800x1200",new Vector2Int(480,1200)),
                ("mobile-320x568",new Vector2Int(320,568)),("mobile-390x844",new Vector2Int(390,844)),
                ("mobile-390x664",new Vector2Int(390,664)),("rotated-844x390",new Vector2Int(320,390))})
            {
                var size=item.Item2;string prefix=language+"-"+item.Item1;
                app.SetLanguage(language);app.ShowLobby();app.stageScroll.verticalNormalizedPosition=1;
                yield return Capture(prefix+"-lobby",app,size.x,size.y,scaledCanvas:true,validate:()=>
                {
                    report.screens.Add(MeasureDesktop(prefix+"-lobby",size));
                    if(size.y>=568)
                    {
                        var view=ScreenBounds(app.stageScroll.viewport);
                        var secondRow=ScreenBounds((RectTransform)app.cards[2].transform);
                        var thirdRow=ScreenBounds((RectTransform)app.cards[4].transform);
                        Assert.GreaterOrEqual(secondRow.yMin,view.yMin,"Second row remains fully visible");
                        Assert.AreEqual(thirdRow.center.y,view.yMin,1,"Third row is half visible as a scroll cue");
                    }
                    AssertClickable(app.cards[0].button);
                });
                app.stageScroll.verticalNormalizedPosition=0;
                yield return Capture(prefix+"-lobby-end",app,size.x,size.y,scaledCanvas:true,validate:()=>
                {
                    MeasureDesktop(prefix+"-lobby-end",size);
                    var last=ScreenBounds((RectTransform)app.cards[29].transform);var view=ScreenBounds(app.stageScroll.viewport);
                    Assert.GreaterOrEqual(last.yMin,view.yMin-1);Assert.Less(last.yMin,view.yMax);
                });
                app.StartStage(0);
                yield return Capture(prefix+"-match",app,size.x,size.y,scaledCanvas:true,validate:()=>
                {
                    report.screens.Add(MeasureDesktop(prefix+"-match",size));
                    AssertClickable(app.matchHelp);AssertClickable(app.pause);
                    if(size.y>=664)AssertMatchFits(app);
                });
                yield return Capture(prefix+"-match-bottom",app,size.x,size.y,bottom:true,scaledCanvas:true,validate:()=>
                {
                    AssertClickable(app.matchRetry);var retry=ScreenBounds((RectTransform)app.matchRetry.transform);
                    var view=ScreenBounds(app.matchScroll.viewport);Assert.GreaterOrEqual(retry.yMin,view.yMin-1);Assert.LessOrEqual(retry.yMax,view.yMax+1);
                });
            }
            string directory=Environment.GetEnvironmentVariable("STAGEGAME_CAPTURE_DIRECTORY")??"Evidence/desktop-centered-20261016/screens";
            File.WriteAllText(Path.Combine(directory,"layout-metrics.json"),JsonUtility.ToJson(report,true));
        }
        [UnityTest] public IEnumerator NativeWideViewportFallbackAndRotatedHostKeepControlsReachable()
        {
            foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1920,1080),new Vector2Int(2556,1338)})
            {
                app.ShowLobby();
                yield return Capture("native-"+size.x+"x"+size.y+"-lobby",app,size.x,size.y,scaledCanvas:true,validate:()=>
                {
                    var record=MeasureDesktop("native-lobby",size);Assert.LessOrEqual(record.contentWidth,480.1f);
                });
                app.StartStage(0);
                yield return Capture("native-"+size.x+"x"+size.y+"-match",app,size.x,size.y,scaledCanvas:true,validate:()=>
                {
                    MeasureDesktop("native-match",size);AssertMatchFits(app);
                });
            }
            // A phone rotated within the host retains the 44 CSS-pixel touch floor and vertical scrolling.
            app.StartStage(0);
            yield return Capture("host-rotated-320x390-top",app,640,780,host:true,scaledCanvas:true,pixelRatio:2);
            yield return Capture("host-rotated-320x390-bottom",app,640,780,host:true,bottom:true,scaledCanvas:true,pixelRatio:2,validate:()=>AssertClickable(app.matchRetry));
        }
    }
}
