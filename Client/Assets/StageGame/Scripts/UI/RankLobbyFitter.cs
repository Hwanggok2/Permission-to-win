using UnityEngine;

namespace StageGame
{
    // Keep the saved list's lower edge; reveal two rows and half of the next row.
    public sealed class RankLobbyFitter : MonoBehaviour
    {
        public RectTransform stages, help, settings, notice;
        public RectTransform multiplayer, adRemoval;
        public float maximumListHeight, minimumTopInset, buttonAboveList, noticeAboveList;
        RectTransform rect;
        void OnEnable() { Canvas.willRenderCanvases += Fit; Fit(); }
        void OnDisable() => Canvas.willRenderCanvases -= Fit;
        // Update before uGUI recalculates clipping when the safe-area size changes.
        void OnRectTransformDimensionsChange() => Fit();
        public void Fit()
        {
            if (rect == null) rect = (RectTransform)transform;
            if (stages == null || help == null || settings == null || notice == null || rect.rect.height <= 0) return;
            float top = Mathf.Max(minimumTopInset, rect.rect.height - stages.offsetMin.y - maximumListHeight);
            stages.offsetMax = new Vector2(stages.offsetMax.x, -top);
            help.anchoredPosition = new Vector2(help.anchoredPosition.x, buttonAboveList - top);
            settings.anchoredPosition = new Vector2(settings.anchoredPosition.x, buttonAboveList - top);
            notice.anchoredPosition = new Vector2(notice.anchoredPosition.x, noticeAboveList - top);
            if(multiplayer!=null && adRemoval!=null)
            {
                bool show=adRemoval.gameObject.activeSelf;
                multiplayer.anchoredPosition=new Vector2(RankUiMetrics.U(show?-130:0),multiplayer.anchoredPosition.y);
                multiplayer.sizeDelta=new Vector2(RankUiMetrics.U(show?420:680),multiplayer.sizeDelta.y);
            }
        }
    }
}
