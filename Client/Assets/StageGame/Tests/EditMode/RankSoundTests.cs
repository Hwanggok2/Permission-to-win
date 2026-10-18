using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace StageGame.Tests
{
    public sealed class RankSoundAssetTests
    {
        [Test] public void SavedSoundsUseShortPcmClipsAndCoverEveryButtonOnce()
        {
            var app=AssetDatabase.LoadAssetAtPath<GameObject>(Editor.RankUiBuilder.AppRootPath).GetComponent<RankGameApp>();
            Assert.AreEqual(Editor.RankUiBuilder.StoneSoundPath,AssetDatabase.GetAssetPath(app.moveSound));
            Assert.AreEqual(Editor.RankUiBuilder.ButtonSoundPath,AssetDatabase.GetAssetPath(app.buttonSound));
            Assert.AreNotSame(app.moveSound,app.buttonSound);
            Assert.That(app.moveSound.length,Is.InRange(.2f,.35f));Assert.AreEqual(.2f,app.buttonSound.length,.001f);
            foreach(var clip in new[]{app.moveSound,app.buttonSound})
            {
                var importer=(AudioImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(clip));
                Assert.AreEqual(AudioCompressionFormat.PCM,importer.defaultSampleSettings.compressionFormat);
                var samples=new float[clip.samples*clip.channels];Assert.IsTrue(clip.GetData(samples,0));
                float peak=samples.Max(value=>Mathf.Abs(value));Assert.Greater(peak,.2f);Assert.Less(peak,.7f);
            }
            CollectionAssert.AreEquivalent(app.GetComponentsInChildren<Button>(true),app.soundButtons);
            Assert.AreEqual(app.soundButtons.Length,app.soundButtons.Distinct().Count());
        }
        [TestCase("TypingKey")][TestCase("ButtonClick")]
        public void AacSafeCopiesPreserveTheEntireOriginalAttackBetweenSilentGuards(string name)
        {
            var original=AssetDatabase.LoadAssetAtPath<AudioClip>(Editor.ProjectBuilder.Root+"Audio/"+name+".wav");
            var guarded=AssetDatabase.LoadAssetAtPath<AudioClip>(Editor.ProjectBuilder.Root+"Audio/"+name+"AacSafe.wav");
            Assert.AreEqual(44100,guarded.frequency);Assert.AreEqual(original.channels,guarded.channels);
            Assert.GreaterOrEqual(guarded.samples,2048+original.samples+2048);
            // Compare the source WAV payloads: platform imports can introduce codec/resampler ringing.
            var source=WavePcm(AssetDatabase.GetAssetPath(original));var target=WavePcm(AssetDatabase.GetAssetPath(guarded));
            CollectionAssert.AreEqual(source,target.Skip(2048*2).Take(source.Length));
            Assert.IsTrue(target.Take(2048*2).All(value=>value==0));
            Assert.IsTrue(target.Skip(2048*2+source.Length).All(value=>value==0));
            var imported=new float[guarded.samples];Assert.IsTrue(guarded.GetData(imported,0));
            Assert.Greater(imported.Max(value=>Mathf.Abs(value)),name=="TypingKey"?.49f:.25f,"Imported attack must remain audible.");
        }
        static byte[] WavePcm(string path)
        {
            using(var reader=new BinaryReader(File.OpenRead(path)))
            {
                reader.BaseStream.Position=12;
                while(reader.BaseStream.Position<reader.BaseStream.Length)
                {
                    string chunk=new string(reader.ReadChars(4));int size=reader.ReadInt32();
                    if(chunk=="data")return reader.ReadBytes(size);
                    reader.BaseStream.Position+=size+(size&1);
                }
            }
            throw new InvalidDataException(path);
        }
    }

    public sealed partial class RankFlowTests
    {
        void ClickWithPointer(Button button)
        {
            button.OnPointerClick(new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left});
        }
        [UnityTest] public IEnumerator SoundButtonClickPlaysOnceAcrossOpenCloseAndRespectsDisabledAndOff()
        {
            Call("OnApplicationFocus",true);Progress.Data.musicMuted=true;Call("RefreshMusic");app.ShowLobby();
            yield return new WaitForSecondsRealtime(.15f);
            var reference=new AudioSignal();app.audioSource.PlayOneShot(app.buttonSound,.55f);
            yield return MeasureListenerOutput(reference,.18f);Assert.Greater(reference.peak,.02f);
            var opened=new AudioSignal();ClickWithPointer(app.lobbySettings);Assert.IsTrue(app.settings.activeSelf);
            yield return MeasureListenerOutput(opened,.18f);
            Assert.AreEqual(reference.peak,opened.peak,.01f,"One button activation must produce exactly one click.");
            var closed=new AudioSignal();ClickWithPointer(app.settingsClose);Assert.IsFalse(app.settings.activeSelf);
            yield return MeasureListenerOutput(closed,.18f);Assert.AreEqual(reference.peak,closed.peak,.01f);
            var disabled=new AudioSignal();app.multiplayer.interactable=false;ClickWithPointer(app.multiplayer);
            yield return MeasureListenerOutput(disabled,.18f);Assert.Less(disabled.peak,.001f);
            app.multiplayer.interactable=true;
            ClickWithPointer(app.lobbySettings);ClickWithPointer(app.soundToggle);Assert.IsFalse(Progress.Data.soundEnabled);
            yield return new WaitForSecondsRealtime(.15f);
            var muted=new AudioSignal();ClickWithPointer(app.settingsClose);ClickWithPointer(app.lobbyHelp);
            yield return MeasureListenerOutput(muted,.18f);Assert.Less(muted.peak,.001f);
            ClickWithPointer(app.helpClose);ClickWithPointer(app.lobbySettings);ClickWithPointer(app.soundToggle);
            var enabled=new AudioSignal();yield return MeasureListenerOutput(enabled,.18f);
            Assert.IsTrue(Progress.Data.soundEnabled);Assert.AreEqual(reference.peak,enabled.peak,.01f);
        }
        [UnityTest] public IEnumerator SoundStoneImpactAndUiClickWorkWhileMusicContinuesThroughModal()
        {
            Call("OnApplicationFocus",true);Progress.Data.musicMuted=true;Call("RefreshMusic");
            yield return SpeechComplete();
            yield return new WaitForSecondsRealtime(.15f);
            var click=new AudioSignal();ClickWithPointer(app.columns[3]);
            yield return MeasureListenerOutput(click,.18f);Assert.Greater(click.peak,.02f);Assert.AreEqual(0,Board.MoveCount);
            var impact=new AudioSignal();ClickWithPointer(app.place);
            yield return MeasureListenerOutput(impact,.18f);Assert.AreEqual(1,Board.MoveCount);
            Assert.Greater(impact.peak,click.peak*2);Assert.Less(impact.peak,.9f);
            app.UndoTurn();Progress.Data.soundEnabled=false;yield return new WaitForSecondsRealtime(.15f);
            ClickWithPointer(app.columns[3]);ClickWithPointer(app.place);var muted=new AudioSignal();
            yield return MeasureListenerOutput(muted,.18f);Assert.Less(muted.peak,.001f);
            app.UndoTurn();Progress.Data.musicMuted=false;Call("RefreshMusic");
            ClickWithPointer(app.pause);Assert.IsTrue(app.confirm.activeSelf);Assert.IsTrue(app.musicSource.isPlaying);
            float at=app.musicSource.time;yield return new WaitForSecondsRealtime(.15f);Assert.Greater(app.musicSource.time,at);
            Assert.IsFalse(app.place.interactable,"Gameplay must remain paused even though music continues.");
        }
    }
}
