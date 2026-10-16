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
        public bool widthFitTopAligned;
        public float topPaddingCss = 8;
        public float RenderPixelsPerCssPixel { get; private set; } = 1;
        RectTransform safeRect;
        Rect lastArea;
        Vector2Int lastSize;
        Vector2 lastRectSize;
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] static extern void StageGameReadHostInsets(out float left, out float top, out float right, out float bottom, out float width, out float height, out float controlsBottom, out float hostActive);
#endif
#if UNITY_EDITOR
        bool previewEnabled, previewHost;
        Vector2 previewViewport;
        Vector4 previewInsets;
        public void SetEditorHostPreview(bool enabled, Vector2 viewport, Vector4 insets)
        {
            previewEnabled = previewHost = enabled; previewViewport = viewport; previewInsets = insets; lastArea = default;
        }
        public void SetEditorViewportPreview(Vector2 viewport, Vector4 insets, bool hostActive)
        {
            previewEnabled=true;previewHost=hostActive;previewViewport=viewport;previewInsets=insets;lastArea=default;
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
            float cssPixelRatio = 1;
#if UNITY_WEBGL && !UNITY_EDITOR
            StageGameReadHostInsets(out float left, out float top, out float right, out float bottom, out float width, out float height, out float controlsBottom, out float hostActive);
            safe = HostLayout.ContentArea(safe, size, new Vector2(width, height), new Vector4(left, top, right, bottom), controlsBottom, hostActive > .5f);
            if (width > 0) cssPixelRatio = Screen.width / width;
#elif UNITY_EDITOR
            if (previewEnabled) { safe = HostLayout.ContentArea(safe, size, previewViewport, previewInsets, hostActive:previewHost); cssPixelRatio = Screen.width / previewViewport.x; }
#endif
            var canvas = GetComponentInParent<Canvas>();
            RenderPixelsPerCssPixel = cssPixelRatio * canvas.pixelRect.width / Screen.width;
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
            if (widthFitTopAligned)
            {
                // Web uses a centered portrait canvas. Also protect editor/native wide windows
                // without reducing the width of an ordinary portrait mobile viewport.
                float contentWidth = available.x;
                if (available.x > available.y)
                {
                    float cssToLocal = RenderPixelsPerCssPixel / canvas.scaleFactor;
                    contentWidth = Mathf.Min(contentWidth, Mathf.Clamp(available.y * referenceSize.x / referenceSize.y, 320 * cssToLocal, 480 * cssToLocal));
                }
                float widthFit = contentWidth / referenceSize.x;
                // RenderTexture previews can have a pixel size different from the editor Game view.
                float padding = topPaddingCss * cssPixelRatio * canvas.pixelRect.width / Screen.width / canvas.scaleFactor;
                content.anchorMin = content.anchorMax = content.pivot = new Vector2(.5f, 1);
                content.anchoredPosition = new Vector2(0, -padding);
                content.sizeDelta = new Vector2(referenceSize.x, Mathf.Max(1, available.y - padding) / widthFit);
                content.localScale = Vector3.one * widthFit;
                return;
            }
            float fit = Mathf.Min(1f, available.x / referenceSize.x, available.y / referenceSize.y);
            content.localScale = Vector3.one * fit;
        }
    }
}
