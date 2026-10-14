using UnityEngine;

namespace StageGame
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        public RectTransform content;
        Rect lastArea;
        Vector2Int lastSize;
        void Update()
        {
            if (Screen.width <= 0 || Screen.height <= 0) return;
            Rect safe = Screen.safeArea;
            var size = new Vector2Int(Screen.width, Screen.height);
            if (safe == lastArea && size == lastSize) return;
            lastArea = safe; lastSize = size;
            var rect = (RectTransform)transform;
            rect.anchorMin = new Vector2(safe.xMin / Screen.width, safe.yMin / Screen.height);
            rect.anchorMax = new Vector2(safe.xMax / Screen.width, safe.yMax / Screen.height);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            if (content != null)
            {
                float fit = Mathf.Min(rect.rect.width / 720f, rect.rect.height / 1280f);
                content.localScale = Vector3.one * Mathf.Min(1f, fit);
            }
        }
    }
}
