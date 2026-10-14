using UnityEngine;

namespace StageGame
{
    public static class HostLayout
    {
        // StageGame product safety allowance, NOT an official Toss navigation-bar height.
        public const float ReservedTopCss = 64;
        // Insets use CSS pixels, in left/top/right/bottom order. Screen.safeArea uses Unity pixels.
        public static Rect ContentArea(Rect osSafe, Vector2Int screen, Vector2 cssViewport, Vector4 cssInsets, float controlsBottomCss = -1)
        {
            if (screen.x <= 0 || screen.y <= 0 || !Finite(cssViewport.x) || !Finite(cssViewport.y) || cssViewport.x <= 0 || cssViewport.y <= 0) return osSafe;
            float sx = screen.x / cssViewport.x, sy = screen.y / cssViewport.y;
            // Merge the two OS inset sources using max, then add the host allowance exactly once.
            float left = Mathf.Max(osSafe.xMin, Inset(cssInsets.x) * sx);
            float right = Mathf.Max(screen.x - osSafe.xMax, Inset(cssInsets.z) * sx);
            float bottom = Mathf.Max(osSafe.yMin, Inset(cssInsets.w) * sy);
            float top = Mathf.Max(screen.y - osSafe.yMax, Inset(cssInsets.y) * sy) + ReservedTopCss * sy;
            if (Finite(controlsBottomCss) && controlsBottomCss >= 0) top = Mathf.Max(top, (controlsBottomCss + 8) * sy);
            left = Mathf.Clamp(left, 0, screen.x); right = Mathf.Clamp(right, 0, screen.x - left);
            bottom = Mathf.Clamp(bottom, 0, screen.y); top = Mathf.Clamp(top, 0, screen.y - bottom);
            return new Rect(left, bottom, screen.x - left - right, screen.y - top - bottom);
        }
        static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        static float Inset(float value) => Finite(value) ? Mathf.Max(0, value) : 0;
    }
}
