using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace StageGame
{
    public sealed class RankGameApp : MonoBehaviour
    {
        public RankTheme theme;
        public GameObject lobby, match, result, settings, help, confirm, notices;
        public RectTransform detail, boardView, normalBoardSlot, resultBoardSlot;
        public ScrollRect stageScroll;
        public RankStageCard[] cards;
        public Button multiplayer, lobbySettings, lobbyHelp, detailClose, previous, following, play;
        public Text lobbyTitle, lobbyNotice, multiplayerLabel, detailRank, detailNumber, detailStars, detailDescription, detailState, playLabel;
        public Image[] detailStones, stones;
        public Image movePreview, winLine, playerAvatar;
        public Button[] columns;
        public Text matchTitle, aiName, aiStars, aiSpeech, turnLabel, moveLabel, matchNotice;
        public Button pause, undo, place, matchRetry, hint;
        public Text hintLabel;
        public Button retry, nextStage, resultLobby;
        public Text resultTitle, resultAdStatus;
        public Button settingsClose, helpClose, confirmClose, confirmLeave, confirmResume, noticesOpen, noticesClose, soundToggle, korean, english;
        public Text soundLabel, koreanLabel, englishLabel, settingsDetail, noticesBody;
        public AudioSource audioSource;
        public AudioClip moveSound;
        public LocalizedLabel[] localizedLabels;

        GameData data;
        StringTable strings;
        ProgressStore progress;
        ConnectFourBoard board;
        IAdService ads = new NoFillAds();
        RewardGate rewardGate;
        readonly List<ConnectFourBoard> undoHistory = new List<ConnectFourBoard>();
        int selected, activeStage, selectedColumn = -1, searchGeneration;
        string matchId, noticeKey = "select_column";
        object[] noticeArgs = Array.Empty<object>();
        bool busy, applicationPaused, resultRecorded, resolving, winningRevealed, detailSliding;
        Coroutine aiRoutine, resultRoutine, slideRoutine;
        RectTransform animatedStone;
        Vector2 animatedPosition;
        int animatedSibling;
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] static extern void StageGameSetLanguage(string language);
#endif

        string T(string key) => strings.Text(key);
        string F(string key, params object[] args) => strings.Format(key, args);
        bool CanChoose => match.activeSelf && board != null && !board.IsFinished && !busy && !resolving && !applicationPaused && !confirm.activeSelf && !rewardGate.Pending && board.CurrentPlayer == 1;

        void Start()
        {
            for (int i = 0; i < cards.Length; i++) { int index = i; cards[i].button.onClick.AddListener(() => OpenDetail(index)); }
            previous.onClick.AddListener(() => SelectDetail(-1)); following.onClick.AddListener(() => SelectDetail(1));
            detailClose.onClick.AddListener(CloseDetail); play.onClick.AddListener(() => { if (play.interactable && !detailSliding) StartStage(selected); });
            lobbySettings.onClick.AddListener(OpenSettings); lobbyHelp.onClick.AddListener(() => help.SetActive(true));
            settingsClose.onClick.AddListener(() => settings.SetActive(false)); helpClose.onClick.AddListener(() => help.SetActive(false));
            noticesOpen.onClick.AddListener(() => notices.SetActive(true)); noticesClose.onClick.AddListener(() => notices.SetActive(false));
            soundToggle.onClick.AddListener(() => { progress.Data.soundEnabled = !progress.Data.soundEnabled; progress.Save(); RefreshSettings(); });
            korean.onClick.AddListener(() => SetLanguage("ko")); english.onClick.AddListener(() => SetLanguage("en"));
            pause.onClick.AddListener(OpenPause); confirmClose.onClick.AddListener(Resume); confirmResume.onClick.AddListener(Resume);
            confirmLeave.onClick.AddListener(ShowLobby); hint.onClick.AddListener(RequestHint);
            undo.onClick.AddListener(UndoTurn); place.onClick.AddListener(PlaceSelected);
            matchRetry.onClick.AddListener(() => { if (match.activeSelf && matchRetry.gameObject.activeSelf && matchRetry.interactable) StartStage(activeStage); });
            retry.onClick.AddListener(() => { if (result.activeSelf && retry.interactable) StartStage(activeStage); });
            nextStage.onClick.AddListener(() => { if (result.activeSelf && nextStage.interactable && nextStage.gameObject.activeSelf) StartStage(activeStage + 1); });
            resultLobby.onClick.AddListener(() => { if (result.activeSelf && resultLobby.interactable) ShowLobby(); });
            for (int i = 0; i < columns.Length; i++) { int column = i; columns[i].onClick.AddListener(() => SelectColumn(column)); }
            try
            {
                bool fresh = !PlayerPrefs.HasKey("stagegame.progress.v1");
                strings = StringTable.Load(); data = GameData.Load(strings); data.ValidateRankData(); progress = new ProgressStore(data.Stages.Length);
                strings.SetLanguage(progress.Data.language); selected = progress.Data.unlockedStage - 1;
                RefreshLanguage(); ShowLobby();
                if (fresh)
                {
                    progress.Save(); StartStage(0); SetNotice("select_column");
                }
            }
            catch (Exception error)
            {
                Debug.LogError("[Rank data] " + error);
                lobby.SetActive(true); match.SetActive(false); CloseOverlays();
                lobbyNotice.text = "데이터를 다시 불러와 주세요. / Please restart.";
                foreach (var card in cards) card.button.interactable = false;
                lobbySettings.interactable = lobbyHelp.interactable = false;
            }
        }

        void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Escape) || resolving || detailSliding) return;
            if (notices.activeSelf) notices.SetActive(false);
            else if (help.activeSelf) help.SetActive(false);
            else if (settings.activeSelf) settings.SetActive(false);
            else if (confirm.activeSelf) Resume();
            else if (result.activeSelf) { if (resultLobby.interactable) ShowLobby(); }
            else if (detail.gameObject.activeSelf) CloseDetail();
            else if (match.activeSelf) OpenPause();
        }

        void RefreshLobby()
        {
            lobbyTitle.text = T("game_title"); multiplayerLabel.text = T("multiplayer_start") + " · " + T("coming_soon");
            multiplayer.interactable = false;
            for (int i = 0; i < cards.Length; i++)
            {
                var stage = data.Stages[i]; var card = cards[i]; bool locked = stage.id > progress.Data.unlockedStage, cleared = stage.id <= progress.Data.highestClearedStage;
                card.rank.text = T(stage.rankKey); card.number.text = F("stage_label", stage.id.ToString("00")); card.stars.text = Stars(stage.difficultyStars);
                card.button.GetComponent<Image>().sprite = stage.id == progress.Data.unlockedStage ? theme.currentCard : locked ? theme.lockedCard : theme.panel;
                card.state.sprite = locked ? theme.locked : theme.check; card.state.gameObject.SetActive(locked || cleared);
            }
            string warning = progress.SaveWarning ?? progress.LoadWarning; lobbyNotice.text = warning == null ? "" : T(warning);
        }

        public void OpenDetail(int index)
        {
            if (data == null || index < 0 || index >= cards.Length || detailSliding || detail.gameObject.activeSelf) return;
            selected = index; RefreshDetail(); detail.gameObject.SetActive(true);
            slideRoutine = StartCoroutine(SlideDetail(true));
        }
        public void CloseDetail() { if (!detail.gameObject.activeSelf || detailSliding) return; slideRoutine = StartCoroutine(SlideDetail(false)); }
        IEnumerator SlideDetail(bool opening)
        {
            detailSliding = true; play.interactable = previous.interactable = following.interactable = detailClose.interactable = false;
            float elapsed = 0;
            while (elapsed < .22f)
            {
                float t = Mathf.SmoothStep(0, 1, elapsed / .22f);
                detail.anchoredPosition = new Vector2(Mathf.Lerp(opening ? 720 : 0, opening ? 0 : 720, t), 0);
                yield return null; elapsed += Time.unscaledDeltaTime;
            }
            detail.anchoredPosition = Vector2.zero; detailSliding = false; slideRoutine = null;
            if (!opening) detail.gameObject.SetActive(false); else RefreshDetail();
            detailClose.interactable = true;
        }
        void SelectDetail(int delta) { if (detailSliding) return; selected = Mathf.Clamp(selected + delta, 0, data.Stages.Length - 1); RefreshDetail(); }
        void RefreshDetail()
        {
            var stage = data.Stages[selected]; bool unlocked = stage.id <= progress.Data.unlockedStage;
            detailRank.text = T(stage.rankKey); detailStars.text = Stars(stage.difficultyStars); detailNumber.text = F("stage_label", stage.id.ToString("00"));
            detailDescription.text = T(stage.descriptionKey);
            detailState.text = unlocked ? "" : F("stage_lock_requirement", stage.id - 1);
            playLabel.text = T(unlocked ? "client_ui_start" : "stage_locked"); play.interactable = unlocked && !detailSliding;
            previous.interactable = selected > 0 && !detailSliding; following.interactable = selected + 1 < data.Stages.Length && !detailSliding;
            var opening = GameData.CreateBoard(stage);
            for (int i = 0; i < detailStones.Length; i++) SetStone(detailStones[i], opening[i % 7, i / 7], false);
        }

        public void ShowLobby()
        {
            CancelSearch(); CancelResultSequence(); CancelPendingReward(); ads.CancelPending(); matchId = null; busy = false;
            match.SetActive(false); lobby.SetActive(true); CloseOverlays();
            if (data != null) { selected = progress.Data.unlockedStage - 1; RefreshLobby(); }
        }
        void CloseOverlays()
        {
            result.SetActive(false); settings.SetActive(false); help.SetActive(false); confirm.SetActive(false); notices.SetActive(false);
            if (slideRoutine != null) StopCoroutine(slideRoutine); slideRoutine = null; detailSliding = false;
            detail.anchoredPosition = Vector2.zero; detail.gameObject.SetActive(false); detailClose.interactable = true;
        }
        public void StartStage(int index)
        {
            if (data == null || index < 0 || index >= data.Stages.Length || index + 1 > progress.Data.unlockedStage) return;
            CancelSearch(); CancelResultSequence(); CancelPendingReward(); ads.CancelPending();
            board = GameData.CreateBoard(data.Stages[index]); activeStage = selected = index; matchId = Guid.NewGuid().ToString("N");
            undoHistory.Clear(); rewardGate = new RewardGate(); resultRecorded = resolving = winningRevealed = busy = false; selectedColumn = -1;
            lobby.SetActive(false); match.SetActive(true); CloseOverlays();
            foreach (var button in new[] { pause, undo, place, matchRetry }) button.gameObject.SetActive(true);
            turnLabel.gameObject.SetActive(true); matchNotice.gameObject.SetActive(true);
            playerAvatar.gameObject.SetActive(true);
            boardView.anchoredPosition = normalBoardSlot.anchoredPosition; boardView.localScale = normalBoardSlot.localScale;
            winLine.gameObject.SetActive(false); SetNotice("select_column"); RefreshMatchHeader(); RefreshBoard();
            if (board.CurrentPlayer == -1) BeginThink(false);
        }

        public void SelectColumn(int column)
        {
            if (!CanChoose) return;
            selectedColumn = board.CanDrop(column) ? column : -1;
            SetNotice(selectedColumn < 0 ? "help_full_column" : "selected_column_accessibility", selectedColumn + 1);
            RefreshBoard();
        }
        public void PlaceSelected()
        {
            if (!CanChoose || !board.CanDrop(selectedColumn)) return;
            var before = board.Clone();
            if (!board.TryDrop(selectedColumn)) return;
            undoHistory.Add(before); selectedColumn = -1; SetNotice(""); PlayMove(); RefreshBoard();
            if (board.IsFinished) Finish(); else BeginThink(false);
        }
        public void UndoTurn()
        {
            if (!match.activeSelf || board == null || board.IsFinished || resolving || applicationPaused || confirm.activeSelf || undoHistory.Count == 0) return;
            CancelSearch(); CancelPendingReward();
            board = undoHistory[undoHistory.Count - 1]; undoHistory.RemoveAt(undoHistory.Count - 1);
            selectedColumn = -1; busy = false; SetNotice("undo_done"); RefreshBoard();
        }

        void BeginThink(bool isHint) { aiRoutine = StartCoroutine(Think(isHint, searchGeneration)); }
        IEnumerator Think(bool isHint, int generation)
        {
            busy = true; RefreshBoard(); string id = matchId;
            if (!isHint) yield return new WaitForSecondsRealtime(.25f);
            var stage = data.Stages[activeStage];
            using (var search = new AiSearch(board, isHint ? Math.Max(3, stage.aiDepth) : stage.aiDepth, stage.aiNodeBudget,
                isHint ? 0 : stage.aiRandomPermille, activeStage * 997 + board.MoveCount * 31))
            {
                double activeSeconds = 0; float lastTick = Time.realtimeSinceStartup;
                while (!search.Complete && activeSeconds < .65)
                {
                    if (generation != searchGeneration || id != matchId) yield break;
                    float now = Time.realtimeSinceStartup;
                    if (!applicationPaused && !confirm.activeSelf)
                    {
                        activeSeconds += now - lastTick; if (activeSeconds >= .65) break;
                        var timer = System.Diagnostics.Stopwatch.StartNew();
                        do { search.Step(16); } while (!search.Complete && timer.Elapsed.TotalSeconds < .004 && activeSeconds + timer.Elapsed.TotalSeconds < .65);
                    }
                    lastTick = now; yield return null;
                }
                while (applicationPaused || confirm.activeSelf) { if (generation != searchGeneration || id != matchId) yield break; yield return null; }
                if (generation != searchGeneration || id != matchId) yield break;
                if (isHint) { selectedColumn = search.ChosenColumn; SetNotice("hint_recommendation", selectedColumn + 1); }
                else if (board.TryDrop(search.ChosenColumn)) PlayMove();
            }
            busy = false; aiRoutine = null; RefreshBoard();
            if (!isHint && board.IsFinished) Finish();
        }

        void SetStone(Image image, int player, bool winning)
        {
            image.enabled = player != 0;
            image.sprite = player == 1 ? winning ? theme.blackWin : theme.blackStone : winning ? theme.whiteWin : theme.whiteStone;
            image.color = Color.white;
        }
        void RefreshBoard()
        {
            if (board == null) return;
            for (int i = 0; i < stones.Length; i++)
            {
                bool winning = false; foreach (int cell in board.WinningCells) if (cell == i) winning = true;
                SetStone(stones[i], board[i % 7, i / 7], winning && winningRevealed);
            }
            bool canChoose = CanChoose;
            foreach (var column in columns) column.interactable = canChoose;
            bool valid = canChoose && board.CanDrop(selectedColumn);
            movePreview.gameObject.SetActive(valid);
            if (valid) movePreview.rectTransform.anchoredPosition = stones[board.LandingRow(selectedColumn) * 7 + selectedColumn].rectTransform.anchoredPosition;
            place.interactable = valid;
            undo.interactable = undoHistory.Count > 0 && !board.IsFinished && !resolving && !applicationPaused && !confirm.activeSelf;
            pause.interactable = matchRetry.interactable = !board.IsFinished && !resolving && !applicationPaused;
            hint.interactable = board.CurrentPlayer == 1 && !busy && !board.IsFinished && !rewardGate.Pending && !rewardGate.Granted && !applicationPaused;
            hintLabel.text = T(rewardGate.Granted ? "client_hint_used" : "client_hint_request");
            turnLabel.text = T(board.IsFinished ? "turn_finished" : busy ? "turn_thinking" : board.CurrentPlayer == 1 ? "ui_player_turn" : "client_ui_ai_turn");
            moveLabel.text = F("move_count", board.MoveCount.ToString("00"));
        }
        void RefreshMatchHeader()
        {
            var stage = data.Stages[activeStage]; matchTitle.text = T("game_title"); aiName.text = T(stage.rankKey); aiStars.text = Stars(stage.difficultyStars);
            aiSpeech.text = T(board.IsFinished && board.Winner != 0 ? board.Winner == 1 ? stage.aiLoseKey : stage.aiWinKey : stage.aiIntroKey);
        }
        static string Stars(int count) => new string('★', count);

        void Finish()
        {
            if (resultRecorded || board == null || !board.IsFinished) return;
            resultRecorded = resolving = true; CancelPendingReward(); selectedColumn = -1;
            progress.RecordResult(matchId, activeStage + 1, board.Winner == 1, data.Stages.Length);
            SetResultActions(false); RefreshBoard(); resultRoutine = StartCoroutine(FinishSequence(matchId));
        }
        IEnumerator FinishSequence(string id)
        {
            if (board.Winner != 0)
            {
                animatedStone = stones[board.LastMove].rectTransform; animatedPosition = animatedStone.anchoredPosition;
                animatedSibling = animatedStone.GetSiblingIndex(); animatedStone.SetAsLastSibling();
                float elapsed = 0;
                while (elapsed < .4f && id == matchId)
                {
                    float ease = 1 - Mathf.Pow(1 - Mathf.Clamp01(elapsed / .4f), 3);
                    animatedStone.localScale = Vector3.one * Mathf.Lerp(1.45f, 1, ease);
                    animatedStone.anchoredPosition = animatedPosition + Vector2.up * Mathf.Lerp(34, 0, ease);
                    yield return null; if (!applicationPaused) elapsed += Time.unscaledDeltaTime;
                }
                RestoreAnimatedStone(); winningRevealed = true; RefreshBoard(); ShowWinLine(); elapsed = 0;
                while (elapsed < .5f && id == matchId) { yield return null; if (!applicationPaused) elapsed += Time.unscaledDeltaTime; }
            }
            if (id != matchId) yield break;
            foreach (var button in new[] { pause, undo, place, matchRetry }) button.gameObject.SetActive(false);
            turnLabel.gameObject.SetActive(false); matchNotice.gameObject.SetActive(false);
            playerAvatar.gameObject.SetActive(false);
            bool hasNext = board.Winner == 1 && activeStage + 1 < data.Stages.Length;
            nextStage.gameObject.SetActive(hasNext);
            retry.GetComponent<Image>().sprite = hasNext ? theme.panel : theme.primary;
            retry.GetComponentInChildren<Text>().color = hasNext ? new Color32(29, 30, 32, 255) : Color.white;
            boardView.anchoredPosition = hasNext ? resultBoardSlot.anchoredPosition : normalBoardSlot.anchoredPosition;
            boardView.localScale = hasNext ? resultBoardSlot.localScale : normalBoardSlot.localScale;
            RefreshResultText(); RefreshMatchHeader(); result.SetActive(true);
            yield return ResultAdBoundary(id); resolving = false; resultRoutine = null;
        }
        void ShowWinLine()
        {
            if (board.WinningCells.Count < 2) return;
            Vector2 a = Vector2.zero, b = Vector2.zero; float longest = -1;
            foreach (int x in board.WinningCells) foreach (int y in board.WinningCells)
            {
                int dx = y % 7 - x % 7, dy = y / 7 - x / 7;
                if (dx != 0 && dy != 0 && Math.Abs(dx) != Math.Abs(dy)) continue;
                int steps = Math.Max(Math.Abs(dx), Math.Abs(dy)); if (steps < 3) continue;
                int sx = Math.Sign(dx), sy = Math.Sign(dy); bool continuous = true;
                for (int i = 0; i <= steps; i++) if (board[x % 7 + sx * i, x / 7 + sy * i] != board.Winner) continuous = false;
                if (!continuous) continue;
                var p = stones[x].rectTransform.anchoredPosition; var q = stones[y].rectTransform.anchoredPosition;
                if ((q - p).sqrMagnitude > longest) { longest = (q - p).sqrMagnitude; a = p; b = q; }
            }
            if (longest < 0) return;
            winLine.rectTransform.anchoredPosition = (a + b) / 2;
            winLine.rectTransform.sizeDelta = new Vector2(Vector2.Distance(a, b), 5);
            winLine.rectTransform.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg);
            winLine.gameObject.SetActive(true);
        }
        void RefreshResultText()
        {
            if (board == null || !board.IsFinished) return;
            resultTitle.text = T(board.Winner == 1 ? "client_result_win" : board.IsDraw ? "result_draw" : "client_result_loss");
            resultAdStatus.text = progress.SaveWarning != null ? T(progress.SaveWarning) : "";
        }
        IEnumerator ResultAdBoundary(string id)
        {
            bool resolved = false; long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (AdPolicy.Eligible(data.Ads, progress.Data, now)) ads.ShowInterstitial(value =>
            {
                if (resolved || matchId != id || !result.activeSelf || applicationPaused) return;
                resolved = true;
                if (value == AdResult.Completed) { progress.Data.lastInterstitialMatch = progress.Data.completedMatches; progress.Data.lastInterstitialUtc = now; progress.Save(); }
            }); else resolved = true;
            float deadline = Time.realtimeSinceStartup + 4;
            while (!resolved && Time.realtimeSinceStartup < deadline && matchId == id) yield return null;
            resolved = true; ads.CancelPending(); if (matchId == id && result.activeSelf) SetResultActions(true);
        }
        void SetResultActions(bool enabled) { retry.interactable = nextStage.interactable = resultLobby.interactable = enabled; }
        void RestoreAnimatedStone()
        {
            if (animatedStone == null) return;
            animatedStone.localScale = Vector3.one; animatedStone.anchoredPosition = animatedPosition; animatedStone.SetSiblingIndex(animatedSibling); animatedStone = null;
        }
        void CancelResultSequence() { if (resultRoutine != null) StopCoroutine(resultRoutine); resultRoutine = null; resolving = false; RestoreAnimatedStone(); }
        void CancelSearch() { searchGeneration++; if (aiRoutine != null) StopCoroutine(aiRoutine); aiRoutine = null; busy = false; }

        void OpenPause()
        {
            if (!match.activeSelf || board == null || board.IsFinished || resolving) return;
            CancelPendingReward(); confirm.SetActive(true); RefreshBoard();
        }
        void Resume() { confirm.SetActive(false); RefreshBoard(); }
        void RequestHint()
        {
            if (!match.activeSelf || busy || applicationPaused || board == null || board.IsFinished || board.CurrentPlayer != 1 || !rewardGate.TryBegin(out int token)) return;
            confirm.SetActive(false); string id = matchId; var gate = rewardGate; int moves = board.MoveCount; RefreshBoard();
            ads.ShowRewarded(value =>
            {
                if (id != matchId || gate != rewardGate) return;
                if (!match.activeSelf || applicationPaused || confirm.activeSelf || busy || board.MoveCount != moves || board.CurrentPlayer != 1) { gate.Cancel(); RefreshBoard(); return; }
                if (gate.Complete(token, value)) BeginThink(true);
                else if (!gate.Granted) { SetNotice(value == AdResult.Unavailable ? "client_hint_unavailable" : "hint_incomplete"); RefreshBoard(); }
            });
            if (gate.Pending) StartCoroutine(HintDeadline(id, gate));
        }
        IEnumerator HintDeadline(string id, RewardGate gate)
        {
            float deadline = Time.realtimeSinceStartup + 6;
            while (gate.Pending && id == matchId && Time.realtimeSinceStartup < deadline) yield return null;
            if (id == matchId && gate.Pending) { CancelPendingReward(); SetNotice("hint_timeout"); RefreshBoard(); }
        }
        void CancelPendingReward() { if (rewardGate == null || !rewardGate.Pending) return; rewardGate.Cancel(); ads.CancelPending(); }
        void SetNotice(string key, params object[] args) { noticeKey = key; noticeArgs = args; matchNotice.text = key.Length == 0 ? "" : F(key, args); }
        void OpenSettings() { settings.SetActive(true); RefreshSettings(); }
        void RefreshSettings()
        {
            soundLabel.text = T(progress.Data.soundEnabled ? "settings_sound_on" : "settings_sound_off"); settingsDetail.text = T(progress.SaveWarning ?? "settings_detail");
            koreanLabel.text = strings.Language == "ko" ? F("language_selected", T("language_ko")) : T("language_ko");
            englishLabel.text = strings.Language == "en" ? F("language_selected", T("language_en")) : T("language_en");
        }
        public void SetLanguage(string language) { if (progress == null) return; progress.Data.language = StringTable.NormalizeLanguage(language); strings.SetLanguage(progress.Data.language); progress.Save(); RefreshLanguage(); }
        void RefreshLanguage()
        {
            foreach (var label in localizedLabels) label.target.text = T(label.key);
            noticesBody.text = Resources.Load<TextAsset>("LegalNotices")?.text ?? T("legal_missing");
            RefreshLobby(); RefreshSettings(); RefreshDetail();
#if UNITY_WEBGL && !UNITY_EDITOR
            StageGameSetLanguage(strings.Language);
#endif
            if (board != null) { RefreshMatchHeader(); RefreshBoard(); RefreshResultText(); matchNotice.text = noticeKey.Length == 0 ? "" : F(noticeKey, noticeArgs); }
        }
        void PlayMove() { if (progress.Data.soundEnabled) audioSource.PlayOneShot(moveSound, .3f); }
        void OnApplicationPause(bool paused) { applicationPaused = paused; if (paused) { CancelPendingReward(); ads.CancelPending(); progress?.Save(); } RefreshBoard(); }
        void OnApplicationQuit() { progress?.Save(); }
        void OnDestroy() { CancelSearch(); CancelResultSequence(); rewardGate?.Cancel(); }
#if UNITY_EDITOR
        public void SetAdsForTests(IAdService service) { ads.CancelPending(); ads = service; }
#endif
    }
}
