using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace StageGame.Tests
{
    public sealed class RatingDisplayAssetTests
    {
        [Test] public void SavedRatingsSliderAndDedicatedTypingClipHaveValidReferences()
        {
            var app=AssetDatabase.LoadAssetAtPath<GameObject>(Editor.RankUiBuilder.AppRootPath).GetComponent<RankGameApp>();
            foreach(var card in app.cards){Assert.NotNull(card.rating);Assert.AreEqual(card.transform,card.rating.transform.parent);}
            Assert.NotNull(app.aiRating);Assert.NotNull(app.turnTimerSlider);Assert.NotNull(app.typingSound);
            Assert.AreEqual(app.boardView,app.turnTimerLabel.transform.parent.parent);
            Assert.IsFalse(app.turnTimerSlider.interactable);Assert.AreEqual(0,app.turnTimerSlider.minValue);Assert.AreEqual(1,app.turnTimerSlider.maxValue);
            Assert.AreEqual(Editor.RankUiBuilder.TypingSoundPath,AssetDatabase.GetAssetPath(app.typingSound));
            Assert.AreNotSame(app.buttonSound,app.typingSound);Assert.AreNotSame(app.moveSound,app.typingSound);
            Assert.AreEqual(.2f,app.typingSound.length,.001f,"AAC-safe copy retains the short click between silent guards.");
            Assert.AreEqual(AudioCompressionFormat.PCM,((AudioImporter)AssetImporter.GetAtPath(Editor.RankUiBuilder.TypingSoundPath)).defaultSampleSettings.compressionFormat);
        }
    }
    public sealed partial class RankFlowTests
    {
        const string RatingEvidence="Evidence/rating-display-speech-20261015";
        [UnityTest] public IEnumerator RatingLabelsFollowEveryStageAndLanguageAndFitAllScreens()
        {
            Progress.Data.unlockedStage=30;Call("OnApplicationFocus",true);
            foreach(string language in new[]{"ko","en"})
            {
                app.SetLanguage(language);app.ShowLobby();
                for(int stage=0;stage<30;stage++)
                {
                    string expected=Data.Strings.Format("ai_rating",Data.Stages[stage].aiRating);
                    Assert.AreEqual(expected,app.cards[stage].rating.text);
                    Set("selected",stage);Call("RefreshDetail");StringAssert.Contains(expected,app.detailNumber.text);
                    app.StartStage(stage);Call("CancelSearch");Assert.AreEqual(expected,app.aiRating.text);
                }
                foreach(var size in new[]{new Vector2Int(1080,1920),new Vector2Int(390,844),new Vector2Int(320,568)})
                {
                    string prefix=language+"-"+size.x;bool host=size.x!=1080;
                    app.ShowLobby();app.stageScroll.verticalNormalizedPosition=1;
                    yield return Capture(prefix+"-rating-lobby",app,size.x,size.y,host,directory:RatingEvidence,validate:()=>
                    {
                        foreach(var card in app.cards){Assert.LessOrEqual(card.rating.preferredWidth,card.rating.rectTransform.rect.width);Assert.LessOrEqual(card.rating.preferredHeight,card.rating.rectTransform.rect.height);}
                    });
                    app.OpenDetail(29);yield return new WaitForSecondsRealtime(.3f);
                    yield return Capture(prefix+"-rating-detail",app,size.x,size.y,host,directory:RatingEvidence,validate:()=>
                    {Assert.LessOrEqual(app.detailNumber.preferredWidth,app.detailNumber.rectTransform.rect.width);});
                    app.StartStage(0);Call("CancelToast");Set("turnSecondsRemaining",15f);Call("RefreshTurnTimer");
                    yield return Capture(prefix+"-rating-match",app,size.x,size.y,host,directory:RatingEvidence,validate:()=>
                    {
                        Assert.LessOrEqual(app.aiRating.preferredHeight,app.aiRating.rectTransform.rect.height);
                        Assert.That(app.turnTimerSlider.value,Is.InRange(.4f,.5f));
                        AssertSliderAboveGrid();if(size.x==1080)AssertMatchFits(app);
                    });
                }
                app.StartStage(27);Call("CancelToast");yield return AiReady();
                Set("turnSecondsRemaining",4f);Call("RefreshTurnTimer");
                yield return Capture(language+"-1080-rating-long-name-warning",app,1080,1920,false,directory:RatingEvidence,validate:()=>
                {AssertSliderAboveGrid();AssertMatchFits(app);});
            }
        }
        void AssertSliderAboveGrid()
        {
            var corners=new Vector3[4];app.turnTimerLabel.rectTransform.GetWorldCorners(corners);
            float labelTop=app.boardView.InverseTransformPoint(corners[2]).y,labelBottom=app.boardView.InverseTransformPoint(corners[0]).y;
            ((RectTransform)app.turnTimerSlider.transform).GetWorldCorners(corners);
            float barTop=app.boardView.InverseTransformPoint(corners[2]).y,barBottom=app.boardView.InverseTransformPoint(corners[0]).y;
            float gridTop=((RectTransform)app.boardView.Find("Horizontal_5")).anchoredPosition.y;
            Assert.Greater(labelBottom,barTop);Assert.Greater(barBottom,gridTop+RankUiMetrics.U(8));
            Assert.Less(labelTop,app.boardView.rect.yMax-RankUiMetrics.U(20));
            Assert.Less(app.turnTimerLabel.fontSize,48);
            Assert.IsNull(app.matchScroll.content.Find("TurnTimer"),"Old header timer must have moved into the board.");
        }
        [UnityTest] public IEnumerator SliderTracksPausedCountdownAndCannotChangeTimeByInput()
        {
            Call("OnApplicationFocus",true);app.StartStage(0);Call("RefreshTurnTimer");Assert.AreEqual(1,app.turnTimerSlider.value);
            AdvanceTimer(15);Call("RefreshTurnTimer");Assert.AreEqual(.5f,app.turnTimerSlider.value,.001f);
            app.pause.onClick.Invoke();AdvanceTimer(60);Call("RefreshTurnTimer");Assert.AreEqual(.5f,app.turnTimerSlider.value,.001f);
            Assert.IsFalse(app.turnTimerSlider.interactable);
            app.turnTimerSlider.value=1;Assert.AreEqual(15,Remaining,"A slider event must never control the countdown.");
            Call("RefreshTurnTimer");Assert.AreEqual(.5f,app.turnTimerSlider.value,.001f);app.confirmResume.onClick.Invoke();
            AdvanceTimer(11);Call("RefreshTurnTimer");Assert.AreEqual(4f/30,app.turnTimerSlider.value,.001f);
            Assert.AreEqual(app.turnTimerLabel.color,app.turnTimerSlider.fillRect.GetComponent<Image>().color);
            app.SelectColumn(2);AdvanceTimer(4);Assert.AreEqual(1,Board.MoveCount);Assert.IsFalse(app.turnTimerSlider.gameObject.activeInHierarchy);
            yield return AiReady();Call("RefreshTurnTimer");Assert.That(app.turnTimerSlider.value,Is.InRange(.96f,1f));
        }
        [Serializable] sealed class TypingMixReport
        {
            public AudioSignal oldTyping=new AudioSignal(),newTyping=new AudioSignal(),musicOnly=new AudioSignal(),musicAndTyping=new AudioSignal(),allSounds=new AudioSignal();
        }
        IEnumerator OldTypingReference()
        {
            for(int n=0;n<15;n++){app.audioSource.PlayOneShot(app.buttonSound,.18f);yield return new WaitForSecondsRealtime(.04f);}
        }
        [UnityTest] public IEnumerator DedicatedTypingIsStrongerAndAudibleWithMusicWithoutClipping()
        {
            Call("OnApplicationFocus",true);app.ShowLobby();Progress.Data.musicMuted=true;Call("RefreshMusic");app.audioSource.Stop();
            yield return new WaitForSecondsRealtime(.15f);var report=new TypingMixReport();
            var old=app.StartCoroutine(OldTypingReference());yield return MeasureListenerOutput(report.oldTyping,.6f);app.StopCoroutine(old);app.audioSource.Stop();
            yield return new WaitForSecondsRealtime(.1f);app.StartStage(0);yield return MeasureListenerOutput(report.newTyping,.6f);
            Assert.Greater(report.newTyping.rms,report.oldTyping.rms*4);Assert.Greater(report.newTyping.peak,.15f);
            Call("CancelSpeech");yield return new WaitForSecondsRealtime(.1f);
            Progress.Data.musicMuted=false;Call("RefreshMusic");app.musicSource.time=2;yield return MeasureListenerOutput(report.musicOnly,.4f);
            app.StartStage(0);app.musicSource.time=2;yield return MeasureListenerOutput(report.musicAndTyping,.4f);
            Assert.IsTrue(app.musicSource.isPlaying);Assert.Greater(report.musicAndTyping.rms,report.musicOnly.rms*1.5f);
            app.SelectColumn(3);app.PlaceSelected();yield return MeasureListenerOutput(report.allSounds,.3f);Assert.Less(report.allSounds.peak,.99f);
            Directory.CreateDirectory(RatingEvidence);File.WriteAllText(RatingEvidence+"/typing-listener-output.json",JsonUtility.ToJson(report,true));
        }
    }
}
