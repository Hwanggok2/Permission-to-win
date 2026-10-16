using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace StageGame.Tests
{
    public sealed partial class RankFlowTests
    {
        const string Key = "stagegame.progress.v1";
        bool hadSave, hadRecovery;
        string recovery;
        string saved;
        RankGameApp app;
        [UnitySetUp] public IEnumerator Setup()
        {
            hadRecovery=PlayerPrefs.HasKey(Key+".recovery");recovery=PlayerPrefs.GetString(Key+".recovery");
            hadSave=PlayerPrefs.HasKey(Key); saved=PlayerPrefs.GetString(Key); PlayerPrefs.DeleteKey(Key);
            EditorSceneManager.OpenScene(Editor.ProjectBuilder.ScenePath);
            yield return new EnterPlayMode(); yield return null;
            app=UnityEngine.Object.FindFirstObjectByType<RankGameApp>(); Assert.NotNull(app);
            // Gameplay fixtures continue after the first-run rules; dedicated tests cover the panel itself.
            if(app.help.activeSelf)app.helpClose.onClick.Invoke();
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            yield return new ExitPlayMode();
            if(hadSave)PlayerPrefs.SetString(Key,saved);else PlayerPrefs.DeleteKey(Key);
            if(hadRecovery)PlayerPrefs.SetString(Key+".recovery",recovery);else PlayerPrefs.DeleteKey(Key+".recovery");PlayerPrefs.Save();
        }
        T Field<T>(string name)=>(T)typeof(RankGameApp).GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(app);
        void Set(string name,object value)=>typeof(RankGameApp).GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(app,value);
        void Call(string name,params object[] args)=>typeof(RankGameApp).GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(app,args);
        IEnumerator ResultReady()
        {
            float until=Time.realtimeSinceStartup+6;
            while((!app.result.activeSelf||!app.retry.interactable)&&Time.realtimeSinceStartup<until)yield return null;
            Assert.IsTrue(app.result.activeSelf);Assert.IsTrue(app.retry.interactable);
        }
        static void AssertMatchFits(RankGameApp game)
        {
            var viewport=game.matchScroll.viewport;
            Assert.LessOrEqual(game.matchScroll.content.rect.height*game.matchScroll.content.localScale.y,viewport.rect.height+.1f,"The whole match must fit without scrolling at1080x1920.");
            foreach(var button in game.match.GetComponentsInChildren<Button>())
            {
                var corners=new Vector3[4];button.GetComponent<RectTransform>().GetWorldCorners(corners);
                Assert.GreaterOrEqual(viewport.InverseTransformPoint(corners[0]).y,viewport.rect.yMin-.1f,"Clipped bottom: "+button.name);
                Assert.LessOrEqual(viewport.InverseTransformPoint(corners[2]).y,viewport.rect.yMax+.1f,"Clipped top: "+button.name);
            }
        }
        [UnityTest] public IEnumerator PreviewSavedRankScreens()
        {
            Assert.IsTrue(app.match.activeSelf);Assert.AreEqual(0,Field<int>("activeStage"));
            foreach(string language in new[]{"ko","en"})
            {
                app.SetLanguage(language);
                foreach(var size in new[]{new Vector2Int(1080,1920),new Vector2Int(390,844),new Vector2Int(320,568)})
                {
                    string prefix=language+"-"+size.x;
                    app.StartStage(0);yield return new WaitForSecondsRealtime(.9f);yield return Capture(prefix+"-first-match",app,size.x,size.y,true,validate:()=>
                    {
                        if(size.x==1080)AssertMatchFits(app);
                        var rect=(RectTransform)app.pause.transform;
                        foreach(float x in new[]{-.4f,0f,.4f})
                        {
                            var position=rect.TransformPoint(new Vector3(rect.rect.width*x,0,0));
                            var pointer=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(app.GetComponent<Canvas>().worldCamera,position)};
                            var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);
                            Assert.IsNotEmpty(hits);
                            Assert.AreEqual(app.pause.gameObject,ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject),"Transparent pause padding must remain clickable.");
                        }
                    });
                    app.SelectColumn(3);yield return Capture(prefix+"-preview",app,size.x,size.y,true);
                    yield return Capture(prefix+"-match-bottom",app,size.x,size.y,true,true);
                    app.matchRetry.onClick.Invoke();yield return Capture(prefix+"-restart-confirm",app,size.x,size.y,true);app.actionCancel.onClick.Invoke();
                    app.ShowLobby();app.stageScroll.verticalNormalizedPosition=1;yield return Capture(prefix+"-lobby",app,size.x,size.y,true);
                    app.stageScroll.verticalNormalizedPosition=0;Canvas.ForceUpdateCanvases();yield return Capture(prefix+"-lobby-end",app,size.x,size.y,true);
                    app.OpenDetail(1);yield return new WaitForSecondsRealtime(.3f);yield return Capture(prefix+"-detail",app,size.x,size.y,true);
                    yield return Capture(prefix+"-detail-bottom",app,size.x,size.y,true,true);
                    app.CloseDetail();yield return new WaitForSecondsRealtime(.3f);
                    app.lobbyHelp.onClick.Invoke();yield return Capture(prefix+"-help",app,size.x,size.y,true);app.helpClose.onClick.Invoke();
                    app.lobbySettings.onClick.Invoke();yield return Capture(prefix+"-settings",app,size.x,size.y,true);
                    app.noticesOpen.onClick.Invoke();yield return Capture(prefix+"-notices",app,size.x,size.y,true);app.noticesClose.onClick.Invoke();app.settingsClose.onClick.Invoke();
                    app.StartStage(0);app.pause.onClick.Invoke();yield return Capture(prefix+"-pause",app,size.x,size.y,true);app.confirmResume.onClick.Invoke();
                    var board=new ConnectFourBoard();foreach(int col in new[]{0,6,1,6,2,5})board.TryDrop(col);
                    Set("board",board);Call("RefreshBoard");app.SelectColumn(3);app.PlaceSelected();yield return ResultReady();Assert.AreEqual(Data.Text("result_win"),app.resultTitle.text);yield return Capture(prefix+"-win",app,size.x,size.y,true,validate:()=>{if(size.x==1080)AssertMatchFits(app);});
                }
            }
        }
        static IEnumerator Capture(string name, RankGameApp game, int width = 1080, int height = 1920, bool host = false, bool bottom = false, Action validate = null, string directory = "Evidence/opening-ranks-20261015/regression", Action<Texture2D> validateImage = null, bool scaledCanvas = false, float pixelRatio = 1)
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) yield break;
            directory=Environment.GetEnvironmentVariable("STAGEGAME_CAPTURE_DIRECTORY")??directory;
            Directory.CreateDirectory(directory);
            var camera = Camera.main;
            var canvas = game.GetComponent<Canvas>();
            var scaler = game.GetComponent<CanvasScaler>();
            var fitter = game.GetComponentInChildren<SafeAreaFitter>();
            float oldScale = canvas.scaleFactor;
            // CanvasScaler reads the editor Game view, so apply its Expand formula to this render target.
            if(scaledCanvas)Assert.AreEqual(CanvasScaler.ScreenMatchMode.Expand,scaler.screenMatchMode);
            scaler.enabled = false; canvas.scaleFactor = scaledCanvas?Mathf.Min(width/scaler.referenceResolution.x,height/scaler.referenceResolution.y):1;
            fitter.SetEditorViewportPreview(new Vector2(width/pixelRatio, height/pixelRatio), host?new Vector4(0,59,0,34):Vector4.zero, host);
            var target = new RenderTexture(width, height, 24);
            camera.targetTexture = target;
            canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1;
            canvas.pixelPerfect = true;
            Texture2D image=null;var previousActive=RenderTexture.active;
            try
            {
            foreach (var text in game.GetComponentsInChildren<Text>(true))
            {
                Assert.IsTrue(text.font.dynamic, "Capture requires dynamic Korean font");
                text.cachedTextGenerator.Invalidate();
                text.SetAllDirty();
            }
            yield return null;
            foreach(var graphic in game.GetComponentsInChildren<Graphic>(true)) graphic.SetAllDirty();
            Canvas.ForceUpdateCanvases();
            yield return null;
            yield return new WaitForSecondsRealtime(.12f);
            Canvas.ForceUpdateCanvases();
            if(bottom || (game.lobby.activeSelf && name.EndsWith("-lobby-end")))
            {
                // Choose the endpoint after the requested capture size has finished laying out.
                if(game.detail.gameObject.activeSelf)game.detailScroll.verticalNormalizedPosition=0;
                else if(game.lobby.activeSelf)game.stageScroll.verticalNormalizedPosition=0;
                else game.matchScroll.verticalNormalizedPosition=0;
                Canvas.ForceUpdateCanvases();yield return null;Canvas.ForceUpdateCanvases();
            }
            validate?.Invoke();
            if (host)
            {
                if (game.notices.activeSelf)
                {
                    // Font metrics can reflow when switching the camera render target; scroll after that layout.
                    var scroll = game.notices.GetComponentInChildren<ScrollRect>();
                    scroll.verticalNormalizedPosition = 0; Canvas.ForceUpdateCanvases();
                    Assert.AreEqual(0, scroll.verticalNormalizedPosition, .001f);
                }
                var safe = new Rect(0, 34*pixelRatio, width, height - (34+123)*pixelRatio);
                foreach (var button in game.GetComponentsInChildren<Button>())
                {
                    if (button.name.StartsWith("Column_") && !button.interactable) continue;
                    var corners = new Vector3[4]; ((RectTransform)button.transform).GetWorldCorners(corners);
                    var lo = RectTransformUtility.WorldToScreenPoint(camera, corners[0]);
                    var hi = RectTransformUtility.WorldToScreenPoint(camera, corners[2]);
                    Assert.GreaterOrEqual((hi.x - lo.x)/pixelRatio, 43.99f, name + " hit width: " + button.name);
                    Assert.GreaterOrEqual((hi.y - lo.y)/pixelRatio, 43.99f, name + " hit height: " + button.name);
                    if (button.GetComponentInParent<ScrollRect>() != null) continue;
                    Assert.GreaterOrEqual(lo.x, safe.xMin - .1f, name + " left: " + button.name);
                    Assert.GreaterOrEqual(lo.y, safe.yMin - .1f, name + " bottom: " + button.name);
                    Assert.LessOrEqual(hi.x, safe.xMax + .1f, name + " right: " + button.name);
                    Assert.LessOrEqual(hi.y, safe.yMax + .1f, name + " top: " + button.name);
                }
                foreach (var text in game.GetComponentsInChildren<Text>())
                {
                    if (text.GetComponentInParent<ScrollRect>() != null) continue; // Intentionally extends inside the clipped legal ScrollRect.
                    Assert.LessOrEqual(text.preferredHeight, text.rectTransform.rect.height + 2, name + " text: " + text.name + ": " + text.text);
                    var corners = new Vector3[4]; text.rectTransform.GetWorldCorners(corners);
                    Assert.LessOrEqual(RectTransformUtility.WorldToScreenPoint(camera, corners[2]).y, safe.yMax + .1f, name + " text top: " + text.name);
                }
                foreach (var graphic in fitter.content.GetComponentsInChildren<Graphic>())
                {
                    if (!graphic.enabled || graphic.GetComponentInParent<ScrollRect>() != null) continue;
                    var corners = new Vector3[4]; graphic.rectTransform.GetWorldCorners(corners);
                    var lo = RectTransformUtility.WorldToScreenPoint(camera, corners[0]);
                    var hi = RectTransformUtility.WorldToScreenPoint(camera, corners[2]);
                    Assert.GreaterOrEqual(lo.y, safe.yMin - .1f, name + " graphic bottom: " + graphic.name);
                    Assert.LessOrEqual(hi.y, safe.yMax + .1f, name + " graphic top: " + graphic.name);
                }
            }
            Debug.Log($"[Capture] {name}: screen={Screen.width}x{Screen.height}, canvas={canvas.pixelRect}, scale={canvas.scaleFactor}, ppu={game.matchTitle.pixelsPerUnit}, fontSize={game.matchTitle.fontSize}");
            camera.Render();
            RenderTexture.active = target;
            image = new Texture2D(width, height, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, width, height), 0, 0); image.Apply();
            File.WriteAllBytes(Path.Combine(directory,name + ".png"), image.EncodeToPNG());
            validateImage?.Invoke(image);
            if(name.EndsWith("-first-match")||name.EndsWith("-header-match")||name.EndsWith("-header-lobby")||name.EndsWith("-lobby"))
            {
                var title=game.lobby.activeSelf?game.lobbyTitle:game.matchTitle;
                var center=RectTransformUtility.WorldToScreenPoint(camera,title.transform.position);
                var corners=new Vector3[4];title.rectTransform.GetWorldCorners(corners);
                float scale=(RectTransformUtility.WorldToScreenPoint(camera,corners[2]).x-RectTransformUtility.WorldToScreenPoint(camera,corners[0]).x)/title.rectTransform.rect.width;
                int halfWidth=Mathf.CeilToInt(title.preferredWidth*scale/2),halfHeight=Mathf.CeilToInt(title.preferredHeight*scale/2),ink=0;
                for(int y=Mathf.Max(0,(int)center.y-halfHeight);y<Mathf.Min(height,(int)center.y+halfHeight);y++)
                    for(int x=Mathf.Max(0,(int)center.x-halfWidth);x<Mathf.Min(width,(int)center.x+halfWidth);x++)
                        if(image.GetPixel(x,y).grayscale<.5f)ink++;
                Assert.Greater(ink,10,name+" title must render visible pixels; culled="+title.canvasRenderer.cull);
            }
            }
            finally
            {
            RenderTexture.active = previousActive; camera.targetTexture = null;
            canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.worldCamera = null;
            fitter.SetEditorHostPreview(false, Vector2.zero, Vector4.zero);
            canvas.scaleFactor = oldScale; scaler.enabled = true;
            if(image!=null)UnityEngine.Object.Destroy(image); UnityEngine.Object.Destroy(target);
            }
        }
    }
}
