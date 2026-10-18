using UnityEngine;
using UnityEngine.UI;

namespace StageGame
{
    // Code-native pixel ring, saved as a Graphic in the board prefab. No raster asset edits.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class LastMoveHighlight : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear(); var rect = rectTransform.rect; float unit = Mathf.Min(rect.width, rect.height) / 32;
            for (int y = -16; y < 16; y++) for (int x = -16; x < 16; x++)
            {
                float radius = new Vector2(x + .5f, y + .5f).magnitude;
                if (radius < 13 || radius > 15.5f) continue;
                int start = mesh.currentVertCount; var lo = rect.center + new Vector2(x, y) * unit;
                mesh.AddVert(lo, color, Vector2.zero); mesh.AddVert(lo + Vector2.right * unit, color, Vector2.zero);
                mesh.AddVert(lo + Vector2.one * unit, color, Vector2.zero); mesh.AddVert(lo + Vector2.up * unit, color, Vector2.zero);
                mesh.AddTriangle(start, start + 1, start + 2); mesh.AddTriangle(start, start + 2, start + 3);
            }
        }
        public void Show(Image[] stones, int cell)
        {
            bool visible = cell >= 0 && cell < stones.Length;
            gameObject.SetActive(visible); if (!visible) return;
            rectTransform.anchoredPosition = stones[cell].rectTransform.anchoredPosition;
            rectTransform.sizeDelta = stones[cell].rectTransform.sizeDelta * 1.32f;
        }
    }
}
