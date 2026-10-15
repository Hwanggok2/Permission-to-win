using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace StageGame
{
    [Serializable] public sealed class LocalizedLabel { public Text target; public string key; }

    public sealed class GameApp : MonoBehaviour
    {
        [Header("Saved prefab references")]
        public GameObject lobby, match, result, settings, help, confirm;
        public Text stageNumber, stageTitle, stageDetail, progressLabel, lobbyNotice;
        public Text matchTitle, turnLabel, matchNotice, moveLabel;
        public Text resultTitle, resultDetail, resultAdStatus;
        public Text settingsDetail, helpBody;
        public Button previous, following, play, lobbySettings, lobbyHelp;
        public Button pause, hint, retry, nextStage, resultLobby, resultClose;
        public Button settingsClose, soundToggle, helpClose, confirmClose, confirmLeave, confirmResume;
        public Text soundLabel;
        public Image[] stones;
        public Text[] stoneLabels;
        public Text[] columnLabels;
        public Image[] columnMarkers;
        public Button[] columns;
        public GameObject lobbyBanner, resultBanner;
        public AudioSource audioSource;
        public AudioClip moveSound;
        public GameObject notices;
        public Button noticesOpen, noticesClose;
        public Text noticesBody;
        public Button korean, english;
        public Text koreanLabel, englishLabel, playLabel, hintLabel;
        public LocalizedLabel[] localizedLabels;
        public Button[] stageNodes;
        public Text[] stageNodeNumbers, stageNodeStates;
        public Text chapterLabel, chapterTier;
        public Text retryLabel, nextStageLabel, resultLobbyLabel;
        public RectTransform resultPrimarySlot, resultSecondarySlot;

        public TabletopTheme theme;
        public Image[] stageNodeIcons;
        static readonly Color Ink = new Color32(23, 62, 67, 255);
        static readonly Color Highlight = new Color32(228, 182, 92, 255);
        GameData data;
        StringTable strings;
        ProgressStore progress;
        ConnectFourBoard board;
        IAdService ads = new NoFillAds();
        RewardGate rewardGate;
        int selected, activeStage, hintColumn = -1;
        string matchId;
        bool busy, applicationPaused, resultRecorded, leaving, resolving, winningRevealed;
        Coroutine aiRoutine, resultRoutine;
        RectTransform animatedStone;
        Vector2 animatedPosition;
        int animatedSibling;
        string matchNoticeKey = "match_instruction";
        object[] matchNoticeArgs = Array.Empty<object>();
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] static extern void StageGameSetLanguage(string language);
#endif

        string T(string key) => strings.Text(key);
        string F(string key, params object[] args) => strings.Format(key, args);

        void Start()
        {
            previous.onClick.AddListener(() => Select(-1));
            following.onClick.AddListener(() => Select(1));
            play.onClick.AddListener(() => StartStage(selected));
            lobbySettings.onClick.AddListener(OpenSettings);
            lobbyHelp.onClick.AddListener(() => help.SetActive(true));
            settingsClose.onClick.AddListener(() => settings.SetActive(false));
            helpClose.onClick.AddListener(() => help.SetActive(false));
            soundToggle.onClick.AddListener(ToggleSound);
            korean.onClick.AddListener(() => SetLanguage("ko"));
            english.onClick.AddListener(() => SetLanguage("en"));
            noticesOpen.onClick.AddListener(() => notices.SetActive(true));
            noticesClose.onClick.AddListener(() => notices.SetActive(false));
            pause.onClick.AddListener(() => { if (resolving || board == null || board.IsFinished) return; CancelPendingReward(); confirm.SetActive(true); RefreshBoard(); });
            confirmClose.onClick.AddListener(Resume);
            confirmResume.onClick.AddListener(Resume);
            confirmLeave.onClick.AddListener(ShowLobby);
            hint.onClick.AddListener(RequestHint);
            retry.onClick.AddListener(() => { if (retry.interactable) StartStage(activeStage); });
            resultLobby.onClick.AddListener(() => { if (resultLobby.interactable) ShowLobby(); });
            for (int i = 0; i < columns.Length; i++) { int col = i; columns[i].onClick.AddListener(() => Drop(col)); }
            for (int i = 0; i < stageNodes.Length; i++)
            {
                int slot = i;
                stageNodes[i].onClick.AddListener(() => { if (data != null) { selected = selected / 10 * 10 + slot; RefreshLobby(); } });
            }
            try
            {
                strings = StringTable.Load();
                data = GameData.Load(strings);
                progress = new ProgressStore(data.Stages.Length);
                strings.SetLanguage(progress.Data.language);
                selected = progress.Data.unlockedStage - 1;
                RefreshLanguage();
                ShowLobby();
            }
            catch (Exception error)
            {
                Debug.LogError("[StageGame Data] " + error);
                lobby.SetActive(true); match.SetActive(false);
                CloseModals();
                // Bootstrap fallback only: the Excel table itself may be unreadable.
                stageTitle.text = strings != null && strings.Contains("error_data_title") ? T("error_data_title") : "데이터 오류 / Data unavailable";
                stageDetail.text = strings != null && strings.Contains("client_error_data") ? T("client_error_data") : "다시 실행해 주세요. / Please restart.";
                lobbyNotice.text = "";
                play.interactable = previous.interactable = following.interactable = false;
                lobbySettings.interactable = lobbyHelp.interactable = false;
                foreach (var node in stageNodes) node.interactable = false;
                lobbyBanner.SetActive(false);
            }
        }

        void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Escape) || resolving || (result.activeSelf && !resultLobby.interactable)) return;
            if (notices.activeSelf) notices.SetActive(false);
            else if (help.activeSelf) help.SetActive(false);
            else if (settings.activeSelf) settings.SetActive(false);
            else if (confirm.activeSelf) Resume();
            else if (result.activeSelf) ShowLobby();
            else if (match.activeSelf) { CancelPendingReward(); confirm.SetActive(true); RefreshBoard(); }
        }

        void Select(int change)
        {
            if (data == null) return;
            selected = Mathf.Clamp(selected / 10 + change, 0, 2) * 10;
            RefreshLobby();
        }

        void RefreshLobby()
        {
            var stage = data.Stages[selected];
            bool unlocked = stage.id <= progress.Data.unlockedStage;
            bool cleared = stage.id <= progress.Data.highestClearedStage;
            int chapter = selected / 10;
            stageNumber.text = F("stage_number", stage.chapter.ToString("00"), stage.id.ToString("00"));
            stageTitle.text = data.Text(stage.titleKey);
            stageDetail.text = unlocked ? T(cleared ? "stage_state_cleared" : "stage_state_current") : F("stage_lock_requirement", stage.id - 1);
            progressLabel.text = F("stage_cleared_progress", progress.Data.highestClearedStage, data.Stages.Length);
            playLabel.text = T(unlocked ? cleared ? "stage_replay" : "client_ui_start" : "stage_locked");
            play.interactable = unlocked;
            previous.interactable = chapter > 0;
            following.interactable = chapter < 2;
            chapterLabel.text = F("stage_map_chapter", chapter + 1, 3);
            chapterTier.text = T("stage_tier_" + (chapter + 1));
            for (int i = 0; i < stageNodes.Length; i++)
            {
                int index = chapter * 10 + i;
                bool isCleared = index + 1 <= progress.Data.highestClearedStage;
                bool isLocked = index + 1 > progress.Data.unlockedStage;
                bool isSelected = index == selected;
                string number = (index + 1).ToString("00");
                stageNodeNumbers[i].text = number;
                stageNodeStates[i].text = (isCleared ? "✓ " : "") + T(isCleared ? "stage_state_cleared" : isLocked ? "stage_state_locked" : "stage_state_current");
                ((Image)stageNodes[i].targetGraphic).sprite = isSelected ? theme.nodeSelected : isLocked ? theme.nodeLocked : isCleared ? theme.nodeCleared : theme.nodeCurrent;
                stageNodes[i].targetGraphic.color = Color.white;
                stageNodeNumbers[i].color = stageNodeStates[i].color = Ink;
                stageNodeIcons[i].sprite = isLocked ? theme.locked : isCleared ? theme.check : theme.play;
                stageNodeIcons[i].color = Ink;
            }
            lobbyBanner.SetActive(data.Ads.bannerLobby && !(ads is NoFillAds));
            string warning = progress.SaveWarning ?? progress.LoadWarning;
            lobbyNotice.text = warning == null ? "" : T(warning);
            progressLabel.gameObject.SetActive(warning == null);
        }

        public void ShowLobby()
        {
            CancelResultSequence();
            CancelSearch();
            CancelPendingReward();
            ads.CancelPending();
            matchId = null;
            busy = false;
            match.SetActive(false); lobby.SetActive(true); CloseModals();
            if (data != null) { selected = progress.Data.unlockedStage - 1; RefreshLobby(); }
        }

        void CloseModals()
        {
            result.SetActive(false); settings.SetActive(false); help.SetActive(false); confirm.SetActive(false);
            notices.SetActive(false);
        }

        public void StartStage(int index)
        {
            if (data == null || index < 0 || index >= data.Stages.Length || index + 1 > progress.Data.unlockedStage) return;
            CancelResultSequence();
            CancelSearch();
            CancelPendingReward();
            ads.CancelPending();
            try { board = GameData.CreateBoard(data.Stages[index]); }
            catch (Exception error) { Debug.LogError(error); ShowLobby(); lobbyNotice.text = T("client_error_data"); return; }
            activeStage = selected = index;
            matchId = Guid.NewGuid().ToString("N");
            rewardGate = new RewardGate();
            resultRecorded = false; winningRevealed = false; busy = false; hintColumn = -1;
            turnLabel.gameObject.SetActive(true); matchNotice.gameObject.SetActive(true); hint.gameObject.SetActive(true); pause.gameObject.SetActive(true);
            foreach(var marker in columnMarkers) marker.gameObject.SetActive(true);
            lobby.SetActive(false); match.SetActive(true); CloseModals();
            matchTitle.text = F("match_title", (index + 1).ToString("00"), data.Text(data.Stages[index].titleKey));
            SetMatchNotice("match_instruction");
            RefreshBoard();
            if (board.CurrentPlayer == -1) aiRoutine = StartCoroutine(Think(false));
        }

        void Drop(int column)
        {
            if (board == null || busy || rewardGate.Pending || applicationPaused || confirm.activeSelf || board.CurrentPlayer != 1 || !board.TryDrop(column)) return;
            hintColumn = -1;
            SetMatchNotice("match_instruction");
            PlayMove();
            RefreshBoard();
            if (board.IsFinished) Finish();
            else aiRoutine = StartCoroutine(Think(false));
        }

        IEnumerator Think(bool isHint)
        {
            busy = true;
            RefreshBoard();
            if (!isHint) yield return new WaitForSecondsRealtime(0.25f);
            var stage = data.Stages[activeStage];
            using (var search = new AiSearch(board, isHint ? Math.Max(3, stage.aiDepth) : stage.aiDepth,
                stage.aiNodeBudget, isHint ? 0 : stage.aiRandomPermille, activeStage * 997 + board.MoveCount * 31))
            {
                double activeSeconds = 0;
                float lastActiveTick = Time.realtimeSinceStartup;
                while (!search.Complete && activeSeconds < 0.65)
                {
                    float currentTick = Time.realtimeSinceStartup;
                    if (!applicationPaused && !confirm.activeSelf)
                    {
                        activeSeconds += currentTick - lastActiveTick;
                        if (activeSeconds >= 0.65) break;
                        var frameTimer = System.Diagnostics.Stopwatch.StartNew();
                        do { search.Step(16); }
                        while (!search.Complete && frameTimer.Elapsed.TotalSeconds < 0.004 && activeSeconds + frameTimer.Elapsed.TotalSeconds < 0.65);
                    }
                    lastActiveTick = currentTick;
                    yield return null;
                }
                Debug.Log($"[AI] nodes={search.NodesVisited}, completedDepth={search.CompletedDepth}, activeMs={activeSeconds * 1000:F1}");
                while (applicationPaused || confirm.activeSelf) yield return null;
                if (isHint)
                {
                    hintColumn = search.ChosenColumn;
                    SetMatchNotice("hint_recommendation", hintColumn + 1);
                }
                else if (board.TryDrop(search.ChosenColumn)) PlayMove();
            }
            busy = false; aiRoutine = null;
            RefreshBoard();
            if (!isHint && board.IsFinished) Finish();
        }

        void RefreshBoard()
        {
            if (board == null) return;
            for (int i = 0; i < stones.Length; i++)
            {
                int player = board[i % 7, i / 7];
                bool winning = false;
                foreach (int cell in board.WinningCells) if (cell == i) winning = true;
                stones[i].sprite = player == 1 ? winning && winningRevealed ? theme.playerWin : theme.player : player == -1 ? winning && winningRevealed ? theme.opponentWin : theme.opponent : theme.hole;
                stones[i].color = Color.white;
                stoneLabels[i].text = "";
            }
            bool canInput = !busy && !rewardGate.Pending && !applicationPaused && !confirm.activeSelf && !board.IsFinished && board.CurrentPlayer == 1;
            for (int col = 0; col < columns.Length; col++)
            {
                columns[col].interactable = canInput && board.CanDrop(col);
                columnMarkers[col].color = col == hintColumn ? Highlight : new Color(23 / 255f, 62 / 255f, 67 / 255f, .09f);
            }
            pause.interactable = !resolving && !board.IsFinished;
            hint.interactable = canInput && !rewardGate.Granted && !rewardGate.Pending;
            hintLabel.text = T(rewardGate.Granted ? "client_hint_used" : "client_hint_request");
            turnLabel.text = T(board.IsFinished ? "turn_finished" : busy ? "turn_thinking" : board.CurrentPlayer == 1 ? "client_ui_player_turn" : "client_ui_ai_turn");
            moveLabel.text = F("move_count", board.MoveCount.ToString("00"));
        }

        void Finish()
        {
            if (resultRecorded || board == null || !board.IsFinished) return;
            resultRecorded = true; resolving = true;
            CancelPendingReward();
            progress.RecordResult(matchId, activeStage + 1, board.Winner == 1, data.Stages.Length);
            SetResultActions(false);
            RefreshBoard();
            resultRoutine = StartCoroutine(FinishSequence(matchId));
        }

        IEnumerator FinishSequence(string id)
        {
            if (board.Winner != 0 && board.LastMove >= 0)
            {
                animatedStone = stones[board.LastMove].rectTransform;
                animatedPosition = animatedStone.anchoredPosition;
                animatedSibling = animatedStone.GetSiblingIndex();
                animatedStone.SetAsLastSibling();
                float elapsed = 0;
                while (elapsed < .4f && matchId == id)
                {
                    float t = Mathf.Clamp01(elapsed / .4f);
                    float ease = 1 - Mathf.Pow(1 - t, 3);
                    animatedStone.localScale = Vector3.one * Mathf.Lerp(1.45f, 1, ease);
                    animatedStone.anchoredPosition = animatedPosition + Vector2.up * Mathf.Lerp(34, 0, ease);
                    yield return null;
                    if (!applicationPaused) elapsed += Time.unscaledDeltaTime;
                }
                RestoreAnimatedStone();
                winningRevealed = true;
                RefreshBoard();
                elapsed = 0;
                while (elapsed < .5f && matchId == id)
                {
                    yield return null;
                    if (!applicationPaused) elapsed += Time.unscaledDeltaTime;
                }
            }
            if (matchId != id) yield break;
            RefreshResultText();
            nextStage.gameObject.SetActive(false); resultClose.gameObject.SetActive(false);
            hint.gameObject.SetActive(false); pause.gameObject.SetActive(false);
            foreach(var marker in columnMarkers) marker.gameObject.SetActive(false);
            turnLabel.gameObject.SetActive(false); matchNotice.gameObject.SetActive(false);
            resultBanner.SetActive(data.Ads.bannerResult && !(ads is NoFillAds));
            result.SetActive(true);
            // Presentation/settlement happens after the winning move and line observation.
            yield return ResultAdBoundary(id);
            resolving = false;
            resultRoutine = null;
        }

        void RestoreAnimatedStone()
        {
            if (animatedStone == null) return;
            animatedStone.localScale = Vector3.one;
            animatedStone.anchoredPosition = animatedPosition;
            animatedStone.SetSiblingIndex(animatedSibling);
            animatedStone = null;
        }

        void CancelResultSequence()
        {
            if (resultRoutine != null) StopCoroutine(resultRoutine);
            resultRoutine = null; resolving = false;
            RestoreAnimatedStone();
        }

        void RefreshResultText()
        {
            if (board == null || !board.IsFinished) return;
            resultTitle.text = T(board.Winner == 1 ? "client_result_win" : board.IsDraw ? "result_draw" : "client_result_loss");
            resultDetail.text = T(board.Winner == 1 ? activeStage + 1 == data.Stages.Length ? "client_result_all_clear" : "result_win_detail" :
                board.IsDraw ? "result_draw_detail" : LossExplanationKey());
            resultAdStatus.text = !string.IsNullOrEmpty(progress.SaveWarning) ? T(progress.SaveWarning) : ads is NoFillAds ? "" : T(ads.Status);
            retry.GetComponent<Image>().sprite = theme.primary;
            resultLobby.GetComponent<Image>().sprite = theme.secondary;
            retryLabel.color = resultLobbyLabel.color = Ink;
        }

        string LossExplanationKey()
        {
            if (board.WinningCells.Count < 2) return "result_loss_detail";
            int a = board.WinningCells[0], b = board.WinningCells[1];
            return a / 7 == b / 7 ? "result_loss_horizontal" : a % 7 == b % 7 ? "result_loss_vertical" : "result_loss_diagonal";
        }

        IEnumerator ResultAdBoundary(string id)
        {
            bool resolved = false;
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (AdPolicy.Eligible(data.Ads, progress.Data, now))
            {
                ads.ShowInterstitial(value =>
                {
                    if (resolved || matchId != id || !result.activeSelf || applicationPaused) return;
                    resolved = true;
                    if (value == AdResult.Completed)
                    {
                        progress.Data.lastInterstitialMatch = progress.Data.completedMatches;
                        progress.Data.lastInterstitialUtc = now;
                        progress.Save();
                    }
                });
            }
            else resolved = true;
            // The shipped no-fill adapter resolves synchronously. Future SDKs must cancel late presentation.
            float deadline = Time.realtimeSinceStartup + 4f;
            while (!resolved && Time.realtimeSinceStartup < deadline && matchId == id) yield return null;
            resolved = true;
            ads.CancelPending();
            if (matchId == id && result.activeSelf) SetResultActions(true);
        }

        void SetResultActions(bool enabled)
        {
            retry.interactable = resultLobby.interactable = enabled;
            nextStage.interactable = resultClose.interactable = false;
        }

        void RequestHint()
        {
            if (busy || applicationPaused || confirm.activeSelf || board == null || board.IsFinished || board.CurrentPlayer != 1 || !rewardGate.TryBegin(out int token)) return;
            string id = matchId;
            var gate = rewardGate;
            int moveAtRequest = board.MoveCount;
            RefreshBoard();
            ads.ShowRewarded(value =>
            {
                if (matchId != id || gate != rewardGate) return;
                if (!match.activeSelf || applicationPaused || confirm.activeSelf || busy || board.CurrentPlayer != 1 || board.MoveCount != moveAtRequest)
                {
                    gate.Cancel();
                    RefreshBoard();
                    return;
                }
                bool awarded = gate.Complete(token, value);
                if (awarded) aiRoutine = StartCoroutine(Think(true));
                else if (!gate.Granted)
                {
                    SetMatchNotice(value == AdResult.Unavailable ? "client_hint_unavailable" : "hint_incomplete");
                    RefreshBoard();
                }
            });
            if (gate.Pending) StartCoroutine(HintDeadline(id, gate));
        }

        IEnumerator HintDeadline(string id, RewardGate gate)
        {
            float deadline = Time.realtimeSinceStartup + 6f;
            while (gate.Pending && matchId == id && Time.realtimeSinceStartup < deadline) yield return null;
            if (matchId == id && gate.Pending)
            {
                CancelPendingReward();
                SetMatchNotice("hint_timeout");
                RefreshBoard();
            }
        }

        void CancelPendingReward()
        {
            if (rewardGate == null || !rewardGate.Pending) return;
            rewardGate.Cancel();
            ads.CancelPending();
            if (matchNotice != null && strings != null) SetMatchNotice("hint_cancelled");
        }

        void Resume() { confirm.SetActive(false); RefreshBoard(); }
        void OpenSettings()
        {
            settings.SetActive(true);
            RefreshSettings();
        }
        void RefreshSettings()
        {
            soundLabel.text = T(progress.Data.soundEnabled ? "settings_sound_on" : "settings_sound_off");
            settingsDetail.text = T(progress.SaveWarning ?? "settings_detail");
            koreanLabel.text = strings.Language == "ko" ? F("language_selected", T("language_ko")) : T("language_ko");
            englishLabel.text = strings.Language == "en" ? F("language_selected", T("language_en")) : T("language_en");
        }
        public void SetLanguage(string language)
        {
            if (strings == null || progress == null) return;
            progress.Data.language = StringTable.NormalizeLanguage(language);
            strings.SetLanguage(progress.Data.language);
            progress.Save();
            RefreshLanguage();
        }
        void RefreshLanguage()
        {
            foreach (var label in localizedLabels) label.target.text = T(label.key);
            for (int col = 0; col < columnLabels.Length; col++) columnLabels[col].text = F("column_number", col + 1);
#if UNITY_WEBGL && !UNITY_EDITOR
            StageGameSetLanguage(strings.Language);
#endif
            var legalText = Resources.Load<TextAsset>("LegalNotices");
            noticesBody.text = legalText != null ? legalText.text : T("legal_missing");
            RefreshLobby();
            RefreshSettings();
            if (board != null)
            {
                matchTitle.text = F("match_title", (activeStage + 1).ToString("00"), data.Text(data.Stages[activeStage].titleKey));
                RefreshBoard();
                matchNotice.text = F(matchNoticeKey, matchNoticeArgs);
                RefreshResultText();
            }
        }
        void SetMatchNotice(string key, params object[] args)
        {
            matchNoticeKey = key; matchNoticeArgs = args;
            matchNotice.text = F(key, args);
        }
        void ToggleSound()
        {
            if (progress == null) return;
            progress.Data.soundEnabled = !progress.Data.soundEnabled;
            progress.Save(); OpenSettings();
        }
        void PlayMove() { if (progress.Data.soundEnabled && audioSource != null && moveSound != null) audioSource.PlayOneShot(moveSound, 0.3f); }
        void CancelSearch() { if (aiRoutine != null) StopCoroutine(aiRoutine); aiRoutine = null; }
        void OnApplicationPause(bool paused) { applicationPaused = paused; if (paused) { CancelPendingReward(); ads.CancelPending(); progress?.Save(); } RefreshBoard(); }
        void OnApplicationQuit() { leaving = true; progress?.Save(); }
        void OnDestroy() { CancelResultSequence(); if (!leaving) CancelSearch(); rewardGate?.Cancel(); }

#if UNITY_EDITOR
        public void SetEditorAds(AdResult value, bool duplicate)
        {
            ads = new EditorTestAds { Result = value, DuplicateCallback = duplicate };
        }
        public void SetAdsForTests(IAdService service) { ads.CancelPending(); ads = service; }
#endif
    }
}
