using System;

namespace StageGame
{
    public enum AdResult { Completed, Cancelled, Failed, Unavailable }
    public interface IAdService
    {
        string Status { get; }
        void ShowRewarded(Action<AdResult> callback);
        void ShowInterstitial(Action<AdResult> callback);
        // Must suppress both callbacks and any delayed SDK presentation after cancellation.
        void CancelPending();
    }

    // Production builds have no ad SDK or account IDs. No-fill never produces a reward.
    public sealed class NoFillAds : IAdService
    {
        public string Status => "광고 미연결 · 오프라인 플레이 가능";
        public void ShowRewarded(Action<AdResult> callback) => callback(AdResult.Unavailable);
        public void ShowInterstitial(Action<AdResult> callback) => callback(AdResult.Unavailable);
        public void CancelPending() { }
    }

    public sealed class RewardGate
    {
        bool pending, granted;
        int request;
        public bool Granted => granted;
        public bool Pending => pending;
        public bool TryBegin(out int token)
        {
            token = request;
            if (pending || granted) return false;
            pending = true;
            token = ++request;
            return true;
        }
        public bool Complete(int token, AdResult result)
        {
            if (!pending || token != request) return false;
            pending = false;
            if (result != AdResult.Completed) return false;
            granted = true;
            return true;
        }
        public void Cancel() { pending = false; request++; }
    }

    public static class AdPolicy
    {
        public static bool Eligible(AdSettings settings, ProgressData progress, long utcSeconds)
        {
            return progress.completedMatches > settings.interstitialGraceMatches &&
                progress.completedMatches - progress.lastInterstitialMatch >= settings.interstitialEveryMatches &&
                utcSeconds - progress.lastInterstitialUtc >= settings.interstitialCooldownSeconds;
        }
    }

#if UNITY_EDITOR
    public sealed class EditorTestAds : IAdService
    {
        public AdResult Result = AdResult.Unavailable;
        public bool DuplicateCallback;
        public string Status => "EDITOR 전용 모의 광고 · 수익 없음";
        public void ShowRewarded(Action<AdResult> callback) { callback(Result); if (DuplicateCallback) callback(Result); }
        public void ShowInterstitial(Action<AdResult> callback) { callback(Result); if (DuplicateCallback) callback(Result); }
        public void CancelPending() { }
    }
#endif
}
