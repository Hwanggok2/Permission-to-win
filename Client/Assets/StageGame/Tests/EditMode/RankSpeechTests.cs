using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace StageGame.Tests
{
    public sealed partial class RankFlowTests
    {
        IEnumerator SpeechComplete()
        {
            // Batch editors do not own desktop focus; simulate an active game for visual assertions.
            Call("OnApplicationFocus",true);
            float until=Time.realtimeSinceStartup+8;
            while(Field<int[]>("speechOffsets")!=null &&
                  Field<int>("speechVisible")<Field<int[]>("speechOffsets").Length &&
                  Time.realtimeSinceStartup<until)yield return null;
            Assert.AreEqual(Field<string>("speechText"),app.aiSpeech.text,"The complete localized line must remain unchanged.");
        }

        [UnityTest] public IEnumerator SpeechTypesBothLanguagesWithClicksAndStableWrapping()
        {
            Call("OnApplicationFocus",true);Progress.Data.musicMuted=true;Call("RefreshMusic");
            Assert.IsTrue(app.aiSpeech.supportRichText);
            foreach(string language in new[]{"ko","en"})
            {
                app.SetLanguage(language);app.StartStage(0);Call("CancelToast");app.audioSource.Stop();
                string expected=Data.Text(Data.Stages[0].aiIntroKey);
                Assert.AreEqual(0,Field<int>("speechVisible"));Assert.AreNotEqual(expected,app.aiSpeech.text);
                float height=app.aiSpeech.preferredHeight;
                var signal=new AudioSignal();yield return MeasureListenerOutput(signal,.2f);
                Assert.Greater(signal.peak,.01f,"Revealed letters must produce audible typing clicks.");
                int visible=Field<int>("speechVisible");Assert.That(visible,Is.InRange(1,6));
                StringAssert.StartsWith(expected.Substring(0,visible),app.aiSpeech.text);
                Assert.AreEqual(height,app.aiSpeech.preferredHeight,.1f);
                Call("RefreshMatchHeader");Assert.AreEqual(visible,Field<int>("speechVisible"),"A refresh must not restart a line.");
                yield return Capture(language+"-typing",app,390,844,true,directory:"Evidence/speech-20261015");
                yield return SpeechComplete();Assert.AreEqual(expected,app.aiSpeech.text);
                Assert.AreEqual(height,app.aiSpeech.preferredHeight,.1f);
                yield return Capture(language+"-complete",app,390,844,true,directory:"Evidence/speech-20261015");
            }
            Call("SetSpeech","가e\u0301🙂나");Assert.AreEqual(4,Field<int[]>("speechOffsets").Length);
            yield return SpeechComplete();Assert.AreEqual("가e\u0301🙂나",app.aiSpeech.text);
        }

        [UnityTest] public IEnumerator SpeechPausesWithDialogsFocusBackgroundAndAdsWhileMusicCanContinue()
        {
            Call("OnApplicationFocus",true);Progress.Data.musicMuted=true;Call("RefreshMusic");
            Action[] suspend={
                ()=>app.pause.onClick.Invoke(),()=>app.matchRetry.onClick.Invoke(),
                ()=>Call("OnApplicationFocus",false),()=>Call("OnApplicationPause",true),
                ()=>Set("interstitialPending",true)};
            Action[] resume={
                ()=>app.confirmResume.onClick.Invoke(),()=>app.actionCancel.onClick.Invoke(),
                ()=>Call("OnApplicationFocus",true),()=>Call("OnApplicationPause",false),
                ()=>Set("interstitialPending",false)};
            for(int i=0;i<suspend.Length;i++)
            {
                app.StartStage(0);yield return new WaitForSecondsRealtime(.16f);suspend[i]();
                int visible=Field<int>("speechVisible");yield return new WaitForSecondsRealtime(.12f);
                var quiet=new AudioSignal();yield return MeasureListenerOutput(quiet,.12f);
                Assert.AreEqual(visible,Field<int>("speechVisible"),"Typing advanced while suspended: "+i);
                Assert.Less(quiet.peak,.001f,"Typing click leaked while suspended: "+i);
                resume[i]();yield return new WaitForSecondsRealtime(.12f);Assert.Greater(Field<int>("speechVisible"),visible);
            }
            Progress.Data.musicMuted=false;Call("RefreshMusic");app.pause.onClick.Invoke();
            float at=app.musicSource.time;int paused=Field<int>("speechVisible");
            yield return new WaitForSecondsRealtime(.16f);
            Assert.IsTrue(app.musicSource.isPlaying);Assert.Greater(app.musicSource.time,at);
            Assert.AreEqual(paused,Field<int>("speechVisible"));
        }

        [UnityTest] public IEnumerator SpeechMuteHidesNoLettersAndScreenChangesCancelTheOldLine()
        {
            Call("OnApplicationFocus",true);Progress.Data.musicMuted=true;Progress.Data.soundEnabled=false;
            app.StartStage(0);app.audioSource.Stop();var quiet=new AudioSignal();
            yield return MeasureListenerOutput(quiet,.2f);Assert.Less(quiet.peak,.001f);
            Assert.Greater(Field<int>("speechVisible"),0);
            app.SetLanguage("en");Assert.AreEqual(0,Field<int>("speechVisible"));
            Assert.AreEqual(Data.Text(Data.Stages[0].aiIntroKey),Field<string>("speechText"));
            yield return SpeechComplete();
            app.StartStage(0);yield return new WaitForSecondsRealtime(.12f);app.ShowLobby();
            Assert.IsNull(Field<string>("speechText"));string hidden=app.aiSpeech.text;
            Progress.Data.soundEnabled=true;yield return new WaitForSecondsRealtime(.12f);
            quiet=new AudioSignal();yield return MeasureListenerOutput(quiet,.2f);
            Assert.Less(quiet.peak,.001f);Assert.AreEqual(hidden,app.aiSpeech.text);
            app.StartStage(0);Assert.AreEqual(0,Field<int>("speechVisible"));
            yield return new WaitForSecondsRealtime(.12f);app.gameObject.SetActive(false);
            Assert.IsNull(Field<string>("speechText"));
        }

        [UnityTest] public IEnumerator SpeechTypesWinAndLossDialogueAndRestartsForTheNextStage()
        {
            Call("OnApplicationFocus",true);Progress.Data.musicMuted=true;Progress.Data.soundEnabled=false;
            Progress.Data.unlockedStage=2;
            foreach(int side in new[]{1,-1})
            {
                app.StartStage(0);Inject(Replay(side,0,6,1,6,2,5,3));Call("Finish");yield return ResultReady();
                string expected=Data.Text(side==1?Data.Stages[0].aiLoseKey:Data.Stages[0].aiWinKey);
                Assert.AreEqual(expected,Field<string>("speechText"));
                Assert.Less(Field<int>("speechVisible"),Field<int[]>("speechOffsets").Length);
                yield return SpeechComplete();Assert.AreEqual(expected,app.aiSpeech.text);
            }
            app.StartStage(1);Assert.AreEqual(0,Field<int>("speechVisible"));
            Assert.AreEqual(Data.Text(Data.Stages[1].aiIntroKey),Field<string>("speechText"));
            yield return SpeechComplete();
        }
    }
}
