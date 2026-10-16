using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace StageGame.Tests
{
    public sealed class RankMusicAssetTests
    {
        [Test] public void LegacyMusicPreferenceDefaultsOnAndRoundTripsIndependently()
        {
            const string key="stagegame.progress.v1";
            bool exists=PlayerPrefs.HasKey(key);string saved=PlayerPrefs.GetString(key);
            try
            {
                PlayerPrefs.SetString(key,"{\"saveVersion\":1,\"unlockedStage\":7,\"completedMatches\":9,\"soundEnabled\":false}");
                var store=new ProgressStore(30);
                Assert.IsFalse(store.Data.musicMuted);Assert.IsFalse(store.Data.soundEnabled);
                store.Data.musicMuted=true;Assert.IsTrue(store.Save());
                store=new ProgressStore(30);
                Assert.IsTrue(store.Data.musicMuted);Assert.IsFalse(store.Data.soundEnabled);
                Assert.AreEqual(7,store.Data.unlockedStage);Assert.AreEqual(9,store.Data.completedMatches);
                store.Data.soundEnabled=true;Assert.IsTrue(store.Save());
                Assert.IsTrue(new ProgressStore(30).Data.musicMuted);
            }
            finally { if(exists)PlayerPrefs.SetString(key,saved);else PlayerPrefs.DeleteKey(key);PlayerPrefs.Save(); }
        }

        [Test] public void SavedMusicReferencesOriginalAudioAndAllLicenseNoticesArePresent()
        {
            var app=AssetDatabase.LoadAssetAtPath<GameObject>(Editor.RankUiBuilder.AppRootPath).GetComponent<RankGameApp>();
            Assert.NotNull(app.musicSource);Assert.NotNull(app.musicToggle);Assert.NotNull(app.musicLabel);
            Assert.AreNotSame(app.audioSource,app.musicSource);
            Assert.AreEqual(2,app.GetComponentsInChildren<AudioSource>(true).Length);
            Assert.AreEqual(Editor.RankUiBuilder.MusicPath,AssetDatabase.GetAssetPath(app.musicSource.clip));
            Assert.IsFalse(app.musicSource.playOnAwake);Assert.IsTrue(app.musicSource.loop);
            Assert.AreEqual(.30f,app.musicSource.volume,.0001f);Assert.AreEqual(0,app.musicSource.spatialBlend);
            Assert.Greater(app.musicSource.clip.length,30);
            var importer=(AudioImporter)AssetImporter.GetAtPath(Editor.RankUiBuilder.MusicPath);
            Assert.AreEqual(AudioClipLoadType.CompressedInMemory,importer.defaultSampleSettings.loadType);
            using(var sha=SHA256.Create())
                CollectionAssert.AreEqual(sha.ComputeHash(File.ReadAllBytes("../Overview/Audio/originals/easy-lemon.mp3")),sha.ComputeHash(File.ReadAllBytes(Editor.RankUiBuilder.MusicPath)));
            string combined=Resources.Load<TextAsset>("LegalNotices").text;
            foreach(string path in Directory.GetFiles("Assets/StreamingAssets/Notices","*.txt"))
            {
                string name=Path.GetFileName(path);
                if(new[]{"SOURCES.txt","NotoSansKR-OFL.txt"}.Contains(name))continue;
                string notice=File.ReadAllText(path);
                if(name.EndsWith("-OFL.txt",StringComparison.Ordinal))
                {
                    int preamble=notice.IndexOf("PREAMBLE",StringComparison.Ordinal);
                    Assert.Greater(preamble,0,path);
                    StringAssert.Contains(notice.Substring(0,preamble),combined,path+" copyright and RFN");
                    StringAssert.Contains(new string(notice.Substring(preamble).Where(c=>!char.IsWhiteSpace(c)).ToArray()),
                        new string(combined.Where(c=>!char.IsWhiteSpace(c)).ToArray()),path+" complete shared OFL terms");
                }
                else StringAssert.Contains(notice,combined,path);
            }
            StringAssert.Contains("Kevin MacLeod",combined);StringAssert.Contains("USUAN1200076",combined);
            StringAssert.Contains("https://creativecommons.org/licenses/by/4.0/",combined);
            StringAssert.Contains("Original MP3 unchanged",combined);
        }
    }

    public sealed partial class RankFlowTests
    {
        [Serializable] sealed class AudioSignal
        {
            public float peak, rms;
            public int sampledWindows;
        }
        [Serializable] sealed class AudioOutputReport
        {
            public AudioSignal musicBefore=new AudioSignal(), musicAfter=new AudioSignal();
            public AudioSignal moveBefore=new AudioSignal(), moveAfter=new AudioSignal(), muted=new AudioSignal();
        }
        IEnumerator MeasureListenerOutput(AudioSignal signal,float duration)
        {
            var samples=new float[1024];double squares=0;int count=0;
            float until=Time.realtimeSinceStartup+duration;
            while(Time.realtimeSinceStartup<until)
            {
                AudioListener.GetOutputData(samples,0);signal.sampledWindows++;
                foreach(float value in samples){signal.peak=Mathf.Max(signal.peak,Mathf.Abs(value));squares+=value*value;count++;}
                yield return null;
            }
            signal.rms=(float)Math.Sqrt(squares/count);
        }
        [UnityTest] public IEnumerator MusicAndStonePlacementProduceStrongerNonClippingListenerOutput()
        {
            var report=new AudioOutputReport();Call("OnApplicationFocus",true);
            yield return SpeechComplete();
            Assert.IsFalse(AudioListener.pause);Assert.AreEqual(1,AudioListener.volume,.001f);
            app.musicSource.volume=.12f;app.musicSource.time=2;yield return new WaitForSecondsRealtime(.1f);
            yield return MeasureListenerOutput(report.musicBefore,.6f);
            app.musicSource.volume=.30f;app.musicSource.time=2;yield return new WaitForSecondsRealtime(.1f);
            yield return MeasureListenerOutput(report.musicAfter,.6f);
            Progress.Data.musicMuted=true;Call("RefreshMusic");yield return new WaitForSecondsRealtime(.2f);
            app.audioSource.PlayOneShot(app.moveSound,.3f);
            yield return MeasureListenerOutput(report.moveBefore,.25f);
            app.SelectColumn(3);app.PlaceSelected();
            yield return MeasureListenerOutput(report.moveAfter,.25f);
            app.StartStage(0);Progress.Data.soundEnabled=false;yield return new WaitForSecondsRealtime(.2f);
            app.SelectColumn(3);app.PlaceSelected();yield return MeasureListenerOutput(report.muted,.25f);
            Directory.CreateDirectory("Evidence/audio-levels-20261015");
            File.WriteAllText("Evidence/audio-levels-20261015/listener-output.json",JsonUtility.ToJson(report,true));
            Assert.Greater(report.musicBefore.rms,.0001f,"Music must reach the final audio listener, not only report isPlaying.");
            Assert.Greater(report.musicAfter.rms,report.musicBefore.rms*1.8f);
            Assert.Greater(report.moveBefore.peak,.001f);
            Assert.Greater(report.moveAfter.peak,report.moveBefore.peak*3f);
            Assert.Greater(report.moveAfter.peak,.08f);
            Assert.Less(report.musicAfter.peak+report.moveAfter.peak,.99f,"Combined headroom must avoid clipping.");
            Assert.Less(report.muted.peak,.001f,"User OFF settings must still mute actual output.");
        }

        [UnityTest] public IEnumerator MusicSettingRestoresAfterReloadAndBothLanguagesFitSavedSettings()
        {
            Call("OnApplicationFocus",true);yield return null;
            Assert.IsTrue(app.musicSource.isPlaying);Assert.IsFalse(Progress.Data.musicMuted);
            app.ShowLobby();Assert.IsTrue(app.musicSource.isPlaying);
            app.lobbySettings.onClick.Invoke();app.soundToggle.onClick.Invoke();app.musicToggle.onClick.Invoke();
            Assert.IsFalse(Progress.Data.soundEnabled);Assert.IsTrue(Progress.Data.musicMuted);
            SceneManager.LoadScene("Main");yield return null;yield return null;
            app=UnityEngine.Object.FindFirstObjectByType<RankGameApp>();Call("OnApplicationFocus",true);
            Assert.IsTrue(Progress.Data.musicMuted);Assert.IsFalse(Progress.Data.soundEnabled);
            Assert.IsTrue(app.lobby.activeSelf);Assert.IsFalse(app.musicSource.isPlaying);
            app.StartStage(0);yield return null;Assert.IsFalse(app.musicSource.isPlaying);
            Call("OnApplicationPause",true);Call("OnApplicationPause",false);
            Call("OnApplicationFocus",false);Call("OnApplicationFocus",true);yield return null;
            Assert.IsFalse(app.musicSource.isPlaying);
            app.musicToggle.onClick.Invoke();yield return null;
            Assert.IsTrue(app.musicSource.isPlaying);Assert.IsFalse(Progress.Data.soundEnabled);
            app.ShowLobby();app.lobbySettings.onClick.Invoke();
            foreach(string language in new[]{"ko","en"})
            {
                app.SetLanguage(language);
                foreach(bool muted in new[]{false,true})
                {
                    if(Progress.Data.musicMuted!=muted)app.musicToggle.onClick.Invoke();
                    Assert.AreEqual(Data.Text(muted?"settings_music_off":"settings_music_on"),app.musicLabel.text);
                    foreach(var size in new[]{new Vector2Int(1080,1920),new Vector2Int(390,844),new Vector2Int(320,568)})
                        yield return Capture(language+"-"+size.x+(muted?"-music-off":"-music-on"),app,size.x,size.y,true,directory:"Evidence/easy-lemon-20261015");
                }
            }
            app.noticesOpen.onClick.Invoke();
            StringAssert.Contains("Kevin MacLeod",app.noticesBody.text);
            yield return Capture("en-1080-music-license",app,1080,1920,false,directory:"Evidence/easy-lemon-20261015");
        }

        [UnityTest] public IEnumerator MusicContinuesThroughPauseAndConfirmationButSuspendsInBackground()
        {
            Call("OnApplicationFocus",true);yield return new WaitForSecondsRealtime(.15f);
            Assert.IsTrue(app.musicSource.isPlaying);app.musicSource.time=2;
            app.pause.onClick.Invoke();Assert.IsTrue(app.confirm.activeSelf);Assert.IsTrue(app.musicSource.isPlaying);
            float modalAt=app.musicSource.time;yield return new WaitForSecondsRealtime(.15f);
            Assert.Greater(app.musicSource.time,modalAt);
            app.confirmLeave.onClick.Invoke();Assert.IsTrue(app.actionConfirm.activeSelf);Assert.IsTrue(app.musicSource.isPlaying);
            modalAt=app.musicSource.time;yield return new WaitForSecondsRealtime(.15f);Assert.Greater(app.musicSource.time,modalAt);
            app.actionCancel.onClick.Invoke();Assert.IsTrue(app.confirm.activeSelf);Assert.IsTrue(app.musicSource.isPlaying);
            Call("OnApplicationFocus",false);float pausedAt=app.musicSource.time;
            app.confirmResume.onClick.Invoke();Assert.IsFalse(app.musicSource.isPlaying);
            yield return new WaitForSecondsRealtime(.15f);Assert.AreEqual(pausedAt,app.musicSource.time,.05f);
            Call("OnApplicationPause",true);Call("OnApplicationFocus",true);Assert.IsFalse(app.musicSource.isPlaying);
            Call("OnApplicationPause",false);yield return new WaitForSecondsRealtime(.2f);
            Assert.IsTrue(app.musicSource.isPlaying);Assert.Greater(app.musicSource.time,pausedAt);
            app.matchRetry.onClick.Invoke();Assert.IsTrue(app.actionConfirm.activeSelf);Assert.IsTrue(app.musicSource.isPlaying);
            modalAt=app.musicSource.time;yield return new WaitForSecondsRealtime(.15f);Assert.Greater(app.musicSource.time,modalAt);
            app.actionCancel.onClick.Invoke();Assert.IsTrue(app.musicSource.isPlaying);
            app.pause.onClick.Invoke();app.musicToggle.onClick.Invoke();app.confirmResume.onClick.Invoke();
            Assert.IsTrue(Progress.Data.musicMuted);Assert.IsFalse(app.musicSource.isPlaying);
            app.musicToggle.onClick.Invoke();Assert.IsTrue(app.musicSource.isPlaying);
            app.ShowLobby();Assert.IsTrue(app.musicSource.isPlaying);
            app.StartStage(0);yield return null;Assert.IsTrue(app.musicSource.isPlaying);
            app.enabled=false;Assert.IsFalse(app.musicSource.isPlaying);app.enabled=true;
        }

        [UnityTest] public IEnumerator MusicKeepsItsPositionAcrossLobbyMatchAndResultReturn()
        {
            Call("OnApplicationFocus",true);yield return new WaitForSecondsRealtime(.1f);
            var source=app.musicSource;var clip=source.clip;source.time=12;
            yield return new WaitForSecondsRealtime(.1f);
            for(int transition=0;transition<3;transition++)
            {
                float before=source.time;app.ShowLobby();
                Assert.AreSame(source,app.musicSource);Assert.AreSame(clip,source.clip);
                Assert.IsTrue(source.isPlaying);Assert.GreaterOrEqual(source.time,before-.05f,"Returning to lobby must not restart the track.");
                app.lobbySettings.onClick.Invoke();yield return new WaitForSecondsRealtime(.1f);
                Assert.Greater(source.time,before,"Music must continue advancing in lobby/settings.");
                before=source.time;app.StartStage(0);
                Assert.IsTrue(source.isPlaying);Assert.GreaterOrEqual(source.time,before-.05f,"Entering a match must keep the current position.");
                yield return new WaitForSecondsRealtime(.1f);Assert.Greater(source.time,before);
            }
            Inject(Replay(1,0,6,1,6,2,5));app.SelectColumn(3);app.PlaceSelected();yield return ResultReady();
            Assert.IsTrue(source.isPlaying);float resultAt=source.time;
            app.resultLobby.onClick.Invoke();Assert.IsTrue(source.isPlaying);app.actionAccept.onClick.Invoke();
            Assert.IsTrue(app.lobby.activeSelf);Assert.IsTrue(source.isPlaying);
            Assert.GreaterOrEqual(source.time,resultAt-.05f,"Confirmed result-to-lobby return must resume, not restart.");
            app.musicToggle.onClick.Invoke();Assert.IsFalse(source.isPlaying);
            app.StartStage(0);app.ShowLobby();yield return null;Assert.IsFalse(source.isPlaying);
            app.musicToggle.onClick.Invoke();yield return null;Assert.IsTrue(source.isPlaying);
            Call("OnApplicationPause",true);Assert.IsFalse(source.isPlaying);
            Call("OnApplicationPause",false);Assert.IsTrue(source.isPlaying);
        }

        [UnityTest] public IEnumerator MusicWaitsForRewardAdsAndResumesOnFailureCancellationOrTimeout()
        {
            Call("OnApplicationFocus",true);
            foreach(AdResult outcome in Enum.GetValues(typeof(AdResult)))
            {
                app.StartStage(0);var ads=new PendingAds();app.SetAdsForTests(ads);yield return null;
                Assert.IsTrue(app.musicSource.isPlaying);app.hint.onClick.Invoke();Assert.IsFalse(app.musicSource.isPlaying);
                ads.Reward(outcome);yield return null;Assert.IsTrue(app.musicSource.isPlaying);
            }
            app.StartStage(0);var pending=new PendingAds();app.SetAdsForTests(pending);app.hint.onClick.Invoke();
            Call("OnApplicationFocus",false);pending.Reward(AdResult.Failed);yield return null;Assert.IsFalse(app.musicSource.isPlaying);
            Call("OnApplicationFocus",true);Assert.IsTrue(app.musicSource.isPlaying);
            app.hint.onClick.Invoke();Assert.IsFalse(app.musicSource.isPlaying);
            float until=Time.realtimeSinceStartup+7;
            while(Field<RewardGate>("rewardGate").Pending&&Time.realtimeSinceStartup<until)yield return null;
            yield return null;Assert.IsFalse(Field<RewardGate>("rewardGate").Pending);Assert.IsTrue(app.musicSource.isPlaying);
            app.hint.onClick.Invoke();app.musicToggle.onClick.Invoke();Call("OnApplicationPause",true);
            Call("OnApplicationPause",false);pending.Reward(AdResult.Completed);yield return null;
            Assert.IsTrue(Progress.Data.musicMuted);Assert.IsFalse(app.musicSource.isPlaying);
            app.musicToggle.onClick.Invoke();app.SetAdsForTests(new NoFillAds());app.hint.onClick.Invoke();yield return null;
            Assert.IsTrue(app.musicSource.isPlaying);
        }

        [UnityTest] public IEnumerator MusicWaitsForInterstitialAndLateCallbacksCannotOverrideMusicPreference()
        {
            Call("OnApplicationFocus",true);
            foreach(bool timeout in new[]{false,true})
            {
                if(Progress.Data.musicMuted)app.musicToggle.onClick.Invoke();
                app.StartStage(0);Progress.Data.completedMatches=4;Progress.Data.lastInterstitialMatch=0;Progress.Data.lastInterstitialUtc=0;
                var ads=new PendingAds();app.SetAdsForTests(ads);Inject(Replay(1,0,6,1,6,2,5));
                app.SelectColumn(3);app.PlaceSelected();float until=Time.realtimeSinceStartup+3;
                while(ads.Interstitials==0&&Time.realtimeSinceStartup<until)yield return null;
                Assert.AreEqual(1,ads.Interstitials);Assert.IsFalse(app.musicSource.isPlaying);
                if(!timeout){app.musicToggle.onClick.Invoke();ads.Interstitial(AdResult.Completed);}
                yield return ResultReady();yield return null;
                Assert.AreEqual(timeout,app.musicSource.isPlaying);
                app.ShowLobby();float lobbyAt=app.musicSource.time;
                ads.Interstitial(AdResult.Completed);yield return null;Assert.AreEqual(timeout,app.musicSource.isPlaying);
                if(timeout)Assert.GreaterOrEqual(app.musicSource.time,lobbyAt-.05f);
            }
        }
    }
}
