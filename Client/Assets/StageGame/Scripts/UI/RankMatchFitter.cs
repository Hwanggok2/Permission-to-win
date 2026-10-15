using UnityEngine;
using UnityEngine.UI;

namespace StageGame
{
    // Fits the saved match tree below the reserved top area. Small screens retain usable hit targets.
    public sealed class RankMatchFitter : MonoBehaviour
    {
        public ScrollRect scroll;
        public RectTransform minimumTapTarget;
        public float minimumTapPixels=44;
        readonly Vector3[] corners=new Vector3[4];
        SafeAreaFitter safeArea;

        void OnEnable() { safeArea=GetComponentInParent<SafeAreaFitter>();Canvas.willRenderCanvases+=Fit; }
        void OnDisable() => Canvas.willRenderCanvases-=Fit;
        void Fit()
        {
            if(scroll==null || minimumTapTarget==null)return;
            var content=scroll.content;var viewport=scroll.viewport;
            if(content.rect.width<=0 || content.rect.height<=0 || viewport.rect.height<=0)return;
            var canvas=GetComponentInParent<Canvas>();
            if(canvas==null)return;
            var camera=canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.worldCamera;
            minimumTapTarget.GetWorldCorners(corners);
            float pixels=RectTransformUtility.WorldToScreenPoint(camera,corners[2]).x-RectTransformUtility.WorldToScreenPoint(camera,corners[0]).x;
            if(pixels<=0 || content.localScale.x<=0)return;
            // WebGL screen points use framebuffer pixels; touch size is measured in CSS pixels.
            float pixelRatio=safeArea==null?1:safeArea.RenderPixelsPerCssPixel;
            float minimumScale=minimumTapPixels*pixelRatio*content.localScale.x/pixels;
            float widthFit=Mathf.Min(1,viewport.rect.width/content.rect.width);
            float heightFit=viewport.rect.height/content.rect.height;
            float scale=Mathf.Min(widthFit,Mathf.Max(heightFit,minimumScale));
            content.localScale=Vector3.one*scale;
            if(content.rect.height*scale<=viewport.rect.height+.01f)
            {
                scroll.StopMovement();content.anchoredPosition=Vector2.zero;
            }
        }
    }
}
