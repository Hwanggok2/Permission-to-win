using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace StageGame.Tests
{
    public sealed partial class RankFlowTests
    {
        [Serializable] sealed class ViewportMeasurement
        {
            public string name;
            public float titleTopCss, boardWidthCss, contentWidthCss, minimumTapCss, bottomCss;
        }
        [Serializable] sealed class ViewportMeasurements { public List<ViewportMeasurement> screens=new List<ViewportMeasurement>(); }
        [UnityTest] public IEnumerator MobileVisualViewportKeepsWidthAndCssTouchSizeAcrossPixelRatios()
        {
            var report=new ViewportMeasurements();
            foreach(string language in new[]{"ko","en"})
            foreach(var size in new[]{new Vector2Int(390,664),new Vector2Int(390,844),new Vector2Int(454,768),new Vector2Int(1080,1920)})
            foreach(float ratio in size==new Vector2Int(390,664)?new[]{1f,1.5f,2f}:new[]{1f})
            {
                app.SetLanguage(language);
                string prefix=$"{language}-{size.x}x{size.y}-dpr{ratio}";
                app.StartStage(0);
                yield return SpeechComplete();
                yield return Capture(prefix+"-match",app,Mathf.RoundToInt(size.x*ratio),Mathf.RoundToInt(size.y*ratio),scaledCanvas:true,pixelRatio:ratio,validate:()=>
                {
                    var measure=MeasureViewport(prefix+"-match",size,ratio);
                    AssertMatchFits(app);
                    Assert.Less(measure.titleTopCss/size.x,.34f,"Plain webview must not add the Toss control inset.");
                    Assert.Greater(measure.boardWidthCss/size.x,.80f,"Board should fill the available width.");
                    Assert.GreaterOrEqual(measure.minimumTapCss,43.9f,"Touch targets must use CSS pixels at every DPR.");
                    report.screens.Add(measure);
                });
                app.ShowLobby();
                yield return Capture(prefix+"-lobby",app,Mathf.RoundToInt(size.x*ratio),Mathf.RoundToInt(size.y*ratio),scaledCanvas:true,pixelRatio:ratio,validate:()=>
                {
                    var measure=MeasureViewport(prefix+"-lobby",size,ratio);
                    Assert.Less(measure.titleTopCss/size.x,.34f);
                    report.screens.Add(measure);
                });
            }
            foreach(float ratio in new[]{1f,1.5f,2f})
            {
                app.StartStage(0);yield return SpeechComplete();
                yield return Capture("toss-390x844-dpr"+ratio+"-match",app,Mathf.RoundToInt(390*ratio),Mathf.RoundToInt(844*ratio),true,scaledCanvas:true,pixelRatio:ratio);
            }
            var first=report.screens[0];
            foreach(var measure in report.screens)
                if(measure.name.StartsWith("ko-390x664")&&measure.name.EndsWith("-match"))
                {
                    Assert.AreEqual(first.titleTopCss,measure.titleTopCss,1);
                    Assert.AreEqual(first.boardWidthCss,measure.boardWidthCss,1);
                    Assert.AreEqual(first.minimumTapCss,measure.minimumTapCss,1);
                }
            string directory=Environment.GetEnvironmentVariable("STAGEGAME_CAPTURE_DIRECTORY")??"Evidence/web-audio-fsb-20261016/screens";
            Directory.CreateDirectory(directory);File.WriteAllText(Path.Combine(directory,"viewport-metrics.json"),JsonUtility.ToJson(report,true));
        }
        ViewportMeasurement MeasureViewport(string name,Vector2Int size,float ratio)
        {
            var canvas=app.GetComponent<Canvas>();var camera=canvas.worldCamera;
            var fitter=app.GetComponentInChildren<SafeAreaFitter>();
            Func<RectTransform,Rect> bounds=rect=>
            {
                var corners=new Vector3[4];rect.GetWorldCorners(corners);
                var lo=RectTransformUtility.WorldToScreenPoint(camera,corners[0])/ratio;
                var hi=RectTransformUtility.WorldToScreenPoint(camera,corners[2])/ratio;
                return Rect.MinMaxRect(lo.x,lo.y,hi.x,hi.y);
            };
            var title=app.lobby.activeSelf?app.lobbyTitle:app.matchTitle;
            var measure=new ViewportMeasurement{name=name,titleTopCss=size.y-bounds(title.rectTransform).center.y,contentWidthCss=bounds(fitter.content).width};
            Assert.AreEqual(size.x,measure.contentWidthCss,1,"Design content must fill the CSS viewport width.");
            if(app.match.activeSelf)
            {
                measure.boardWidthCss=bounds(app.boardView).width;
                measure.minimumTapCss=float.MaxValue;measure.bottomCss=float.MaxValue;
                foreach(var button in app.match.GetComponentsInChildren<Button>())
                {
                    var box=bounds((RectTransform)button.transform);
                    measure.minimumTapCss=Mathf.Min(measure.minimumTapCss,box.width,box.height);
                    measure.bottomCss=Mathf.Min(measure.bottomCss,box.yMin);
                }
            }
            Debug.Log("[MobileViewport] "+JsonUtility.ToJson(measure));return measure;
        }
    }
}
