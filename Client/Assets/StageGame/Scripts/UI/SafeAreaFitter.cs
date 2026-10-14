using UnityEngine;

namespace StageGame
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        public RectTransform content;
        RectTransform safeRect;
        Rect lastArea;
        Vector2Int lastSize;
        Vector2 lastRectSize;

        void OnEnable()
        {
            safeRect = (RectTransform)transform;
            lastArea = default; lastSize = default; lastRectSize = default;
            // uGUI CanvasScaler updates in preWillRenderCanvases, after MonoBehaviour.Update.
            Canvas.willRenderCanvases += Fit;
        }
        void OnDisable() { Canvas.willRenderCanvases -= Fit; }

        void Fit()
        {
            if (Screen.width <= 0 || Screen.height <= 0) return;
            Rect safe = Screen.safeArea;
            var size = new Vector2Int(Screen.width, Screen.height);
            bool anchorsChanged = safe != lastArea || size != lastSize;
            if (anchorsChanged)
            {
                lastArea = safe; lastSize = size;
                safeRect.anchorMin = new Vector2(safe.xMin / Screen.width, safe.yMin / Screen.height);
                safeRect.anchorMax = new Vector2(safe.xMax / Screen.width, safe.yMax / Screen.height);
                safeRect.offsetMin = safeRect.offsetMax = Vector2.zero;
            }
            // The available rectangle can change later without another Screen-size event.
            Vector2 available = safeRect.rect.size;
            if (content == null || available.x <= 0 || available.y <= 0 || (!anchorsChanged && available == lastRectSize)) return;
            lastRectSize = available;
            float fit = Mathf.Min(1f, available.x / 720f, available.y / 1280f);
            content.localScale = Vector3.one * fit;
        }
    }
}
