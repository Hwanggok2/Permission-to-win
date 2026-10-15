#if STAGEGAME_TABLETOP_LEGACY_TESTS
// Historical UI suite; the active Main scene is covered by RankFlowTests.
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
using UnityEngine.SceneManagement;

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
        public IEnumerator SafeAreaRefitsAfterCanvasLayoutChangesWithoutScreenResize()
        {
            var canvas = app.GetComponent<Canvas>();
            var scaler = app.GetComponent<CanvasScaler>();
            var fitter = app.GetComponentInChildren<SafeAreaFitter>();
            var safeRect = (RectTransform)fitter.transform;
            var originalScreen = new Vector2Int(Screen.width, Screen.height);
            float oldScale = canvas.scaleFactor;
            scaler.enabled = false;
            try
            {
                // CanvasScaler/layout may change the available rectangle after the Screen-size event.
                // Keep Screen fixed and reproduce that independent rectangle change on the saved scene.
                foreach (float scale in new[] { 0.25f, 1f, 0.25f })
                {
                    canvas.scaleFactor = scale;
                    Canvas.ForceUpdateCanvases(); yield return null;
                    Canvas.ForceUpdateCanvases();
                    float expected = Mathf.Min(1f, safeRect.rect.width / fitter.referenceSize.x, safeRect.rect.height / fitter.referenceSize.y);
                    Assert.AreEqual(originalScreen, new Vector2Int(Screen.width, Screen.height));
                    Assert.AreEqual(expected, fitter.content.localScale.x, 0.001f, "Available rectangle changed with unchanged Screen size.");
                    Assert.AreEqual(expected, fitter.content.localScale.y, 0.001f);
                    Debug.Log($"[Layout Reflow] canvasScale={scale}, available={safeRect.rect.size}, contentScale={fitter.content.localScale.x}");
                }
            }
            finally { canvas.scaleFactor = oldScale; scaler.enabled = true; Canvas.ForceUpdateCanvases(); }
        }

        [UnityTest]
        public IEnumerator HostProfilesKeepAllScreensBelowControlsAndPreserveLicenseScroll()
        {
            foreach (string language in new[] { "ko", "en" })
            {
                app.SetLanguage(language);
                // The final 390 profile also checks returning from a smaller canvas without a Screen event.
                foreach (var viewport in new[] { new Vector2Int(390, 844), new Vector2Int(320, 568), new Vector2Int(390, 844) })
                {
                    string prefix = $"host-{language}-{viewport.x}";
                    app.ShowLobby();
                    Capture(prefix + "-lobby", app, viewport.x, viewport.y, true);
                    app.lobbySettings.onClick.Invoke(); yield return null;
                    Capture(prefix + "-settings", app, viewport.x, viewport.y, true);
                    app.noticesOpen.onClick.Invoke(); yield return null;
                    var scroll = app.notices.GetComponentInChildren<ScrollRect>();
                    Canvas.ForceUpdateCanvases();
                    Assert.Greater(scroll.content.rect.height, scroll.viewport.rect.height);
                    scroll.verticalNormalizedPosition = 0; Canvas.ForceUpdateCanvases();
                    Assert.AreEqual(0, scroll.verticalNormalizedPosition, .001f);
                    Assert.AreEqual(Resources.Load<TextAsset>("LegalNotices").text, app.noticesBody.text);
                    Capture(prefix + "-notices-bottom", app, viewport.x, viewport.y, true);
                    app.noticesClose.onClick.Invoke(); app.settingsClose.onClick.Invoke();
                    app.lobbyHelp.onClick.Invoke(); yield return null;
                    Capture(prefix + "-help", app, viewport.x, viewport.y, true);
                    app.helpClose.onClick.Invoke(); app.play.onClick.Invoke(); yield return null;
                    app.hint.onClick.Invoke(); yield return null;
                    Capture(prefix + "-match", app, viewport.x, viewport.y, true);
                    app.pause.onClick.Invoke(); yield return null;
                    Capture(prefix + "-confirm", app, viewport.x, viewport.y, true);
                    app.confirmResume.onClick.Invoke();
                    var board = new ConnectFourBoard();
                    foreach (int column in new[] { 0, 6, 1, 6, 2, 5 }) board.TryDrop(column);
                    SetField("board", board); Invoke("RefreshBoard"); app.columns[3].onClick.Invoke(); yield return WaitForResult();
                    Capture(prefix + "-result", app, viewport.x, viewport.y, true);
                }
            }
        }

        [UnityTest]
        public IEnumerator ChapterMapSelectsLockedStagesAndKeepsStatesAcrossLanguages()
        {
            var progress = GetField<ProgressStore>("progress");
            progress.Data.unlockedStage = 13; progress.Data.highestClearedStage = 12;
            foreach (string language in new[] { "ko", "en" })
            {
                app.SetLanguage(language); app.ShowLobby(); SetField("selected", 0); Invoke("RefreshLobby");
                Assert.IsTrue(app.stageNodeStates[0].text.StartsWith("✓"));
                Assert.AreEqual("01", app.stageNodeNumbers[0].text);
                Assert.AreEqual(GetField<StringTable>("strings").Text("stage_replay"), app.playLabel.text);
                app.following.onClick.Invoke(); yield return null;
                Assert.AreEqual("11", app.stageNodeNumbers[0].text);
                Assert.AreEqual(GetField<StringTable>("strings").Text("stage_state_current"), app.stageNodeStates[2].text);
                app.stageNodes[3].onClick.Invoke(); yield return null;
                Assert.AreEqual("14", app.stageNodeNumbers[3].text); Assert.IsFalse(app.play.interactable);
                Assert.AreEqual(GetField<StringTable>("strings").Format("stage_lock_requirement", 13), app.stageDetail.text);
                app.play.onClick.Invoke(); Assert.IsTrue(app.lobby.activeSelf); Assert.IsFalse(app.match.activeSelf);
                AssertNoButtonOverlap(app.lobby);
                Capture("map-" + language + "-320-locked", app, 320, 568, true);
                app.stageNodes[2].onClick.Invoke(); Assert.IsTrue(app.play.interactable);
                Capture("map-" + language + "-390-current", app, 390, 844, true);
                app.following.onClick.Invoke(); app.stageNodes[9].onClick.Invoke(); yield return null;
                Assert.AreEqual("30", app.stageNodeNumbers[9].text); Assert.IsFalse(app.following.interactable);
                Assert.IsFalse(app.play.interactable);
                app.previous.onClick.Invoke(); app.previous.onClick.Invoke(); Assert.IsFalse(app.previous.interactable);
            }
        }

        [UnityTest]
        public IEnumerator TwoResultActionsAndFinalStageClearDriveReplayMap()
        {
            // Legal terminal fixtures, including the full-board draw from the independent core harness.
            var fixtures = new[] {
                new[] { 0, 6, 1, 6, 2, 5, 3 },
                new[] { 0, 6, 1, 6, 2, 6, 4, 6 },
                new[] { 0,2,2,2,2,2,2,0,0,0,0,0,1,3,3,3,3,3,3,1,1,1,1,1,4,6,6,6,6,6,6,4,4,4,4,4,5,5,5,5,5,5 }
            };
            foreach (string language in new[] { "ko", "en" })
            {
                app.SetLanguage(language);
                for (int outcome = 0; outcome < fixtures.Length; outcome++)
                {
                    app.StartStage(0);
                    var board = new ConnectFourBoard();
                    foreach (int column in fixtures[outcome]) Assert.IsTrue(board.TryDrop(column));
                    Assert.IsTrue(board.IsFinished);
                    if (outcome == 2) Assert.IsTrue(board.IsDraw);
                    SetField("board", board); Invoke("RefreshBoard"); Invoke("Finish"); yield return WaitForResult();
                    AssertTwoResultActions();
                    AssertNoButtonOverlap(app.result);
                    Capture("map-" + language + "-320-result-" + outcome, app, 320, 568, true);
                }
                var progress = GetField<ProgressStore>("progress");
                progress.Data.unlockedStage = 30; progress.Data.highestClearedStage = 29;
                app.StartStage(29);
                var final = new ConnectFourBoard(); foreach (int column in fixtures[0]) final.TryDrop(column);
                SetField("board", final); Invoke("RefreshBoard"); Invoke("Finish"); yield return WaitForResult();
                Assert.IsFalse(app.nextStage.gameObject.activeSelf);
                AssertTwoResultActions();
                Assert.AreEqual(30, new ProgressStore(30).Data.highestClearedStage);
                app.resultLobby.onClick.Invoke(); yield return null;
                Assert.AreEqual("30", app.stageNodeNumbers[9].text); Assert.IsTrue(app.stageNodeStates[9].text.StartsWith("✓"));
                Assert.AreEqual(GetField<StringTable>("strings").Text("stage_replay"), app.playLabel.text);
                Capture("map-" + language + "-320-all-cleared", app, 320, 568, true);
                app.play.onClick.Invoke(); yield return null;
                Assert.IsTrue(app.match.activeSelf); Assert.IsTrue(app.matchTitle.text.StartsWith("30"));
                app.ShowLobby();
            }
        }

        [UnityTest]
        public IEnumerator EveryStageTitleFitsInBothLanguages()
        {
            GetField<ProgressStore>("progress").Data.unlockedStage = 30;
            foreach (string language in new[] { "ko", "en" })
            {
                app.SetLanguage(language);
                for (int index = 0; index < 30; index++)
                {
                    app.ShowLobby(); SetField("selected", index); Invoke("RefreshLobby");
                    AssertTextFits(app.lobby);
                    app.StartStage(index); yield return null;
                    AssertTextFits(app.match);
                }
            }
        }

        [UnityTest]
        public IEnumerator LanguageButtonsUpdateAllScreensAndSurviveSceneReload()
        {
            var table = StringTable.Load();
            Assert.IsFalse(app.lobbyBanner.activeSelf);
            Capture("ko-lobby", app);
            app.lobbySettings.onClick.Invoke(); yield return null;
            Capture("ko-settings", app);
            app.english.onClick.Invoke(); yield return null;
            table.SetLanguage("en");
            Assert.AreEqual("en", new ProgressStore(30).Data.language);
            Assert.AreEqual(table.Text("ui_settings"), app.settings.transform.Find("Frame/Title").GetComponent<Text>().text);
            Assert.AreEqual(table.Text("stage_001"), app.stageTitle.text);
            foreach (var label in app.localizedLabels) Assert.AreEqual(table.Text(label.key), label.target.text, label.key);
            Capture("en-settings", app);
            AssertTextFits(app.settings);
            AssertNoButtonOverlap(app.settings);
            app.noticesOpen.onClick.Invoke(); yield return null;
            Assert.AreEqual(Resources.Load<TextAsset>("LegalNotices").text, app.noticesBody.text);
            app.noticesClose.onClick.Invoke(); app.settingsClose.onClick.Invoke();
            Capture("en-lobby", app); AssertNoButtonOverlap(app.lobby);
            app.lobbyHelp.onClick.Invoke(); yield return null;
            Capture("en-help", app); AssertTextFits(app.help);
            app.helpClose.onClick.Invoke(); app.play.onClick.Invoke(); yield return null;
            Capture("en-match", app); AssertTextFits(app.match);
            app.hint.onClick.Invoke(); yield return null;
            Assert.AreEqual(table.Text("client_hint_unavailable"), app.matchNotice.text);
            app.pause.onClick.Invoke(); yield return null;
            Capture("en-confirm", app); AssertNoButtonOverlap(app.confirm); AssertTextFits(app.confirm);
            app.confirmResume.onClick.Invoke();
            var board = new ConnectFourBoard();
            foreach (int column in new[] { 0, 6, 1, 6, 2, 5 }) board.TryDrop(column);
            SetField("board", board); Invoke("RefreshBoard");
            app.columns[3].onClick.Invoke(); yield return WaitForResult();
            Assert.AreEqual(table.Text("client_result_win"), app.resultTitle.text);
            Assert.IsFalse(app.resultBanner.activeSelf);
            Capture("en-result", app); AssertNoButtonOverlap(app.result); AssertTextFits(app.result);
            app.ShowLobby(); app.lobbySettings.onClick.Invoke(); app.korean.onClick.Invoke(); yield return null;
            Assert.AreEqual("ko", new ProgressStore(30).Data.language);
            app.settingsClose.onClick.Invoke(); app.play.onClick.Invoke(); yield return null;
            Capture("ko-match", app);
            app.ShowLobby(); app.lobbySettings.onClick.Invoke(); app.english.onClick.Invoke();
            yield return SceneManager.LoadSceneAsync("Main"); yield return null;
            app = UnityEngine.Object.FindFirstObjectByType<GameApp>();
            Assert.AreEqual("en", GetField<StringTable>("strings").Language);
            Assert.AreEqual(table.Text("stage_002"), app.stageTitle.text);
            Assert.AreEqual("1/30 cleared", app.progressLabel.text);
            Capture("en-restored", app);
        }

        static void AssertNoButtonOverlap(GameObject screen)
        {
            Canvas.ForceUpdateCanvases();
            var buttons = screen.GetComponentsInChildren<Button>();
            for (int i = 0; i < buttons.Length; i++)
                for (int j = i + 1; j < buttons.Length; j++)
                    Assert.IsFalse(WorldRect(buttons[i]).Overlaps(WorldRect(buttons[j])), buttons[i].name + " overlaps " + buttons[j].name);
        }
        static Rect WorldRect(Button button)
        {
            var corners = new Vector3[4]; ((RectTransform)button.transform).GetWorldCorners(corners);
            return Rect.MinMaxRect(corners[0].x, corners[0].y, corners[2].x, corners[2].y);
        }
        static void AssertTextFits(GameObject screen)
        {
            Canvas.ForceUpdateCanvases();
            foreach (var text in screen.GetComponentsInChildren<Text>())
                Assert.LessOrEqual(text.preferredHeight, text.rectTransform.rect.height + 2, text.name + ": " + text.text);
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
            app.columns[3].onClick.Invoke(); yield return WaitForResult();
            Assert.IsTrue(app.result.activeSelf); Assert.AreEqual("연결 성공!", app.resultTitle.text);
            AssertTwoResultActions();
            Capture("result", app);
            app.resultLobby.onClick.Invoke(); yield return null;
            Assert.AreEqual(1, GetField<int>("selected"));
            app.play.onClick.Invoke(); yield return null;
            Assert.IsTrue(app.matchTitle.text.StartsWith("02"));
            app.ShowLobby();
            Assert.AreEqual("1/30 완료", app.progressLabel.text);
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

        sealed class SettlementAds : IAdService
        {
            public Action<AdResult> Callback;
            public int Calls;
            public float CalledAt;
            public string Status => "ad_unavailable";
            public void ShowRewarded(Action<AdResult> callback) => callback(AdResult.Unavailable);
            public void ShowInterstitial(Action<AdResult> callback) { Calls++; CalledAt=Time.realtimeSinceStartup; Callback=callback; }
            public void CancelPending() { }
        }

        [UnityTest]
        public IEnumerator BothWinningMovesAnimateBeforeAdsAndTwoResultActions()
        {
            var trace = new List<string> { "side,event,elapsedSeconds,scale,resolving,resultActive,retryEnabled,adCalls,moveCount" };
            foreach (int starter in new[] { 1, -1 })
            {
                app.StartStage(0); yield return null;
                var progress=GetField<ProgressStore>("progress");
                progress.Data.completedMatches=4; progress.Data.lastInterstitialMatch=0; progress.Data.lastInterstitialUtc=0;
                GetField<GameData>("data").Stages[0].aiRandomPermille=0;
                GetField<GameData>("data").Ads.bannerResult=false;
                var ads=new SettlementAds(); app.SetAdsForTests(ads);
                var board=new ConnectFourBoard(starter);
                foreach(int col in new[] {0,6,1,6,2,5}) board.TryDrop(col);
                SetField("board",board); Invoke("RefreshBoard");
                if(starter==1) app.columns[3].onClick.Invoke();
                else app.StartCoroutine((IEnumerator)typeof(GameApp).GetMethod("Think",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(app,new object[]{false}));
                float until=Time.realtimeSinceStartup+3;
                while(!GetField<bool>("resolving") && Time.realtimeSinceStartup<until) yield return null;
                Assert.IsTrue(GetField<bool>("resolving")); Assert.AreEqual(starter,board.Winner);
                float start=Time.realtimeSinceStartup;
                var stone=app.stones[board.LastMove].rectTransform;
                string side=starter==1?"player":"ai";
                void Sample(string name) => trace.Add(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "{0},{1},{2:F4},{3:F4},{4},{5},{6},{7},{8}",side,name,Time.realtimeSinceStartup-start,stone.localScale.x,GetField<bool>("resolving"),app.result.activeSelf,app.retry.interactable,ads.Calls,board.MoveCount));
                Assert.Greater(stone.localScale.x,1.4f);
                Assert.AreEqual(stone.parent.childCount-1,stone.GetSiblingIndex(),"Enlarged stone must render in front of every hole.");
                Sample("enlarged");
                Capture("animation-"+side+"-0",app,390,844,true);
                Assert.IsFalse(app.pause.interactable); Assert.IsFalse(app.hint.interactable);
                foreach(var column in app.columns) Assert.IsFalse(column.interactable);
                app.columns[0].onClick.Invoke(); app.pause.onClick.Invoke(); app.hint.onClick.Invoke(); Invoke("Finish");
                Assert.AreEqual(7,board.MoveCount); Assert.IsFalse(app.confirm.activeSelf); Assert.AreEqual(5,progress.Data.completedMatches);
                while(Time.realtimeSinceStartup-start<.18f) yield return null;
                Assert.Greater(stone.localScale.x,1); Assert.Less(stone.localScale.x,1.45f);
                Assert.AreEqual(0,ads.Calls); Assert.IsFalse(app.result.activeSelf); Sample("placing");
                Capture("animation-"+side+"-1",app,390,844,true);
                while(!GetField<bool>("winningRevealed") && Time.realtimeSinceStartup-start<2) yield return null;
                Assert.AreEqual(Vector3.one,stone.localScale); Assert.AreEqual(board.LastMove,stone.GetSiblingIndex()); Assert.IsFalse(app.result.activeSelf);
                Assert.AreEqual(0,ads.Calls); Assert.GreaterOrEqual(Time.realtimeSinceStartup-start,.35f); Sample("landed");
                foreach(int cell in board.WinningCells) Assert.AreSame(starter==1?app.theme.playerWin:app.theme.opponentWin,app.stones[cell].sprite);
                while(ads.Calls==0 && Time.realtimeSinceStartup-start<2) yield return null;
                Assert.AreEqual(1,ads.Calls); Assert.GreaterOrEqual(ads.CalledAt-start,.85f);
                Assert.IsTrue(app.result.activeSelf); Assert.IsFalse(app.retry.interactable); Assert.IsFalse(app.resultLobby.interactable); Sample("adPending");
                ads.Callback(AdResult.Completed); ads.Callback(AdResult.Completed);
                yield return WaitForResult();
                AssertTwoResultActions(); Assert.AreEqual(5,progress.Data.completedMatches); Assert.AreEqual(5,progress.Data.lastInterstitialMatch);
                Sample("actionsEnabled"); Capture("animation-"+side+"-2",app,390,844,true);
                app.resultLobby.onClick.Invoke(); yield return null;
                Assert.AreEqual(progress.Data.unlockedStage-1,GetField<int>("selected"));
                app.StartStage(0); ads.Callback(AdResult.Completed); yield return null;
                Assert.IsFalse(app.result.activeSelf); Assert.AreEqual(0,GetField<ConnectFourBoard>("board").MoveCount);
            }
            File.WriteAllLines("Evidence/graphics-refresh-20261014/animation-timeline.csv",trace);
        }

        [UnityTest]
        public IEnumerator WinningPresentationCancelsCleanlyAndAdTimeoutRejectsLateCompletion()
        {
            app.StartStage(0); yield return null;
            var board=new ConnectFourBoard(); foreach(int col in new[] {0,6,1,6,2,5}) board.TryDrop(col);
            SetField("board",board); Invoke("RefreshBoard"); app.columns[3].onClick.Invoke();
            var stone=app.stones[board.LastMove].rectTransform; Assert.Greater(stone.localScale.x,1);
            app.StartStage(0); Assert.AreEqual(Vector3.one,stone.localScale); Assert.AreEqual(board.LastMove,stone.GetSiblingIndex());
            yield return new WaitForSecondsRealtime(1);
            Assert.IsFalse(app.result.activeSelf); Assert.IsTrue(app.columns[0].interactable);
            var progress=GetField<ProgressStore>("progress"); progress.Data.completedMatches=4;
            var ads=new SettlementAds(); app.SetAdsForTests(ads);
            SetField("board",board); Invoke("RefreshBoard"); Invoke("Finish");
            yield return WaitForResult();
            AssertTwoResultActions(); Assert.AreEqual(1,ads.Calls);
            int before=progress.Data.lastInterstitialMatch;
            ads.Callback(AdResult.Completed); yield return null;
            Assert.AreEqual(before,progress.Data.lastInterstitialMatch); Assert.AreEqual(5,progress.Data.completedMatches);
        }

        IEnumerator WaitForResult()
        {
            float until = Time.realtimeSinceStartup + 6;
            while ((!app.result.activeSelf || !app.retry.interactable) && Time.realtimeSinceStartup < until) yield return null;
            Assert.IsTrue(app.result.activeSelf); Assert.IsTrue(app.retry.interactable);
        }

        void AssertTwoResultActions()
        {
            var buttons = app.result.GetComponentsInChildren<Button>();
            Assert.AreEqual(2, buttons.Length);
            CollectionAssert.AreEquivalent(new[] { app.retry, app.resultLobby }, buttons);
            Assert.IsFalse(app.nextStage.gameObject.activeSelf); Assert.IsFalse(app.resultClose.gameObject.activeSelf);
            Assert.AreSame(app.theme.primary, app.retry.GetComponent<Image>().sprite);
            Assert.AreEqual(((RectTransform)app.retry.transform).anchoredPosition.y, ((RectTransform)app.resultLobby.transform).anchoredPosition.y);
            Assert.IsTrue(app.match.activeSelf); Assert.IsFalse(app.hint.gameObject.activeSelf); Assert.IsFalse(app.pause.interactable); Assert.IsFalse(app.pause.gameObject.activeSelf);
            Assert.AreEqual(0, app.result.GetComponent<Image>().color.a);
            Assert.AreEqual(0, app.result.transform.Find("Frame").GetComponent<Image>().color.a);
            Assert.IsFalse(app.result.GetComponent<Image>().raycastTarget);
            AssertNoButtonOverlap(app.result);
        }

        void SetField(string field, object value) => typeof(GameApp).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(app, value);
        T GetField<T>(string field) => (T)typeof(GameApp).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(app);
        void Invoke(string method, params object[] arguments) => typeof(GameApp).GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(app, arguments);

        static void Capture(string name, GameApp game, int width = 720, int height = 1280, bool host = false)
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) return;
            Directory.CreateDirectory("Evidence/graphics-refresh-20261014");
            var camera = Camera.main;
            var canvas = game.GetComponent<Canvas>();
            var scaler = game.GetComponent<CanvasScaler>();
            var fitter = game.GetComponentInChildren<SafeAreaFitter>();
            float oldScale = canvas.scaleFactor;
            scaler.enabled = false; canvas.scaleFactor = 1;
            fitter.SetEditorHostPreview(host, new Vector2(width, height), new Vector4(0, 59, 0, 34));
            var target = new RenderTexture(width, height, 24);
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
            if (host)
            {
                if (game.notices.activeSelf)
                {
                    // Font metrics can reflow when switching the camera render target; scroll after that layout.
                    var scroll = game.notices.GetComponentInChildren<ScrollRect>();
                    scroll.verticalNormalizedPosition = 0; Canvas.ForceUpdateCanvases();
                    Assert.AreEqual(0, scroll.verticalNormalizedPosition, .001f);
                }
                var safe = new Rect(0, 34, width, height - 34 - 123);
                foreach (var button in game.GetComponentsInChildren<Button>())
                {
                    var corners = new Vector3[4]; ((RectTransform)button.transform).GetWorldCorners(corners);
                    var lo = RectTransformUtility.WorldToScreenPoint(camera, corners[0]);
                    var hi = RectTransformUtility.WorldToScreenPoint(camera, corners[2]);
                    Assert.GreaterOrEqual(hi.x - lo.x, 43.99f, name + " hit width: " + button.name);
                    Assert.GreaterOrEqual(hi.y - lo.y, 43.99f, name + " hit height: " + button.name);
                    Assert.GreaterOrEqual(lo.x, safe.xMin - .1f, name + " left: " + button.name);
                    Assert.GreaterOrEqual(lo.y, safe.yMin - .1f, name + " bottom: " + button.name);
                    Assert.LessOrEqual(hi.x, safe.xMax + .1f, name + " right: " + button.name);
                    Assert.LessOrEqual(hi.y, safe.yMax + .1f, name + " top: " + button.name);
                }
                foreach (var text in game.GetComponentsInChildren<Text>())
                {
                    if (text == game.noticesBody) continue; // Intentionally extends inside the clipped legal ScrollRect.
                    Assert.LessOrEqual(text.preferredHeight, text.rectTransform.rect.height + 2, name + " text: " + text.name + ": " + text.text);
                    var corners = new Vector3[4]; text.rectTransform.GetWorldCorners(corners);
                    Assert.LessOrEqual(RectTransformUtility.WorldToScreenPoint(camera, corners[2]).y, safe.yMax + .1f, name + " text top: " + text.name);
                }
                foreach (var graphic in fitter.content.GetComponentsInChildren<Graphic>())
                {
                    if (graphic == game.noticesBody) continue;
                    var corners = new Vector3[4]; graphic.rectTransform.GetWorldCorners(corners);
                    var lo = RectTransformUtility.WorldToScreenPoint(camera, corners[0]);
                    var hi = RectTransformUtility.WorldToScreenPoint(camera, corners[2]);
                    Assert.GreaterOrEqual(lo.y, safe.yMin - .1f, name + " graphic bottom: " + graphic.name);
                    Assert.LessOrEqual(hi.y, safe.yMax + .1f, name + " graphic top: " + graphic.name);
                }
            }
            Debug.Log($"[Capture] {name}: screen={Screen.width}x{Screen.height}, canvas={canvas.pixelRect}, scale={canvas.scaleFactor}, ppu={game.stageTitle.pixelsPerUnit}, fontSize={game.stageTitle.fontSize}");
            camera.Render();
            var previous = RenderTexture.active; RenderTexture.active = target;
            var image = new Texture2D(width, height, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, width, height), 0, 0); image.Apply();
            File.WriteAllBytes("Evidence/graphics-refresh-20261014/" + name + ".png", image.EncodeToPNG());
            RenderTexture.active = previous; camera.targetTexture = null;
            canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.worldCamera = null;
            fitter.SetEditorHostPreview(false, Vector2.zero, Vector4.zero);
            canvas.scaleFactor = oldScale; scaler.enabled = true;
            UnityEngine.Object.Destroy(image); UnityEngine.Object.Destroy(target);
        }
    }
}

#endif
