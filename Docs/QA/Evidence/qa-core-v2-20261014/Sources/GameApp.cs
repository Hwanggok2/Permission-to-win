using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace StageGame
{
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
        public Image[] columnMarkers;
        public Button[] columns;
        public GameObject lobbyBanner, resultBanner;
        public AudioSource audioSource;
        public AudioClip moveSound;

        static readonly Color Empty = new Color32(15, 27, 43, 255);
        static readonly Color Player = new Color32(108, 237, 199, 255);
        static readonly Color Opponent = new Color32(255, 137, 121, 255);
        static readonly Color Highlight = new Color32(255, 217, 134, 255);
        GameData data;
        ProgressStore progress;
        ConnectFourBoard board;
        IAdService ads = new NoFillAds();
        RewardGate rewardGate;
        int selected, activeStage, hintColumn = -1;
        string matchId;
        bool busy, applicationPaused, resultRecorded, leaving;
        Coroutine aiRoutine;

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
            pause.onClick.AddListener(() => { CancelPendingReward(); confirm.SetActive(true); RefreshBoard(); });
            confirmClose.onClick.AddListener(Resume);
            confirmResume.onClick.AddListener(Resume);
            confirmLeave.onClick.AddListener(ShowLobby);
            hint.onClick.AddListener(RequestHint);
            retry.onClick.AddListener(() => StartStage(activeStage));
            nextStage.onClick.AddListener(() => StartStage(activeStage + 1));
            resultLobby.onClick.AddListener(ShowLobby);
            resultClose.onClick.AddListener(ShowLobby);
            for (int i = 0; i < columns.Length; i++) { int col = i; columns[i].onClick.AddListener(() => Drop(col)); }
            try
            {
                data = GameData.Load();
                progress = new ProgressStore(data.Stages.Length);
                selected = progress.Data.unlockedStage - 1;
                ShowLobby();
                if (!string.IsNullOrEmpty(progress.LoadWarning)) lobbyNotice.text = progress.LoadWarning;
            }
            catch (Exception error)
            {
                Debug.LogError("[StageGame Data] " + error);
                lobby.SetActive(true); match.SetActive(false);
                CloseModals();
                stageTitle.text = "데이터 확인 필요";
                stageDetail.text = "스테이지를 시작할 수 없습니다.";
                lobbyNotice.text = error.Message;
                play.interactable = previous.interactable = following.interactable = false;
            }
        }

        void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Escape)) return;
            if (help.activeSelf) help.SetActive(false);
            else if (settings.activeSelf) settings.SetActive(false);
            else if (confirm.activeSelf) Resume();
            else if (result.activeSelf) ShowLobby();
            else if (match.activeSelf) { CancelPendingReward(); confirm.SetActive(true); RefreshBoard(); }
        }

        void Select(int change)
        {
            if (data == null) return;
            selected = Mathf.Clamp(selected + change, 0, data.Stages.Length - 1);
            RefreshLobby();
        }

        void RefreshLobby()
        {
            var stage = data.Stages[selected];
            bool unlocked = stage.id <= progress.Data.unlockedStage;
            stageNumber.text = $"CHAPTER {stage.chapter:00}   /   STAGE {stage.id:00}";
            stageTitle.text = data.Text(stage.titleKey);
            stageDetail.text = unlocked ? "나의 돌 4개를 먼저 연결하세요.\n열을 누르면 돌이 아래로 떨어집니다." : "이전 스테이지에서 승리하면 열립니다.";
            progressLabel.text = $"{progress.Data.unlockedStage} / {data.Stages.Length} 스테이지 해금";
            play.GetComponentInChildren<Text>().text = unlocked ? "대국 시작" : "잠긴 스테이지";
            play.interactable = unlocked;
            previous.interactable = selected > 0;
            following.interactable = selected < data.Stages.Length - 1;
            lobbyBanner.SetActive(data.Ads.bannerLobby);
            lobbyNotice.text = "무료 재도전 · 진행도는 이 기기에 저장됩니다";
        }

        public void ShowLobby()
        {
            CancelSearch();
            CancelPendingReward();
            ads.CancelPending();
            matchId = null;
            busy = false;
            match.SetActive(false); lobby.SetActive(true); CloseModals();
            if (data != null) RefreshLobby();
        }

        void CloseModals()
        {
            result.SetActive(false); settings.SetActive(false); help.SetActive(false); confirm.SetActive(false);
        }

        public void StartStage(int index)
        {
            if (data == null || index < 0 || index >= data.Stages.Length || index + 1 > progress.Data.unlockedStage) return;
            CancelSearch();
            CancelPendingReward();
            ads.CancelPending();
            try { board = GameData.CreateBoard(data.Stages[index]); }
            catch (Exception error) { ShowLobby(); lobbyNotice.text = error.Message; return; }
            activeStage = selected = index;
            matchId = Guid.NewGuid().ToString("N");
            rewardGate = new RewardGate();
            resultRecorded = false; busy = false; hintColumn = -1;
            lobby.SetActive(false); match.SetActive(true); CloseModals();
            matchTitle.text = $"{index + 1:00}  /  {data.Text(data.Stages[index].titleKey)}";
            matchNotice.text = "민트색은 나, 산호색은 AI입니다.";
            RefreshBoard();
            if (board.CurrentPlayer == -1) aiRoutine = StartCoroutine(Think(false));
        }

        void Drop(int column)
        {
            if (board == null || busy || rewardGate.Pending || applicationPaused || confirm.activeSelf || board.CurrentPlayer != 1 || !board.TryDrop(column)) return;
            hintColumn = -1;
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
                while (!search.Complete && activeSeconds < 0.65)
                {
                    if (!applicationPaused && !confirm.activeSelf)
                    {
                        var frameTimer = System.Diagnostics.Stopwatch.StartNew();
                        do { search.Step(16); }
                        while (!search.Complete && frameTimer.Elapsed.TotalSeconds < 0.004 && activeSeconds + frameTimer.Elapsed.TotalSeconds < 0.65);
                        activeSeconds += frameTimer.Elapsed.TotalSeconds;
                    }
                    yield return null;
                }
                Debug.Log($"[AI] nodes={search.NodesVisited}, completedDepth={search.CompletedDepth}, activeMs={activeSeconds * 1000:F1}");
                while (applicationPaused || confirm.activeSelf) yield return null;
                if (isHint)
                {
                    hintColumn = search.ChosenColumn;
                    matchNotice.text = $"추천: {hintColumn + 1}번 열. 상대의 다음 위협도 살펴보세요.";
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
                stones[i].color = winning ? Highlight : player == 1 ? Player : player == -1 ? Opponent : Empty;
                stoneLabels[i].text = player == 1 ? "나" : player == -1 ? "AI" : "";
            }
            bool canInput = !busy && !rewardGate.Pending && !applicationPaused && !confirm.activeSelf && !board.IsFinished && board.CurrentPlayer == 1;
            for (int col = 0; col < columns.Length; col++)
            {
                columns[col].interactable = canInput && board.CanDrop(col);
                columnMarkers[col].color = col == hintColumn ? Highlight : new Color(1, 1, 1, 0.08f);
            }
            hint.interactable = canInput && !rewardGate.Granted && !rewardGate.Pending;
            turnLabel.text = board.IsFinished ? "대국 종료" : busy ? "다음 수를 살펴보는 중…" : board.CurrentPlayer == 1 ? "나의 차례 · 열을 선택하세요" : "AI의 차례";
            moveLabel.text = $"{board.MoveCount:00} / 42 수";
        }

        void Finish()
        {
            if (resultRecorded) return;
            resultRecorded = true;
            progress.RecordResult(matchId, activeStage + 1, board.Winner == 1, data.Stages.Length);
            resultTitle.text = board.Winner == 1 ? "연결 성공!" : board.IsDraw ? "팽팽한 무승부" : "한 수 더 생각해 볼까요?";
            resultDetail.text = board.Winner == 1 ? "네 개의 돌이 이어졌습니다.\n다음 스테이지에 도전하세요." :
                board.IsDraw ? "42칸이 모두 찼습니다.\n다시 도전해 다른 흐름을 만들어 보세요." : LossExplanation();
            nextStage.gameObject.SetActive(board.Winner == 1 && activeStage + 1 < data.Stages.Length);
            if (board.Winner == 1 && activeStage + 1 == data.Stages.Length) resultDetail.text = "모든 스테이지를 완료했습니다!\n로비에서 원하는 대국을 다시 즐길 수 있어요.";
            resultBanner.SetActive(data.Ads.bannerResult);
            resultAdStatus.text = ads.Status;
            result.SetActive(true);
            SetResultActions(false);
            StartCoroutine(ResultAdBoundary(matchId));
        }

        string LossExplanation()
        {
            if (board.WinningCells.Count < 2) return "상대가 먼저 네 개를 연결했습니다.\n다음에는 상대의 세 돌을 막아 보세요.";
            int a = board.WinningCells[0], b = board.WinningCells[1];
            string direction = a / 7 == b / 7 ? "가로" : a % 7 == b % 7 ? "세로" : "대각선";
            return $"AI가 {direction}로 네 개를 연결했습니다.\n금색으로 표시된 줄을 살펴보세요.";
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
            retry.interactable = nextStage.interactable = resultLobby.interactable = resultClose.interactable = enabled;
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
                    matchNotice.text = value == AdResult.Unavailable ? "현재 광고가 준비되지 않았습니다. 대국은 계속할 수 있어요." : "광고가 완료되지 않아 힌트가 지급되지 않았습니다.";
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
                matchNotice.text = "광고 응답 시간이 지나 취소했습니다. 대국을 계속할 수 있어요.";
                RefreshBoard();
            }
        }

        void CancelPendingReward()
        {
            if (rewardGate == null || !rewardGate.Pending) return;
            rewardGate.Cancel();
            ads.CancelPending();
            if (matchNotice != null) matchNotice.text = "광고 요청을 취소했습니다. 대국을 계속할 수 있어요.";
        }

        void Resume() { confirm.SetActive(false); RefreshBoard(); }
        void OpenSettings()
        {
            settings.SetActive(true);
            soundLabel.text = progress == null || progress.Data.soundEnabled ? "효과음  켜짐" : "효과음  꺼짐";
            settingsDetail.text = "진행도는 이 기기에 저장됩니다.\n광고 SDK 미연결 · 수익 연동 없음\n\n폰트: Noto Sans KR / SIL OFL 1.1\n라이선스: 배포 파일 Notices 폴더";
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
        void OnDestroy() { if (!leaving) CancelSearch(); rewardGate?.Cancel(); }

#if UNITY_EDITOR
        public void SetEditorAds(AdResult value, bool duplicate)
        {
            ads = new EditorTestAds { Result = value, DuplicateCallback = duplicate };
        }
        public void SetAdsForTests(IAdService service) { ads.CancelPending(); ads = service; }
#endif
    }
}
