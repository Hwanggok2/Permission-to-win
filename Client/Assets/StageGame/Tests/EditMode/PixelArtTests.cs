using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace StageGame.Tests
{
    public sealed partial class RankFlowTests
    {
        [UnityTest] public IEnumerator PixelPortraitsFollowCharacterAndMatchState()
        {
            Progress.Data.unlockedStage=30;
            Assert.AreEqual(19,app.theme.portraits.Length);
            Assert.AreEqual(19,app.theme.portraits.Select(p=>p.characterId).Distinct().Count());
            StringAssert.EndsWith("C06-pixel-v5.png",AssetDatabase.GetAssetPath(app.theme.Portrait("C06").intro));
            Assert.AreNotSame(app.theme.Portrait("C01").intro,app.theme.Portrait("C01",true).intro,"Trainee must have no rank; final private has one stripe.");
            Assert.AreSame(app.match.transform,app.turnToast.transform.parent,"Placement guide stays at the screen center independently of board scrolling.");
            for(int i=0;i<30;i++)
            {
                app.StartStage(i);Call("CancelSearch");Set("busy",false);
                Set("board",new ConnectFourBoard());Set("characterThinking",false);Call("RefreshMatchHeader");
                var portrait=app.theme.Portrait(Data.Stages[i].characterId,Data.Stages[i].id>=25);
                Assert.NotNull(portrait);Assert.NotNull(portrait.intro);Assert.NotNull(portrait.thinking);Assert.NotNull(portrait.defeat);
                Assert.AreEqual(portrait.intro,app.aiFace.sprite);Assert.IsFalse(app.rankHat.gameObject.activeSelf);
                Set("characterThinking",true);Call("RefreshBoard");Assert.AreEqual(portrait.thinking,app.aiFace.sprite);
                var win=new ConnectFourBoard();foreach(int col in new[]{0,6,1,6,2,5,3})win.TryDrop(col);
                Set("board",win);Call("RefreshBoard");Assert.AreEqual(portrait.defeat,app.aiFace.sprite);
                var loss=new ConnectFourBoard(-1);foreach(int col in new[]{0,6,1,6,2,5,3})loss.TryDrop(col);
                Set("board",loss);Call("RefreshBoard");Assert.AreEqual(portrait.thinking,app.aiFace.sprite,"AI victory must not use defeat art.");
            }
            foreach(var p in app.theme.portraits)
            {
                var path=AssetDatabase.GetAssetPath(p.intro);
                int number=int.Parse(p.characterId.Substring(1));
                string expected=p.characterId+(number==1?"-uniform-v4.png":number==2?"-occlusion-v7.png":number<=4?"-pixel-v6.png":"-pixel-v5.png");
                foreach(var sprite in new[]{p.intro,p.thinking,p.defeat})
                    StringAssert.EndsWith(expected,AssetDatabase.GetAssetPath(sprite),"Every character/state must use the selected revision.");
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);
                Assert.AreEqual(FilterMode.Point,importer.filterMode);Assert.IsFalse(importer.mipmapEnabled);
                Assert.AreEqual(TextureImporterCompression.Uncompressed,importer.textureCompression);
                Assert.AreEqual(p.intro.rect.size,p.thinking.rect.size);Assert.AreEqual(p.intro.rect.size,p.defeat.rect.size);
            }
            yield return null;
        }

        [UnityTest] public IEnumerator CaptureOccludedChestPortrait()
        {
            const string directory="Evidence/portrait-occlusion-20261018/screens";
            Assert.AreNotEqual(UnityEngine.Rendering.GraphicsDeviceType.Null,SystemInfo.graphicsDeviceType);
            Progress.Data.unlockedStage=30;app.SetLanguage("ko");
            foreach(int stage in new[]{1,24})
            {
                app.StartStage(stage);Call("CancelSearch");Set("busy",false);
                yield return new WaitForSecondsRealtime(.9f);
                var portrait=app.theme.Portrait("C02");
                StringAssert.EndsWith("C02-occlusion-v7.png",AssetDatabase.GetAssetPath(portrait.thinking));
                foreach(string state in new[]{"intro","thinking","defeat"})
                {
                    Set("board",new ConnectFourBoard());Set("characterThinking",state=="thinking");
                    if(state=="defeat")
                    {
                        var win=new ConnectFourBoard();foreach(int col in new[]{0,6,1,6,2,5,3})win.TryDrop(col);
                        Set("board",win);
                    }
                    Call("RefreshBoard");
                    Assert.AreEqual(state=="intro"?portrait.intro:state=="thinking"?portrait.thinking:portrait.defeat,app.aiFace.sprite);
                    string name="stage-"+(stage+1).ToString("D2")+"-C02-"+state;
                    yield return Capture(name+"-1080",app,1080,1920,true,directory:directory);
                    yield return Capture(name+"-390",app,390,844,true,directory:directory);
                }
            }
        }

        [UnityTest] public IEnumerator CaptureAllSelectedPixelPortraits()
        {
            const string directory="Evidence/pixel-complete-20261015/screens";
            Assert.AreNotEqual(UnityEngine.Rendering.GraphicsDeviceType.Null,SystemInfo.graphicsDeviceType);
            Progress.Data.unlockedStage=30;app.SetLanguage("ko");
            var stages=Enumerable.Range(0,Data.Stages.Length).GroupBy(i=>Data.Stages[i].characterId).Select(g=>g.First()).Concat(new[]{25,29});
            foreach(int stage in stages)
            {
                var id=Data.Stages[stage].characterId;
                app.StartStage(stage);Call("CancelSearch");Set("busy",false);
                yield return new WaitForSecondsRealtime(.9f);
                var portrait=app.theme.Portrait(id,Data.Stages[stage].id>=25);
                foreach(string state in new[]{"intro","thinking","defeat"})
                {
                    Set("board",new ConnectFourBoard());Set("characterThinking",state=="thinking");
                    if(state=="defeat")
                    {
                        var win=new ConnectFourBoard();foreach(int col in new[]{0,6,1,6,2,5,3})win.TryDrop(col);
                        Set("board",win);
                    }
                    Call("RefreshBoard");
                    Assert.AreEqual(state=="intro"?portrait.intro:state=="thinking"?portrait.thinking:portrait.defeat,app.aiFace.sprite);
                    string name="stage-"+(stage+1).ToString("D2")+"-"+id+"-"+state;
                    yield return Capture(name+"-390",app,390,844,true,directory:directory);
                    if(id=="C03")yield return Capture(name+"-1080",app,1080,1920,true,directory:directory);
                }
            }
        }

        [UnityTest] public IEnumerator CaptureCorrectedPixelPortraitStates()
        {
            const string directory="Evidence/insignia-pixels-20261015/screens";
            Assert.AreNotEqual(UnityEngine.Rendering.GraphicsDeviceType.Null,SystemInfo.graphicsDeviceType,"Visual verification requires a graphics device.");
            Progress.Data.unlockedStage=30;app.SetLanguage("ko");
            foreach(var id in new[]{"C01","C05","C08","C13","C16","C18","C19"})
            {
                int stage=System.Array.FindIndex(Data.Stages,s=>s.characterId==id);
                app.StartStage(stage);Call("CancelSearch");Set("busy",false);
                yield return new WaitForSecondsRealtime(.9f);
                var portrait=app.theme.Portrait(id);
                if(id!="C01")StringAssert.EndsWith(id+"-pixel-v5.png",AssetDatabase.GetAssetPath(portrait.intro));
                foreach(var state in new[]{"intro","thinking","defeat"})
                {
                    Set("board",new ConnectFourBoard());Set("characterThinking",state=="thinking");
                    if(state=="defeat")
                    {
                        var win=new ConnectFourBoard();foreach(int col in new[]{0,6,1,6,2,5,3})win.TryDrop(col);
                        Set("board",win);
                    }
                    Call("RefreshBoard");
                    Assert.AreEqual(state=="intro"?portrait.intro:state=="thinking"?portrait.thinking:portrait.defeat,app.aiFace.sprite);
                    yield return Capture(id+"-"+state+"-1080",app,1080,1920,true,directory:directory);
                }
                yield return Capture(id+"-defeat-390",app,390,844,true,directory:directory);
            }
        }

        [UnityTest] public IEnumerator PixelTournamentInviteAndFinalAreBoundToWins()
        {
            Progress.Data.unlockedStage=30;
            Assert.AreEqual("C18",Data.Stages[23].characterId);
            CollectionAssert.AreEqual(new[]{"C02","C03","C04","C05","C19","C01"},Data.Stages.Skip(24).Select(s=>s.characterId).ToArray());
            foreach(int stage in new[]{23,29})
            {
                app.StartStage(stage);Call("CancelSearch");Set("busy",false);
                var win=new ConnectFourBoard();foreach(int col in new[]{0,6,1,6,2,5,3})win.TryDrop(col);
                Set("board",win);Call("Finish");yield return ResultReady();
                Assert.AreEqual(app.theme.Portrait(Data.Stages[stage].characterId,Data.Stages[stage].id>=25).defeat,app.aiFace.sprite);
                Assert.AreEqual(stage==23,app.nextStage.gameObject.activeSelf);
                if(stage==23) Assert.AreEqual("64강으로",app.nextStage.GetComponentInChildren<UnityEngine.UI.Text>().text);
                else Assert.AreEqual("사단 병사부 4목 대회 우승!",app.resultTitle.text);
            }
        }

        [UnityTest] public IEnumerator CapturePixelArtAndLocalizedLayouts()
        {
            const string directory="Evidence/opening-ranks-20261015/screens";
            Progress.Data.unlockedStage=30;
            foreach(string language in new[]{"ko","en"})
            {
                app.SetLanguage(language);
                foreach(int stage in new[]{0,1,3,4,5,6,10,11,12,17,23,24,27,28,29})
                {
                    app.StartStage(stage);Call("CancelSearch");Set("busy",false);
                    Set("characterThinking",false);Call("RefreshMatchHeader");Call("RefreshBoard");
                    yield return new WaitForSecondsRealtime(.9f);
                    yield return Capture(language+"-stage-"+(stage+1),app,390,844,true,validate:()=>
                    {
                        var faceCorners=new Vector3[4];var speechCorners=new Vector3[4];
                        app.aiFace.rectTransform.GetWorldCorners(faceCorners);app.aiSpeech.rectTransform.GetWorldCorners(speechCorners);
                        Assert.Less(faceCorners[2].x,speechCorners[0].x,"Portrait must not cover dialogue.");
                        var content=app.matchScroll.content;
                        Assert.IsFalse(content.Find("TitlePlate").gameObject.activeSelf);
                        var pauseRect=(RectTransform)app.pause.transform;
                        Assert.AreEqual(app.matchTitle.rectTransform.rect.height,pauseRect.rect.height);
                        var nameCorners=new Vector3[4];var pauseCorners=new Vector3[4];
                        ((RectTransform)content.Find("AiHeader/NamePlate")).GetWorldCorners(nameCorners);pauseRect.GetWorldCorners(pauseCorners);
                        Assert.Less(nameCorners[2].y,pauseCorners[0].y,"Opponent panel must stay below the title/pause row.");
                    },directory:directory);
                }
                yield return Capture(language+"-1080-final",app,1080,1920,true,false,()=>AssertMatchFits(app),directory:directory);
                app.ShowLobby();app.stageScroll.verticalNormalizedPosition=0;
                yield return Capture(language+"-320-lobby-end",app,320,568,true,directory:directory);
                app.StartStage(0);app.SelectColumn(3);
                yield return Capture(language+"-320-preview",app,320,568,true,directory:directory);
                var win=new ConnectFourBoard();foreach(int col in new[]{0,6,1,6,2,5,3})win.TryDrop(col);
                Set("board",win);Call("Finish");yield return ResultReady();
                yield return Capture(language+"-390-defeat",app,390,844,true,directory:directory);
            }
        }
    }
}
