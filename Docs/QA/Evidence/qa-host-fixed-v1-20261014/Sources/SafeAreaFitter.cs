using UnityEngine;
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace StageGame
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        public RectTransform content;
        public Vector2 referenceSize = new Vector2(720, 960);
        RectTransform safeRect;
        Rect lastArea;
        Vector2Int lastSize;
        Vector2 lastRectSize;
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] static extern void StageGameReadHostInsets(out float left, out float top, out float right, out float bottom, out float width, out float height, out float controlsBottom);
#endif
#if UNITY_EDITOR
        bool previewHost;
        Vector2 previewViewport;
        Vector4 previewInsets;
        public void SetEditorHostPreview(bool enabled, Vector2 viewport, Vector4 insets)
        {
            previewHost = enabled; previewViewport = viewport; previewInsets = insets; lastArea = default;
        }
#endif

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
#if UNITY_WEBGL && !UNITY_EDITOR
            StageGameReadHostInsets(out float left, out float top, out float right, out float bottom, out float width, out float height, out float controlsBottom);
            safe = HostLayout.ContentArea(safe, size, new Vector2(width, height), new Vector4(left, top, right, bottom), controlsBottom);
#elif UNITY_EDITOR
            if (previewHost) safe = HostLayout.ContentArea(safe, size, previewViewport, previewInsets);
#endif
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
            float fit = Mathf.Min(1f, available.x / referenceSize.x, available.y / referenceSize.y);
            content.localScale = Vector3.one * fit;
        }
    }
}
