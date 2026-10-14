// Numeric API doubles only. No Unity layout, lifecycle, rendering, or input claims.
namespace UnityEngine
{
    public struct Vector2Int { public int x, y; public Vector2Int(int x, int y) { this.x=x; this.y=y; } }
    public struct Vector2 { public float x, y; public Vector2(float x, float y) { this.x=x; this.y=y; } }
    public struct Vector4 { public float x, y, z, w; public Vector4(float x, float y, float z, float w) { this.x=x; this.y=y; this.z=z; this.w=w; } }
    public struct Rect
    {
        public float x, y, width, height;
        public Rect(float x, float y, float width, float height) { this.x=x; this.y=y; this.width=width; this.height=height; }
        public float xMin => x; public float xMax => x+width;
        public float yMin => y; public float yMax => y+height;
    }
    public static class Mathf
    {
        public static float Max(float a, float b) => a>b?a:b;
        public static float Min(float a, float b) => a<b?a:b;
        public static float Clamp(float value, float min, float max) { if(value<min) value=min; else if(value>max) value=max; return value; }
    }
}
