using UnityEngine;

namespace StageGame
{
    // Only adjusts the thin dimension of serialized lines; never creates UI objects.
    public sealed class UniformBoardLines : MonoBehaviour
    {
        public RectTransform[] vertical, horizontal;
        public float referenceThickness = 3;
        void OnEnable() => Canvas.willRenderCanvases += Fit;
        void OnDisable() => Canvas.willRenderCanvases -= Fit;
        void Fit()
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;
            var rect = (RectTransform)transform;
            var camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            var corners = new Vector3[4]; rect.GetWorldCorners(corners);
            var a = RectTransformUtility.WorldToScreenPoint(camera, corners[0]);
            var b = RectTransformUtility.WorldToScreenPoint(camera, corners[2]);
            float sx = (b.x - a.x) / rect.rect.width, sy = (b.y - a.y) / rect.rect.height;
            if (sx <= 0 || sy <= 0) return;
            int pixels = Mathf.Max(1, Mathf.RoundToInt(referenceThickness * Mathf.Min(sx, sy)));
            foreach (var line in vertical) line.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, pixels / sx);
            foreach (var line in horizontal) line.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, pixels / sy);
        }
    }
}
