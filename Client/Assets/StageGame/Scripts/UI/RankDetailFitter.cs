using UnityEngine;
using UnityEngine.UI;

namespace StageGame
{
    // Center the saved header/body inside the safe-area screen. The outer rect only slides in X.
    public sealed class RankDetailFitter : MonoBehaviour
    {
        public ScrollRect scroll;
        public RectTransform number, close;
        public float headerHeight = 180, headerCenter = 82.5f, minimumVerticalGap = 48;
        RectTransform rect;
        void OnEnable() { Canvas.willRenderCanvases += Fit; Fit(); }
        void OnDisable() => Canvas.willRenderCanvases -= Fit;
        public void Fit()
        {
            if (rect == null) rect = (RectTransform)transform;
            if (scroll == null || number == null || close == null || rect.rect.height <= 0) return;
            float height = rect.rect.height;
            float gap = Mathf.Max(minimumVerticalGap, (height - headerHeight - scroll.content.rect.height) / 2);
            gap = Mathf.Min(gap, Mathf.Max(0, (height - headerHeight - 1) / 2));
            number.anchoredPosition = new Vector2(number.anchoredPosition.x, -gap - headerCenter);
            close.anchoredPosition = new Vector2(close.anchoredPosition.x, -gap - headerCenter);
            var viewport = (RectTransform)scroll.transform;
            viewport.offsetMin = new Vector2(viewport.offsetMin.x, gap);
            viewport.offsetMax = new Vector2(viewport.offsetMax.x, -gap - headerHeight);
        }
    }
}
