using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace StageGame.Tests
{
    public sealed class PlayFlowTests
    {
        const string SaveKey = "stagegame.progress.v1";
        bool hadSave;
        string originalSave;
        GameApp app;

        [UnitySetUp]
        public IEnumerator Setup()
        {
            hadSave = PlayerPrefs.HasKey(SaveKey);
            originalSave = PlayerPrefs.GetString(SaveKey, "");
            PlayerPrefs.DeleteKey(SaveKey);
            EditorSceneManager.OpenScene(Editor.ProjectBuilder.ScenePath);
            yield return new EnterPlayMode();
            yield return null;
            app = UnityEngine.Object.FindFirstObjectByType<GameApp>();
            Assert.NotNull(app);
        }

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            yield return new ExitPlayMode();
            if (hadSave) PlayerPrefs.SetString(SaveKey, originalSave); else PlayerPrefs.DeleteKey(SaveKey);
            PlayerPrefs.Save();
        }

        [UnityTest]
        public IEnumerator SavedSceneSupportsLobbyMatchResultAndProgress()
        {
            Assert.IsTrue(app.lobby.activeSelf); Assert.IsTrue(app.play.interactable);
            Capture("lobby", app);
            app.play.onClick.Invoke(); yield return null;
            Assert.IsTrue(app.match.activeSelf);
            Assert.AreEqual(42, app.stones.Length);
            app.hint.onClick.Invoke(); yield return null;
            Assert.IsTrue(app.matchNotice.text.Contains("준비"));
            Assert.IsTrue(app.columns[3].interactable);
            app.columns[3].onClick.Invoke();
            float deadline = Time.realtimeSinceStartup + 3f;
            while (!app.moveLabel.text.StartsWith("02") && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.IsTrue(app.moveLabel.text.StartsWith("02"));
            Assert.IsTrue(app.columns[3].interactable);
            Capture("match", app);
            // Inject a legal near-win position, then finish through the real saved UI button.
            var board = new ConnectFourBoard();
            foreach (int col in new[] { 0, 6, 1, 6, 2, 5 }) board.TryDrop(col);
            SetField("board", board); Invoke("RefreshBoard");
            app.columns[3].onClick.Invoke(); yield return null;
            Assert.IsTrue(app.result.activeSelf); Assert.AreEqual("연결 성공!", app.resultTitle.text);
            Assert.IsTrue(app.nextStage.interactable);
            Capture("result", app);
            app.nextStage.onClick.Invoke(); yield return null;
            Assert.IsTrue(app.matchTitle.text.StartsWith("02"));
            app.ShowLobby();
            Assert.IsTrue(app.progressLabel.text.StartsWith("2 /"));
            Assert.AreEqual(2, new ProgressStore(30).Data.unlockedStage);
            app.lobbyHelp.onClick.Invoke(); yield return null;
            Capture("help", app);
            app.helpClose.onClick.Invoke(); Assert.IsFalse(app.help.activeSelf);
            app.lobbySettings.onClick.Invoke(); yield return null;
            Capture("settings", app);
            app.noticesOpen.onClick.Invoke(); yield return null;
            Assert.IsTrue(app.notices.activeSelf);
            Assert.IsTrue(app.noticesBody.text.Contains("SIL OPEN FONT LICENSE"));
            Assert.IsTrue(app.noticesBody.text.Contains("Unity Companion"));
            Capture("notices", app);
            Canvas.ForceUpdateCanvases();
            var click = new PointerEventData(EventSystem.current) { position = RectTransformUtility.WorldToScreenPoint(null, app.noticesClose.transform.position), button = PointerEventData.InputButton.Left };
            var hits = new List<RaycastResult>(); EventSystem.current.RaycastAll(click, hits);
            Assert.IsNotEmpty(hits);
            var clickTarget = ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject);
            Assert.AreEqual(app.noticesClose.gameObject, clickTarget, "The visible X must win the UI raycast, not the scrolling body.");
            ExecuteEvents.Execute(clickTarget, click, ExecuteEvents.pointerClickHandler);
            Assert.IsFalse(app.notices.activeSelf);
            app.settingsClose.onClick.Invoke(); Assert.IsFalse(app.settings.activeSelf);
        }

        sealed class DelayedAds : IAdService
        {
            public Action<AdResult> Callback;
            public int CancelCount;
            public string Status => "test";
            public void ShowRewarded(Action<AdResult> callback) { Callback = callback; }
            public void ShowInterstitial(Action<AdResult> callback) { callback(AdResult.Unavailable); }
            public void CancelPending() { CancelCount++; }
        }

        [UnityTest]
        public IEnumerator RewardPendingLocksBoardAndTimeoutCancelsLateSuccess()
        {
            app.play.onClick.Invoke(); yield return null;
            var ads = new DelayedAds(); app.SetAdsForTests(ads);
            app.hint.onClick.Invoke();
            Assert.IsFalse(app.columns[0].interactable); Assert.IsFalse(app.hint.interactable);
            app.columns[0].onClick.Invoke(); Assert.IsTrue(app.moveLabel.text.StartsWith("00"));
            yield return new WaitForSecondsRealtime(6.2f);
            Assert.IsTrue(app.columns[0].interactable); Assert.Greater(ads.CancelCount, 0);
            ads.Callback(AdResult.Completed); yield return null;
            Assert.IsFalse(GetField<RewardGate>("rewardGate").Granted);
            Assert.IsTrue(app.columns[0].interactable);
        }

        [UnityTest]
        public IEnumerator RewardIsCancelledByModalBackgroundAndMatchTransition()
        {
            app.play.onClick.Invoke(); yield return null;
            var ads = new DelayedAds(); app.SetAdsForTests(ads);
            app.hint.onClick.Invoke(); app.pause.onClick.Invoke(); ads.Callback(AdResult.Completed);
            Assert.IsFalse(GetField<RewardGate>("rewardGate").Granted);
            app.confirmResume.onClick.Invoke(); Assert.IsTrue(app.columns[0].interactable);
            app.hint.onClick.Invoke(); Invoke("OnApplicationPause", true); ads.Callback(AdResult.Completed);
            Assert.IsFalse(GetField<RewardGate>("rewardGate").Granted);
            Invoke("OnApplicationPause", false); Assert.IsTrue(app.columns[0].interactable);
            app.hint.onClick.Invoke(); Action<AdResult> stale = ads.Callback;
            app.ShowLobby(); app.play.onClick.Invoke(); stale(AdResult.Completed); yield return null;
            Assert.IsFalse(GetField<RewardGate>("rewardGate").Granted);
            Assert.IsTrue(app.columns[0].interactable);
        }

        [UnityTest]
        public IEnumerator DuplicateRewardCompletionStartsOnlyOneHint()
        {
            app.play.onClick.Invoke(); yield return null;
            app.SetEditorAds(AdResult.Completed, true); app.hint.onClick.Invoke();
            yield return new WaitForSecondsRealtime(.9f);
            Assert.IsTrue(GetField<RewardGate>("rewardGate").Granted);
            Assert.IsTrue(app.matchNotice.text.StartsWith("추천:"));
            Assert.IsFalse(app.hint.interactable); Assert.IsTrue(app.columns[0].interactable);
        }

        [UnityTest]
        public IEnumerator HighestAiBudgetYieldsFramesAndReturnsWithinActiveDeadline()
        {
            app.play.onClick.Invoke(); yield return null;
            SetField("activeStage", 29);
            SetField("board", new ConnectFourBoard(-1));
            var routine = (IEnumerator)typeof(GameApp).GetMethod("Think", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(app, new object[] { false });
            float start = Time.realtimeSinceStartup; int frames = 0;
            app.StartCoroutine(routine);
            while (GetField<bool>("busy") && Time.realtimeSinceStartup - start < 2f) { frames++; yield return null; }
            Assert.IsFalse(GetField<bool>("busy")); Assert.Greater(frames, 1);
            Assert.AreEqual(1, GetField<ConnectFourBoard>("board").MoveCount);
            Assert.Less(Time.realtimeSinceStartup - start, 1.5f, "Editor smoke threshold including 250ms presentation delay; not a device p95 claim.");
            Debug.Log($"[AI Smoke] top budget completed across {frames} frames in {(Time.realtimeSinceStartup - start) * 1000:F1}ms");
        }

        void SetField(string field, object value) => typeof(GameApp).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(app, value);
        T GetField<T>(string field) => (T)typeof(GameApp).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(app);
        void Invoke(string method, params object[] arguments) => typeof(GameApp).GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(app, arguments);

        static void Capture(string name, GameApp game)
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) return;
            Directory.CreateDirectory("Evidence");
            var camera = Camera.main;
            var canvas = game.GetComponent<Canvas>();
            var scaler = game.GetComponent<CanvasScaler>();
            float oldScale = canvas.scaleFactor;
            scaler.enabled = false; canvas.scaleFactor = 1;
            var target = new RenderTexture(720, 1280, 24);
            camera.targetTexture = target;
            canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1;
            canvas.pixelPerfect = true;
            foreach (var text in game.GetComponentsInChildren<Text>(true))
            {
                Assert.IsTrue(text.font.dynamic, "Capture requires dynamic Korean font");
                text.cachedTextGenerator.Invalidate();
                text.SetAllDirty();
            }
            Canvas.ForceUpdateCanvases();
            Debug.Log($"[Capture] {name}: screen={Screen.width}x{Screen.height}, canvas={canvas.pixelRect}, scale={canvas.scaleFactor}, ppu={game.stageTitle.pixelsPerUnit}, fontSize={game.stageTitle.fontSize}");
            camera.Render();
            var previous = RenderTexture.active; RenderTexture.active = target;
            var image = new Texture2D(720, 1280, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, 720, 1280), 0, 0); image.Apply();
            File.WriteAllBytes("Evidence/" + name + ".png", image.EncodeToPNG());
            RenderTexture.active = previous; camera.targetTexture = null;
            canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.worldCamera = null;
            canvas.scaleFactor = oldScale; scaler.enabled = true;
            UnityEngine.Object.Destroy(image); UnityEngine.Object.Destroy(target);
        }
    }
}
