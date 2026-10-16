using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace StageGame.Tests
{
    public sealed class RankDetailLayoutAssetTests
    {
        [Test] public void DetailFitterAndTextSpaceAreSavedInBothPrefabs()
        {
            var app=AssetDatabase.LoadAssetAtPath<GameObject>(Editor.RankUiBuilder.AppRootPath).GetComponent<RankGameApp>();
            foreach(var detail in new[]{AssetDatabase.LoadAssetAtPath<GameObject>(Editor.RankUiBuilder.Prefabs+"Detail.prefab"),app.detail.gameObject})
            {
                var fit=detail.GetComponent<RankDetailFitter>();Assert.NotNull(fit);
                Assert.NotNull(fit.scroll);Assert.NotNull(fit.number);Assert.NotNull(fit.close);
                Assert.AreEqual(RankUiMetrics.U(1005),fit.scroll.content.rect.height);
                var text=fit.scroll.content.Find("Description").GetComponent<UnityEngine.UI.Text>();
                Assert.AreEqual(RankUiMetrics.U(100),text.rectTransform.sizeDelta.y);
                Assert.AreEqual(RankUiMetrics.U(26),text.fontSize,"Retain readable font size rather than shrinking the description.");
            }
        }
    }
    public sealed partial class RankFlowTests
    {
        [Serializable] sealed class DetailMeasure
        {
            public string name;
            public float topGap,bottomGap,viewportHeight,contentHeight,minimumTap;
        }
        [Serializable] sealed class DetailReport { public List<DetailMeasure> screens=new List<DetailMeasure>(); }
        [UnityTest] public IEnumerator DetailCentersAfterSlideNavigationAndResizeWhileSmallScreensScroll()
        {
            var report=new DetailReport();
            foreach(string language in new[]{"ko","en"})
            foreach(bool host in new[]{false,true})
            foreach(var size in new[]{new Vector2Int(1080,1920),new Vector2Int(320,568),new Vector2Int(390,664),new Vector2Int(390,844),new Vector2Int(405,720),new Vector2Int(480,1080)})
            {
                string prefix=language+"-"+size.x+"x"+size.y+(host?"-host":"-plain");
                app.SetLanguage(language);app.ShowLobby();app.OpenDetail(0);
                Assert.IsTrue(Field<bool>("detailSliding"));Assert.IsFalse(app.play.interactable);
                yield return new WaitForSecondsRealtime(.25f);
                Assert.IsFalse(Field<bool>("detailSliding"));Assert.AreEqual(Vector2.zero,app.detail.anchoredPosition);
                yield return Capture(prefix+"-detail",app,size.x,size.y,host:host,scaledCanvas:true,validate:()=>
                {
                    var outer=ScreenBounds(app.detail);var close=ScreenBounds((RectTransform)app.detailClose.transform);
                    var viewport=ScreenBounds(app.detailScroll.viewport);var body=ScreenBounds(app.detailScroll.content);
                    var record=new DetailMeasure{name=prefix,topGap=outer.yMax-close.yMax,bottomGap=viewport.yMin-outer.yMin,viewportHeight=viewport.height,contentHeight=body.height,minimumTap=float.MaxValue};
                    Assert.AreEqual(record.topGap,record.bottomGap,1,"Header and body must have balanced outer gaps.");
                    Assert.Greater(record.topGap,0);Assert.Greater(viewport.height,0);AssertClickable(app.detailClose);
                    foreach(var button in new[]{app.detailClose,app.previous,app.following,app.play})
                    {
                        var box=ScreenBounds((RectTransform)button.transform);record.minimumTap=Mathf.Min(record.minimumTap,box.width,box.height);
                    }
                    Assert.GreaterOrEqual(record.minimumTap,43.99f);report.screens.Add(record);
                    var headerPosition=app.detailNumber.rectTransform.anchoredPosition;
                    app.following.onClick.Invoke();Assert.AreEqual(1,Field<int>("selected"));
                    app.previous.onClick.Invoke();Assert.AreEqual(0,Field<int>("selected"));
                    Assert.AreEqual(headerPosition,app.detailNumber.rectTransform.anchoredPosition);
                    Assert.AreEqual(Vector2.zero,app.detail.anchoredPosition);
                    Assert.LessOrEqual(app.detailDescription.preferredHeight,app.detailDescription.rectTransform.rect.height+1);
                });
                yield return Capture(prefix+"-detail-bottom",app,size.x,size.y,host:host,bottom:true,scaledCanvas:true,validate:()=>
                {
                    var button=ScreenBounds((RectTransform)app.play.transform);var viewport=ScreenBounds(app.detailScroll.viewport);
                    Assert.GreaterOrEqual(button.yMin,viewport.yMin-1);Assert.LessOrEqual(button.yMax,viewport.yMax+1);
                    AssertClickable(app.play);AssertClickable(app.detailClose);
                    Assert.LessOrEqual(app.detailDescription.preferredHeight,app.detailDescription.rectTransform.rect.height+1);
                });
                app.play.onClick.Invoke();Assert.IsTrue(app.match.activeSelf);Assert.AreEqual(0,Field<int>("activeStage"));
                app.ShowLobby();app.OpenDetail(0);yield return new WaitForSecondsRealtime(.25f);
                app.detailClose.onClick.Invoke();Assert.IsTrue(Field<bool>("detailSliding"));
                yield return new WaitForSecondsRealtime(.25f);Assert.IsFalse(app.detail.gameObject.activeSelf);
            }
            string directory=Environment.GetEnvironmentVariable("STAGEGAME_CAPTURE_DIRECTORY")??"Evidence/detail-centered-20261016/screens";
            File.WriteAllText(Path.Combine(directory,"detail-metrics.json"),JsonUtility.ToJson(report,true));
        }
        [UnityTest] public IEnumerator EveryDetailDescriptionFitsWithoutReducingFontSize()
        {
            app.ShowLobby();app.OpenDetail(0);yield return new WaitForSecondsRealtime(.25f);
            foreach(string language in new[]{"ko","en"})
            {
                app.SetLanguage(language);
                for(int index=0;index<Data.Stages.Length;index++)
                {
                    Set("selected",index);Call("RefreshDetail");Canvas.ForceUpdateCanvases();
                    Assert.LessOrEqual(app.detailDescription.preferredHeight,app.detailDescription.rectTransform.rect.height+1,language+" stage "+(index+1));
                    Assert.AreEqual(RankUiMetrics.U(26),app.detailDescription.fontSize);
                }
            }
            Set("selected",4);Call("RefreshDetail");
            yield return Capture("en-390x664-longest-description",app,390,664,scaledCanvas:true,bottom:true);
        }
    }
}
