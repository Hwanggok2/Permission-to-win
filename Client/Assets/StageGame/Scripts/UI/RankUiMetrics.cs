using UnityEngine;

namespace StageGame
{
    // The saved UI uses actual 1080-wide design units. CSS fitting is performed once by SafeAreaFitter.
    public static class RankUiMetrics
    {
        public const float Unit = 1.5f;
        public static readonly Vector2 Resolution = new Vector2(1080, 1920);
        public static float U(float value) => value * Unit;
        public static Vector2 Point(float x, float y) => new Vector2(U(x), U(y));
    }
}
