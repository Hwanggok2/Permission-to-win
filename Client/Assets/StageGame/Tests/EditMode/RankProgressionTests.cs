using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace StageGame.Tests
{
    public sealed partial class RankFlowTests
    {
        [UnityTest] public IEnumerator HalfStarsAndFinalRankRenderWithReservedBannerSpace()
        {
            const string directory="Evidence/opening-ranks-20261015/regression";
            Progress.Data.unlockedStage=30;
            Assert.AreEqual(1.5f,Data.Stages[2].difficultyStars,"JSON must preserve fractional stars.");
            Assert.AreEqual(8,Data.Stages[29].difficultyStars);
            Assert.AreEqual("story_opponent_030",Data.Stages[29].rankKey);
            Assert.AreEqual("C01",Data.Stages[29].characterId);
            foreach(string language in new[]{"ko","en"})
            {
                app.SetLanguage(language);app.ShowLobby();
                for(int i=0;i<30;i++)
                {
                    var rating=app.cards[i].stars.GetComponent<RankStarRating>();Assert.AreEqual(8,rating.filledStars.Length);
                    Assert.AreEqual(Data.Stages[i].difficultyStars,rating.Value);
                }
                var half=app.cards[2].stars.GetComponent<RankStarRating>();
                Assert.AreEqual(1,half.filledStars[0].fillAmount);Assert.AreEqual(.5f,half.filledStars[1].fillAmount);Assert.AreEqual(0,half.filledStars[2].fillAmount);
                foreach(var size in new[]{new Vector2Int(1080,1920),new Vector2Int(390,844),new Vector2Int(320,568)})
                {
                    app.stageScroll.verticalNormalizedPosition=1;
                    yield return Capture(language+"-"+size.x+"-lobby",app,size.x,size.y,true,false,()=>
                    {
                        var corners=new Vector3[4];app.multiplayer.GetComponent<RectTransform>().GetWorldCorners(corners);
                        float bottom=RectTransformUtility.WorldToScreenPoint(app.GetComponent<Canvas>().worldCamera,corners[0]).y;
                        Assert.GreaterOrEqual(bottom,34+216*size.x/1080f-.1f,"Footer must reserve the same added gap as the header.");
                        Assert.Greater(app.stageScroll.viewport.rect.height,app.cards[0].GetComponent<RectTransform>().rect.height,"One complete card row must remain visible.");
                    },directory:directory);
                    app.stageScroll.verticalNormalizedPosition=0;
                    yield return Capture(language+"-"+size.x+"-lobby-end",app,size.x,size.y,true,validate:()=>
                    {
                        var corners=new Vector3[4];app.cards[29].GetComponent<RectTransform>().GetWorldCorners(corners);
                        foreach(var corner in corners)
                            Assert.IsTrue(app.stageScroll.viewport.rect.Contains(app.stageScroll.viewport.InverseTransformPoint(corner)),"The entire final card must be reachable above the reserved footer.");
                    },directory:directory);
                }
                app.StartStage(2);yield return new WaitForSecondsRealtime(.9f);
                yield return Capture(language+"-390-half-star-match",app,390,844,true,directory:directory,validateImage:image=>
                {
                    var star=app.aiStars.GetComponent<RankStarRating>().filledStars[1];
                    var corners=new Vector3[4];star.rectTransform.GetWorldCorners(corners);
                    var camera=app.GetComponent<Canvas>().worldCamera;
                    var lo=RectTransformUtility.WorldToScreenPoint(camera,corners[0]);
                    var hi=RectTransformUtility.WorldToScreenPoint(camera,corners[2]);
                    float Pixel(float x)=>image.GetPixel(Mathf.RoundToInt(Mathf.Lerp(lo.x,hi.x,x)),Mathf.RoundToInt((lo.y+hi.y)/2)).grayscale;
                    Assert.Less(Pixel(.35f),.35f,"The filled left half must render dark pixels.");
                    Assert.Greater(Pixel(.65f),.65f,"The unfilled right half must render light pixels.");
                });
                app.StartStage(29);yield return new WaitForSecondsRealtime(.9f);
                yield return Capture(language+"-320-final-rank-match",app,320,568,true,validate:()=>
                {
                    Assert.LessOrEqual(app.aiName.preferredHeight,app.aiName.rectTransform.rect.height,"The complete final rank must fit, including both English words.");
                },directory:directory);
                Assert.AreEqual(Data.Text("story_opponent_030"),app.aiName.text);
                foreach(var star in app.aiStars.GetComponent<RankStarRating>().filledStars)Assert.AreEqual(1,star.fillAmount);
            }
        }
    }
}
