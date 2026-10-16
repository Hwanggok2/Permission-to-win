using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
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
        public ScrollRect matchScroll, detailScroll;
        public GameObject actionConfirm;
        public Button actionCancel, actionAccept;
        public Text actionQuestion, actionBody, actionAcceptLabel, turnToastLabel;
        public CanvasGroup turnToast;
        public Image aiFace, rankHat;
        public RankStageCard[] cards;
        public Button multiplayer, lobbySettings, lobbyHelp, detailClose, previous, following, play;
        public Text lobbyTitle, lobbyNotice, multiplayerLabel, detailRank, detailNumber, detailStars, detailDescription, detailState, playLabel;
        public Image[] detailStones, stones;
        public Image movePreview, winLine, playerAvatar;
        public Button[] columns;
        public Text matchTitle, aiName, aiStars, aiSpeech, turnLabel, moveLabel, matchNotice;
        public Text turnTimerLabel;
        public Slider turnTimerSlider;
        public Text aiRating;
        public Button pause, matchHelp, undo, place, matchRetry, hint;
        public Text hintLabel;
        public Button retry, nextStage, resultLobby;
        public Text resultTitle, resultAdStatus;
        public Button settingsClose, helpClose, confirmClose, confirmLeave, confirmResume, noticesOpen, noticesClose, soundToggle, korean, english;
        public Text soundLabel, koreanLabel, englishLabel, settingsDetail, noticesBody;
        public AudioSource audioSource;
        public AudioClip moveSound;
        public AudioClip buttonSound;
        public AudioClip typingSound;
        public Button[] soundButtons;
        public AudioSource musicSource;
        public Button musicToggle;
        public Text musicLabel;
        public LocalizedLabel[] localizedLabels;

        GameData data;
        StringTable strings;
        ProgressStore progress;
        ConnectFourBoard board;
        IAdService ads = new NoFillAds();
        RewardGate rewardGate;
        readonly List<ConnectFourBoard> undoHistory = new List<ConnectFourBoard>();
        int selected, activeStage, selectedColumn = -1, searchGeneration, openingMoveCount, matchAiSeed;
        string matchId, noticeKey = "select_column";
        object[] noticeArgs = Array.Empty<object>();
        bool busy, applicationPaused, resultRecorded, resolving, winningRevealed, detailSliding;
        Coroutine aiRoutine, resultRoutine, slideRoutine;
        Coroutine toastRoutine;
        static bool sessionMatchStarted;
        bool moveGuidePending;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetSessionGuide() => sessionMatchStarted = false;
        enum PendingAction { None, Retry, Lobby }
        PendingAction pendingAction;
        bool destroyed;
        bool characterThinking;
        bool applicationFocused = true, musicStarted, musicPaused, interstitialPending;
        int adGeneration, rewardGeneration;
        string speechText;
        int[] speechOffsets;
        int speechVisible;
        float speechElapsed;
        const float SpeechCharacterSeconds=.04f;
        public const float PlayerTurnSeconds=30;
        float turnSecondsRemaining;
        double lastTurnTick;
        bool turnTimerWasRunning;
        static readonly int[] TimeoutColumns={3,2,4,1,5,0,6};
        double suspensionBegan=-1,suspendedSeconds;
        double ActiveTime()
        {
            double now=Time.realtimeSinceStartup;
            if(Suspended){if(suspensionBegan<0)suspensionBegan=now;}
            else if(suspensionBegan>=0){suspendedSeconds+=now-suspensionBegan;suspensionBegan=-1;}
            return now-suspendedSeconds-(suspensionBegan>=0?now-suspensionBegan:0);
        }
        bool Suspended => applicationPaused || confirm.activeSelf || actionConfirm.activeSelf || help.activeSelf;
        RectTransform animatedStone;
        Vector2 animatedPosition;
        int animatedSibling;
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] static extern void StageGameSetLanguage(string language);
#endif

        static float U(float value)=>RankUiMetrics.U(value);
        string T(string key) => strings.Text(key);
        string F(string key, params object[] args) => strings.Format(key, args);
        bool CanChoose => !destroyed && match.activeSelf && board != null && !board.IsFinished && !busy && !resolving && !Suspended && !rewardGate.Pending && board.CurrentPlayer == 1;

        void Start()
        {
            // Register before navigation handlers so closing a screen still produces one click.
            foreach(var button in soundButtons) { var target=button;target.onClick.AddListener(()=>PlayButtonSound(target)); }
            for (int i = 0; i < cards.Length; i++) { int index = i; cards[i].button.onClick.AddListener(() => OpenDetail(index)); }
            previous.onClick.AddListener(() => SelectDetail(-1)); following.onClick.AddListener(() => SelectDetail(1));
            detailClose.onClick.AddListener(CloseDetail); play.onClick.AddListener(() => { if (play.interactable && !detailSliding) StartStage(selected); });
            lobbySettings.onClick.AddListener(OpenSettings); lobbyHelp.onClick.AddListener(OpenHelp);
            settingsClose.onClick.AddListener(() => settings.SetActive(false)); helpClose.onClick.AddListener(CloseHelp);
            noticesOpen.onClick.AddListener(() => notices.SetActive(true)); noticesClose.onClick.AddListener(() => notices.SetActive(false));
            soundToggle.onClick.AddListener(() => { progress.Data.soundEnabled = !progress.Data.soundEnabled; progress.Save(); RefreshSettings(); if(progress.Data.soundEnabled)PlayButtonSound(soundToggle); });
            musicToggle.onClick.AddListener(() => { progress.Data.musicMuted = !progress.Data.musicMuted; progress.Save(); RefreshSettings(); RefreshMusic(); });
            korean.onClick.AddListener(() => SetLanguage("ko")); english.onClick.AddListener(() => SetLanguage("en"));
            pause.onClick.AddListener(OpenPause); confirmClose.onClick.AddListener(Resume); confirmResume.onClick.AddListener(Resume);
            matchHelp.onClick.AddListener(() => { if (matchHelp.IsActive() && matchHelp.interactable) OpenHelp(); });
            confirmLeave.onClick.AddListener(() => QueueAction(PendingAction.Lobby)); hint.onClick.AddListener(RequestHint);
            actionCancel.onClick.AddListener(CancelAction); actionAccept.onClick.AddListener(AcceptAction);
            undo.onClick.AddListener(UndoTurn); place.onClick.AddListener(PlaceSelected);
            matchRetry.onClick.AddListener(() => { if (match.activeSelf && matchRetry.gameObject.activeSelf && matchRetry.interactable) QueueAction(PendingAction.Retry); });
            retry.onClick.AddListener(() => { if (result.activeSelf && retry.interactable) QueueAction(PendingAction.Retry); });
            nextStage.onClick.AddListener(() => { if (result.activeSelf && nextStage.interactable && nextStage.gameObject.activeSelf) StartStage(activeStage + 1); });
            resultLobby.onClick.AddListener(() => { if (result.activeSelf && resultLobby.interactable) QueueAction(PendingAction.Lobby); });
            for (int i = 0; i < columns.Length; i++) { int column = i; columns[i].onClick.AddListener(() => SelectColumn(column)); }
            try
            {
                bool fresh = !PlayerPrefs.HasKey("stagegame.progress.v1");
                strings = StringTable.Load(); data = GameData.Load(strings); data.ValidateRankData(); progress = new ProgressStore(data.Stages.Length);
                strings.SetLanguage(progress.Data.language); selected = progress.Data.unlockedStage - 1;
                RefreshLanguage(); ShowLobby();
                if (fresh)
                {
                    progress.Save(); StartStage(0, true);
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
            RefreshMusic();
            AdvanceSpeech();
            TickTurnTimer();
            if (!Input.GetKeyDown(KeyCode.Escape) || resolving || detailSliding) return;
            if (actionConfirm.activeSelf) CancelAction();
            else if (notices.activeSelf) notices.SetActive(false);
            else if (help.activeSelf) CloseHelp();
            else if (settings.activeSelf) settings.SetActive(false);
            else if (confirm.activeSelf) Resume();
            else if (result.activeSelf) { if (resultLobby.interactable) QueueAction(PendingAction.Lobby); }
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
                card.rank.text = T(stage.rankKey); card.number.text = F("stage_label", stage.id.ToString("00")); SetStars(card.stars,stage.difficultyStars);
                card.rating.text=F("ai_rating",stage.aiRating);
                card.button.GetComponent<Image>().sprite = stage.id == progress.Data.unlockedStage ? theme.currentCard : locked ? theme.lockedCard : theme.panel;
                card.state.sprite = locked ? theme.locked : theme.check; card.state.gameObject.SetActive(locked || cleared);
            }
            string warning = progress.SaveWarning ?? progress.LoadWarning; lobbyNotice.text = warning == null ? "" : T(warning);
        }

        public void OpenDetail(int index)
        {
            if (data == null || index < 0 || index >= cards.Length || detailSliding || detail.gameObject.activeSelf) return;
            selected = index; RefreshDetail(); detail.gameObject.SetActive(true); detailScroll.verticalNormalizedPosition=1;
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
                detail.anchoredPosition = new Vector2(Mathf.Lerp(opening ? U(720) : 0, opening ? 0 : U(720), t), 0);
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
            detailRank.text = T(stage.rankKey); SetStars(detailStars,stage.difficultyStars); detailNumber.text = F("stage_label", stage.id.ToString("00"))+" · "+F("ai_rating",stage.aiRating);
            detailDescription.text = T(stage.descriptionKey);
            detailState.text = unlocked ? "" : F("stage_lock_requirement", stage.id - 1);
            playLabel.text = T(unlocked ? "client_ui_start" : "stage_locked"); play.interactable = unlocked && !detailSliding;
            previous.interactable = selected > 0 && !detailSliding; following.interactable = selected + 1 < data.Stages.Length && !detailSliding;
            var opening = GameData.CreateBoard(stage);
            for (int i = 0; i < detailStones.Length; i++) SetStone(detailStones[i], opening[i % 7, i / 7], false);
        }

        public void ShowLobby()
        {
            CancelSpeech();
            CancelSearch(); CancelResultSequence(); CancelPendingReward(); ads.CancelPending(); matchId = null; busy = false;
            match.SetActive(false); lobby.SetActive(true); CloseOverlays();
            if (data != null) { selected = progress.Data.unlockedStage - 1; RefreshLobby(); }
            RefreshMusic();
        }
        void CloseOverlays()
        {
            CancelToast(); pendingAction=PendingAction.None; actionConfirm.SetActive(false);
            result.SetActive(false); settings.SetActive(false); help.SetActive(false); confirm.SetActive(false); notices.SetActive(false);
            if (slideRoutine != null) StopCoroutine(slideRoutine); slideRoutine = null; detailSliding = false;
            detail.anchoredPosition = Vector2.zero; detail.gameObject.SetActive(false); detailClose.interactable = true; ActiveTime();
        }
        public void StartStage(int index) => StartStage(index, false);
        void StartStage(int index, bool showRules)
        {
            if (data == null || index < 0 || index >= data.Stages.Length || index + 1 > progress.Data.unlockedStage) return;
            moveGuidePending = !sessionMatchStarted; sessionMatchStarted = true;
            CancelSpeech();
            CancelSearch(); CancelResultSequence(); CancelPendingReward(); ads.CancelPending();
            board = GameData.CreateBoard(data.Stages[index]); activeStage = selected = index; matchId = Guid.NewGuid().ToString("N");
            openingMoveCount = board.MoveCount;
            matchAiSeed=UnityEngine.Random.Range(1,int.MaxValue);
            ResetTurnTimer();
            undoHistory.Clear(); rewardGate = new RewardGate(); resultRecorded = resolving = winningRevealed = busy = false; selectedColumn = -1;
            characterThinking = false;
            lobby.SetActive(false); match.SetActive(true); CloseOverlays();
            foreach (var button in new[] { pause, matchHelp, undo, place, matchRetry }) button.gameObject.SetActive(true);
            turnLabel.gameObject.SetActive(false); matchNotice.gameObject.SetActive(true);
            playerAvatar.gameObject.SetActive(false);
            matchScroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,U(1170));matchScroll.verticalNormalizedPosition=1;
            boardView.anchoredPosition = normalBoardSlot.anchoredPosition; boardView.localScale = normalBoardSlot.localScale;
            winLine.gameObject.SetActive(false); SetNotice("select_column"); RefreshMatchHeader(); RefreshBoard();
            if (showRules) OpenHelp();
            if (board.CurrentPlayer == -1) BeginThink(false); else ShowTurnToast();
            RefreshMusic();
        }

        public void SelectColumn(int column)
        {
            if (!CanChoose) return;
            CancelToast();
            selectedColumn = board.CanDrop(column) ? column : -1;
            SetNotice(selectedColumn < 0 ? "help_full_column" : "");
            RefreshBoard();
        }
        public void PlaceSelected()
        {
            if (!CanChoose || !board.CanDrop(selectedColumn)) return;
            var before = board.Clone();
            if (!board.TryDrop(selectedColumn)) return;
            characterThinking = true;
            CancelToast();
            undoHistory.Add(before); selectedColumn = -1; SetNotice(""); PlayMove(); RefreshBoard();
            if (board.IsFinished) Finish(); else BeginThink(false);
        }
        public void UndoTurn()
        {
            if (!match.activeSelf || board == null || board.IsFinished || resolving || Suspended || undoHistory.Count == 0) return;
            CancelSearch(); CancelPendingReward();
            board = undoHistory[undoHistory.Count - 1]; undoHistory.RemoveAt(undoHistory.Count - 1);
            selectedColumn = -1; busy = false; SetNotice("undo_done"); RefreshBoard(); ShowTurnToast();
        }

        void BeginThink(bool isHint) { aiRoutine = StartCoroutine(Think(isHint, searchGeneration)); }
        AiSearch CreateSearch(bool isHint)
        {
            var stage = data.Stages[activeStage];
            return new AiSearch(board, isHint ? Math.Max(3, stage.aiDepth) : stage.aiDepth, stage.aiNodeBudget,
                isHint ? 0 : stage.aiRandomPermille, unchecked(matchAiSeed + board.MoveCount * 31),
                oversightPermille: isHint ? 0 : stage.aiOversightPermille);
        }
        IEnumerator Think(bool isHint, int generation)
        {
            if (!isHint) characterThinking = true;
            busy = true; RefreshBoard(); string id = matchId;
            double started=ActiveTime();
            using (var search = CreateSearch(isHint))
            {
                while(true)
                {
                    if(destroyed || generation!=searchGeneration || id!=matchId)yield break;
                    double elapsed=ActiveTime()-started;
                    if(!Suspended)
                    {
                        if(!search.Complete && elapsed<.65)
                        {
                            var timer=System.Diagnostics.Stopwatch.StartNew();
                            do{search.Step(16);}while(!search.Complete && timer.Elapsed.TotalSeconds<.004 && elapsed+timer.Elapsed.TotalSeconds<.65);
                        }
                        // Search and presentation time overlap. Hints use only the search deadline.
                        if((search.Complete || elapsed>=.65) && (isHint || elapsed>=1.0))break;
                    }
                    yield return null;
                }
                if (isHint) { selectedColumn = search.ChosenColumn; SetNotice("hint_recommendation", selectedColumn + 1); }
                else if (board.TryDrop(search.ChosenColumn)) PlayMove();
            }
            busy = false; aiRoutine = null; RefreshBoard();
            if (!isHint && board.IsFinished) Finish(); else if (!isHint) ShowTurnToast();
        }

        void SetStone(Image image, int player, bool winning)
        {
            image.enabled = player != 0;
            image.sprite = player == 1 ? winning ? theme.blackWin : theme.blackStone : winning ? theme.whiteWin : theme.whiteStone;
            image.color = Color.white;
        }
        bool CanRunTurnTimer => CanChoose && applicationFocused && !settings.activeSelf && !help.activeSelf && !notices.activeSelf && !interstitialPending;
        void ResetTurnTimer()
        {
            turnSecondsRemaining=PlayerTurnSeconds;
            lastTurnTick=Time.realtimeSinceStartupAsDouble;turnTimerWasRunning=false;
        }
        void TickTurnTimer()
        {
            double now=Time.realtimeSinceStartupAsDouble;
            bool running=CanRunTurnTimer;
            if(running && turnTimerWasRunning)AdvanceTurnTimer((float)(now-lastTurnTick));
            lastTurnTick=now;turnTimerWasRunning=running;
            RefreshTurnTimer();
        }
        void AdvanceTurnTimer(float seconds)
        {
            if(!CanRunTurnTimer || seconds<=0)return;
            turnSecondsRemaining=Mathf.Max(0,turnSecondsRemaining-seconds);
            if(turnSecondsRemaining>0)return;
            if(!board.CanDrop(selectedColumn))
                foreach(int column in TimeoutColumns)if(board.CanDrop(column)){selectedColumn=column;break;}
            int placedColumn=selectedColumn;
            PlaceSelected();
            if(!board.IsFinished)SetNotice("turn_timeout_placed",placedColumn+1);
        }
        void RefreshTurnTimer()
        {
            bool visible=match.activeSelf && board!=null && !board.IsFinished && !resolving && board.CurrentPlayer==1;
            turnTimerLabel.transform.parent.gameObject.SetActive(visible);
            if(!visible)return;
            turnTimerLabel.text=F("turn_timer_seconds",Mathf.CeilToInt(turnSecondsRemaining));
            turnTimerLabel.color=turnSecondsRemaining<=5 ? new Color32(161,40,34,255) : new Color32(29,30,32,255);
            turnTimerSlider.SetValueWithoutNotify(turnSecondsRemaining/PlayerTurnSeconds);
            turnTimerSlider.fillRect.GetComponent<Image>().color=turnSecondsRemaining<=5 ? new Color32(161,40,34,255) : new Color32(76,91,47,255);
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
            undo.interactable = undoHistory.Count > 0 && !board.IsFinished && !resolving && !Suspended;
            pause.interactable = matchRetry.interactable = !board.IsFinished && !resolving && !applicationPaused && !actionConfirm.activeSelf && !help.activeSelf;
            matchHelp.interactable = !board.IsFinished && !resolving && !Suspended && !rewardGate.Pending;
            undo.GetComponentInChildren<Text>().color=undo.interactable?new Color32(29,30,32,255):new Color(.55f,.55f,.55f);
            matchRetry.GetComponentInChildren<Text>().color=matchRetry.interactable?new Color32(29,30,32,255):new Color(.55f,.55f,.55f);
            hint.interactable = board.CurrentPlayer == 1 && !busy && !board.IsFinished && !rewardGate.Pending && !rewardGate.Granted && !applicationPaused && !actionConfirm.activeSelf && !help.activeSelf;
            hintLabel.text = T(rewardGate.Granted ? "client_hint_used" : "client_hint_request");
            turnLabel.text = T(board.IsFinished ? "turn_finished" : busy ? "turn_thinking" : board.CurrentPlayer == 1 ? "ui_player_turn" : "client_ui_ai_turn");
            moveLabel.text = F("move_count", board.MoveCount.ToString("00"));
            RefreshPortrait();
            RefreshTurnTimer();
        }
        void RefreshMatchHeader()
        {
            var stage = data.Stages[activeStage]; matchTitle.text = T("game_title"); aiName.text = T(stage.rankKey); SetStars(aiStars,stage.difficultyStars);
            aiRating.text=F("ai_rating",stage.aiRating);
            SetSpeech(T(board.IsFinished && board.Winner != 0 ? board.Winner == 1 ? stage.aiLoseKey : stage.aiWinKey : stage.aiIntroKey));
            if (theme.Portrait(stage.characterId) != null)
            {
                rankHat.gameObject.SetActive(false);
                RefreshPortrait();
                return;
            }
            rankHat.sprite=theme.rankHats[(stage.id-1)/3];
            float nameWidth=Mathf.Min(U(260),aiName.preferredWidth+U(8)),starsWidth=U(256);
            float total=U(92+28+20)+nameWidth+starsWidth,left=-total/2;
            aiFace.rectTransform.anchoredPosition=new Vector2(left+U(46),0);rankHat.rectTransform.anchoredPosition=new Vector2(left+U(46),U(40));
            aiName.rectTransform.sizeDelta=new Vector2(nameWidth,U(100));aiName.rectTransform.anchoredPosition=new Vector2(left+U(120)+nameWidth/2,0);
            aiStars.rectTransform.sizeDelta=new Vector2(starsWidth,U(50));aiStars.rectTransform.anchoredPosition=new Vector2(left+U(140)+nameWidth+starsWidth/2,0);
        }
        static void SetStars(Text label,float value) => label.GetComponent<RankStarRating>().SetValue(value);

        void SetSpeech(string text)
        {
            if(!match.activeInHierarchy)return;
            if(speechText==text){RenderSpeech();return;}
            speechText=text;speechOffsets=StringInfo.ParseCombiningCharacters(text);
            speechVisible=0;speechElapsed=0;RenderSpeech();
        }
        void AdvanceSpeech()
        {
            if(speechOffsets==null || speechVisible>=speechOffsets.Length || destroyed ||
               !match.activeInHierarchy || !aiSpeech.isActiveAndEnabled || Suspended || !applicationFocused ||
               settings.activeSelf || help.activeSelf || notices.activeSelf || interstitialPending ||
               (rewardGate!=null && rewardGate.Pending))return;
            speechElapsed+=Time.unscaledDeltaTime;
            if(speechElapsed<SpeechCharacterSeconds)return;
            // Reveal at most one text element per frame: no burst of queued clicks after a stall.
            speechElapsed=0;int offset=speechOffsets[speechVisible++];RenderSpeech();
            if(progress.Data.soundEnabled && !char.IsWhiteSpace(speechText,offset))
                audioSource.PlayOneShot(typingSound,.75f);
        }
        void RenderSpeech()
        {
            int end=speechVisible<speechOffsets.Length?speechOffsets[speechVisible]:speechText.Length;
            // The transparent remainder keeps wrapping and best-fit size stable while typing.
            aiSpeech.text=end==speechText.Length?speechText:
                speechText.Substring(0,end)+"<color=#00000000>"+speechText.Substring(end)+"</color>";
        }
        void CancelSpeech()
        {
            if(speechText!=null)aiSpeech.text=speechText;
            speechText=null;speechOffsets=null;speechVisible=0;speechElapsed=0;
        }

        void RefreshPortrait()
        {
            if (data == null || board == null) return;
            var stage = data.Stages[activeStage];
            var portrait = theme.Portrait(stage.characterId, stage.id >= 25);
            if (portrait == null) return;
            aiFace.sprite = board.IsFinished && board.Winner == 1 ? portrait.defeat :
                characterThinking || board.IsFinished ? portrait.thinking : portrait.intro;
            aiFace.preserveAspect = true;
        }

        void Finish()
        {
            if (resultRecorded || board == null || !board.IsFinished) return;
            resultRecorded = resolving = true; CancelPendingReward(); selectedColumn = -1;
            CancelToast();
            // A top-row winning stone rises into this status band during its placement animation.
            turnLabel.gameObject.SetActive(false);
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
                    animatedStone.anchoredPosition = animatedPosition + Vector2.up * Mathf.Lerp(U(34), 0, ease);
                    yield return null; if (!applicationPaused) elapsed += Time.unscaledDeltaTime;
                }
                RestoreAnimatedStone(); winningRevealed = true; RefreshBoard(); ShowWinLine(); elapsed = 0;
                while (elapsed < .5f && id == matchId) { yield return null; if (!applicationPaused) elapsed += Time.unscaledDeltaTime; }
            }
            if (id != matchId) yield break;
            foreach (var button in new[] { pause, matchHelp, undo, place, matchRetry }) button.gameObject.SetActive(false);
            turnLabel.gameObject.SetActive(false); matchNotice.gameObject.SetActive(false);
            playerAvatar.gameObject.SetActive(false);
            bool hasNext = board.Winner == 1 && activeStage + 1 < data.Stages.Length;
            nextStage.gameObject.SetActive(hasNext);
            retry.GetComponent<Image>().sprite = hasNext ? theme.panel : theme.primary;
            retry.GetComponentInChildren<Text>().color = hasNext ? new Color32(29, 30, 32, 255) : Color.white;
            boardView.anchoredPosition = normalBoardSlot.anchoredPosition; boardView.localScale=Vector3.one;
            RefreshResultText(); RefreshMatchHeader(); result.SetActive(true);
            yield return ResultAdBoundary(id); RefreshResultText(); resolving = false; resultRoutine = null;
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
            winLine.rectTransform.sizeDelta = new Vector2(Vector2.Distance(a, b), U(5));
            winLine.rectTransform.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg);
            winLine.gameObject.SetActive(true);
        }
        void RefreshResultText()
        {
            if (board == null || !board.IsFinished) return;
            resultTitle.text = T(board.Winner == 1 ? "result_win" : board.IsDraw ? "result_draw" : "client_result_loss");
            if (board.Winner == 1 && activeStage == data.Stages.Length - 1) resultTitle.text = T("story_champion");
            nextStage.GetComponentInChildren<Text>().text = T(activeStage == 23 ? "action_to_round_64" : "nextstage");
            resultAdStatus.text = progress.SaveWarning != null ? T(progress.SaveWarning) : "";
            bool hasNext=board.Winner==1&&activeStage+1<data.Stages.Length;float warning=progress.SaveWarning==null?0:55;
            resultAdStatus.gameObject.SetActive(warning>0);
            nextStage.GetComponent<RectTransform>().anchoredPosition=RankUiMetrics.Point(0,-1185-warning);
            retry.GetComponent<RectTransform>().anchoredPosition=RankUiMetrics.Point(-174,(hasNext?-1310:-1185)-warning);
            resultLobby.GetComponent<RectTransform>().anchoredPosition=RankUiMetrics.Point(174,(hasNext?-1310:-1185)-warning);
            matchScroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,U((hasNext?1385:1250)+warning));
        }
        IEnumerator ResultAdBoundary(string id)
        {
            bool resolved = false; int generation=adGeneration; long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (AdPolicy.Eligible(data.Ads, progress.Data, now))
            {
                interstitialPending = true; RefreshMusic();
                ads.ShowInterstitial(value =>
                {
                    if (destroyed || generation!=adGeneration || resolved || matchId != id || !result.activeSelf || Suspended) return;
                    resolved = true; interstitialPending = false; RefreshMusic();
                    if (value == AdResult.Completed) { progress.Data.lastInterstitialMatch = progress.Data.completedMatches; progress.Data.lastInterstitialUtc = now; progress.Save(); }
                });
            }
            else resolved = true;
            float deadline = Time.realtimeSinceStartup + 4;
            while (!resolved && Time.realtimeSinceStartup < deadline && matchId == id) yield return null;
            resolved = true; interstitialPending = false; ads.CancelPending(); RefreshMusic(); if (!destroyed && generation==adGeneration && matchId == id && result.activeSelf) { resolving=false;SetResultActions(true); }
        }
        void SetResultActions(bool enabled) { retry.interactable = nextStage.interactable = resultLobby.interactable = enabled; }
        void RestoreAnimatedStone()
        {
            if (animatedStone == null) return;
            animatedStone.localScale = Vector3.one; animatedStone.anchoredPosition = animatedPosition; animatedStone.SetSiblingIndex(animatedSibling); animatedStone = null;
        }
        void CancelResultSequence() { adGeneration++; interstitialPending = false; if (resultRoutine != null) StopCoroutine(resultRoutine); resultRoutine = null; resolving = false; RestoreAnimatedStone(); }
        void CancelSearch() { searchGeneration++; if (aiRoutine != null) StopCoroutine(aiRoutine); aiRoutine = null; busy = false; }

        void QueueAction(PendingAction action)
        {
            if(destroyed || pendingAction!=PendingAction.None || !match.activeSelf || board==null || resolving || applicationPaused || help.activeSelf) return;
            pendingAction=action; CancelToast(); CancelPendingReward(); actionConfirm.SetActive(true); ActiveTime(); RefreshActionText(); RefreshBoard();
            RefreshMusic();
        }
        void RefreshActionText()
        {
            bool restart=pendingAction==PendingAction.Retry;
            actionQuestion.text=T(restart?"confirm_retry_title":"confirm_exit_title");
            actionBody.text=T(restart?"confirm_retry_body":"confirm_exit_body");
            actionAcceptLabel.text=T(restart?"retry":"confirm_leave");
        }
        void CancelAction()
        {
            if(pendingAction==PendingAction.None)return;
            pendingAction=PendingAction.None;actionConfirm.SetActive(false);ActiveTime();RefreshBoard();
            RefreshMusic();
        }
        void AcceptAction()
        {
            if(destroyed || pendingAction==PendingAction.None || !actionConfirm.activeSelf)return;
            var action=pendingAction;pendingAction=PendingAction.None;actionConfirm.SetActive(false);confirm.SetActive(false);ActiveTime();
            if(action==PendingAction.Retry)StartStage(activeStage);else ShowLobby();
        }
        void CancelToast()
        {
            if(toastRoutine!=null)StopCoroutine(toastRoutine);toastRoutine=null;
            if(turnToast!=null){turnToast.alpha=0;turnToast.gameObject.SetActive(false);}
        }
        void ShowTurnToast()
        {
            ResetTurnTimer();
            ShowMoveGuide();
        }
        void ShowMoveGuide()
        {
            // This flag belongs to the first match of the process, not the progress save or each turn.
            // Rules can defer it until the first player turn becomes available.
            if (!moveGuidePending || !CanChoose) return;
            moveGuidePending = false;
            CancelToast();
            toastRoutine=StartCoroutine(TurnToastSequence());
        }
        IEnumerator TurnToastSequence()
        {
            turnToastLabel.text=T("select_column");turnToast.gameObject.SetActive(true);turnToast.alpha=1;
            float elapsed=0;
            while(elapsed<3f){turnToast.alpha=elapsed<2.6f?1:1-(elapsed-2.6f)/.4f;yield return null;elapsed+=Time.unscaledDeltaTime;}
            turnToast.alpha=0;turnToast.gameObject.SetActive(false);toastRoutine=null;
        }

        void OpenPause()
        {
            if (!match.activeSelf || board == null || board.IsFinished || resolving || actionConfirm.activeSelf || help.activeSelf) return;
            CancelToast(); CancelPendingReward(); confirm.SetActive(true); ActiveTime(); RefreshBoard();
            RefreshMusic();
        }
        void Resume() { confirm.SetActive(false); ActiveTime(); RefreshBoard(); RefreshMusic(); }
        void RequestHint()
        {
            if (destroyed || actionConfirm.activeSelf || help.activeSelf || !match.activeSelf || busy || applicationPaused || board == null || board.IsFinished || board.CurrentPlayer != 1 || !rewardGate.TryBegin(out int token)) return;
            confirm.SetActive(false); ActiveTime(); string id = matchId; var gate = rewardGate; int moves = board.MoveCount; int generation=++rewardGeneration; RefreshBoard();
            RefreshMusic();
            ads.ShowRewarded(value =>
            {
                if (destroyed || generation!=rewardGeneration || id != matchId || gate != rewardGate || !gate.Pending) return;
                if (!match.activeSelf || Suspended || busy || board.MoveCount != moves || board.CurrentPlayer != 1) { gate.Cancel(); RefreshBoard(); RefreshMusic(); return; }
                if (gate.Complete(token, value)) BeginThink(true);
                else if (!gate.Granted) { SetNotice(value == AdResult.Unavailable ? "client_hint_unavailable" : "hint_incomplete"); RefreshBoard(); }
                RefreshMusic();
            });
            if (gate.Pending) StartCoroutine(HintDeadline(id, gate));
        }
        IEnumerator HintDeadline(string id, RewardGate gate)
        {
            float deadline = Time.realtimeSinceStartup + 6;
            while (gate.Pending && id == matchId && Time.realtimeSinceStartup < deadline) yield return null;
            if (id == matchId && gate.Pending) { CancelPendingReward(); SetNotice("hint_timeout"); RefreshBoard(); }
        }
        void CancelPendingReward() { if (rewardGate == null || !rewardGate.Pending) return; rewardGeneration++; rewardGate.Cancel(); ads.CancelPending(); }
        void SetNotice(string key, params object[] args)
        {
            noticeKey=key;noticeArgs=args;matchNotice.text=key.Length==0?"":F(key,args);
            // The placement guide is shown centrally; keep other contextual notices in their saved slot.
            matchNotice.gameObject.SetActive(key.Length>0 && key!="select_column");
        }
        void OpenHelp() { CancelToast();help.SetActive(true);ActiveTime();RefreshBoard(); }
        void CloseHelp() { help.SetActive(false);ActiveTime();RefreshBoard();ShowMoveGuide(); }
        void OpenSettings() { settings.SetActive(true); RefreshSettings(); }
        void RefreshSettings()
        {
            soundLabel.text = T(progress.Data.soundEnabled ? "settings_sound_on" : "settings_sound_off"); settingsDetail.text = T(progress.SaveWarning ?? "settings_detail");
            musicLabel.text = T(progress.Data.musicMuted ? "settings_music_off" : "settings_music_on");
            koreanLabel.text = strings.Language == "ko" ? F("language_selected", T("language_ko")) : T("language_ko");
            englishLabel.text = strings.Language == "en" ? F("language_selected", T("language_en")) : T("language_en");
        }
        public void SetLanguage(string language) { if (progress == null) return; progress.Data.language = StringTable.NormalizeLanguage(language); strings.SetLanguage(progress.Data.language); progress.Save(); RefreshLanguage(); }
        void RefreshLanguage()
        {
            foreach (var label in localizedLabels) label.target.text = T(label.key);
            noticesBody.text = Resources.Load<TextAsset>("LegalNotices")?.text ?? T("legal_missing");
            RefreshLobby(); RefreshSettings(); RefreshDetail();
            turnToastLabel.text=T("select_column");if(pendingAction!=PendingAction.None)RefreshActionText();
#if UNITY_WEBGL && !UNITY_EDITOR
            StageGameSetLanguage(strings.Language);
#endif
            if (board != null) { RefreshMatchHeader(); RefreshBoard(); RefreshResultText(); matchNotice.text = noticeKey.Length == 0 ? "" : F(noticeKey, noticeArgs); }
        }
        void PlayMove() { if (progress.Data.soundEnabled) audioSource.PlayOneShot(moveSound, 1f); }
        void PlayButtonSound(Button button)
        {
            if(destroyed || progress==null || !progress.Data.soundEnabled || applicationPaused || !applicationFocused || !button.IsActive() || !button.IsInteractable())return;
            audioSource.PlayOneShot(buttonSound,.55f);
        }
        void RefreshMusic()
        {
            if (musicSource == null) return;
            if (destroyed || progress == null || (!match.activeInHierarchy && !lobby.activeInHierarchy) || progress.Data.musicMuted)
            {
                StopMusic(); return;
            }
            bool suspend = applicationPaused || !applicationFocused || interstitialPending || (rewardGate != null && rewardGate.Pending);
            if (suspend)
            {
                if (musicStarted && !musicPaused) { musicSource.Pause(); musicPaused = true; }
            }
            else if (!musicStarted) { musicSource.Play(); musicStarted = true; musicPaused = false; }
            else if (musicPaused) { musicSource.UnPause(); musicPaused = false; }
        }
        void StopMusic() { if (musicSource != null && musicStarted) musicSource.Stop(); musicStarted = musicPaused = false; }
        void OnDisable() { CancelSpeech();StopMusic(); }
        void OnApplicationFocus(bool focused) { applicationFocused = focused; turnTimerWasRunning=false; RefreshMusic(); }
        void OnApplicationPause(bool paused) { applicationPaused = paused; turnTimerWasRunning=false; ActiveTime(); if (paused) { CancelToast(); CancelPendingReward(); ads.CancelPending(); interstitialPending = false; progress?.Save(); } RefreshBoard(); RefreshMusic(); }
        void OnApplicationQuit() { progress?.Save(); }
        void OnDestroy() { destroyed=true; matchId=null; pendingAction=PendingAction.None; CancelToast(); CancelSearch(); CancelResultSequence(); rewardGate?.Cancel(); ads.CancelPending(); }
#if UNITY_EDITOR
        public void SetAdsForTests(IAdService service) { ads.CancelPending(); ads = service; }
#endif
    }
}
