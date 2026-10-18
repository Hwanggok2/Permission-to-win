using UnityEngine;
using UnityEngine.UI;

namespace StageGame
{
    // Fits existing serialized elements in CSS-sized touch targets, including hosted short views.
    public sealed class AdRemovalOfferLayout : MonoBehaviour
    {
        public RectTransform frame, close, title, subtitle, benefits, hintBenefit, permanent, buy, later, restore, status;
        readonly Vector3[] corners=new Vector3[4];
        void OnEnable(){Canvas.willRenderCanvases+=Fit;}
        void OnDisable(){Canvas.willRenderCanvases-=Fit;}
        static void Box(RectTransform r,float x,float y,float w,float h)
        {
            r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);r.localScale=Vector3.one;
        }
        static void Font(RectTransform r,float size){var text=r.GetComponent<Text>();if(text!=null)text.fontSize=Mathf.RoundToInt(size);}
        static void Action(RectTransform r,float size)
        {
            var text=r.Find("Label").GetComponent<Text>();text.rectTransform.anchorMin=Vector2.zero;text.rectTransform.anchorMax=Vector2.one;
            text.rectTransform.offsetMin=new Vector2(8,2);text.rectTransform.offsetMax=new Vector2(-8,-2);text.fontSize=Mathf.RoundToInt(size);
        }
        public void Fit()
        {
            var root=(RectTransform)transform;var canvas=GetComponentInParent<Canvas>();if(canvas==null || root.rect.width<=0)return;
            var safe=GetComponentInParent<SafeAreaFitter>();var camera=canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.worldCamera;
            root.GetWorldCorners(corners);float pixels=Vector2.Distance(RectTransformUtility.WorldToScreenPoint(camera,corners[0]),RectTransformUtility.WorldToScreenPoint(camera,corners[3]));
            float css=pixels/Mathf.Max(.01f,safe==null?1:safe.RenderPixelsPerCssPixel);if(css<=0)return;
            float p=root.rect.width/css,h=root.rect.height/p,w=Mathf.Min(350,css-24);
            bool compact=h<550;float padding=compact?10:16,gap=compact?3:8,titleH=44,benefitH=compact?32:46,descriptionH=compact?32:40,actionH=compact?48:52,footH=44;
            float subtitleH=compact?0:32,statusH=28;
            float total=padding*2+titleH+subtitleH+gap*5+benefitH*2+descriptionH+actionH+footH+statusH;
            frame.anchorMin=frame.anchorMax=frame.pivot=Vector2.one*.5f;frame.anchoredPosition=Vector2.zero;frame.sizeDelta=new Vector2(w,total)*p;
            float inner=w-padding*2,y=padding;
            Box(title,-22*p,y*p,(inner-44)*p,titleH*p);Font(title,(compact?24:28)*p);
            Box(close,(w/2-26)*p,6*p,44*p,44*p);
            ((RectTransform)close.Find("Icon")).sizeDelta=Vector2.one*24*p;
            y+=titleH;subtitle.gameObject.SetActive(!compact);Box(subtitle,0,y*p,inner*p,subtitleH*p);Font(subtitle,15*p);y+=subtitleH+gap;
            Box(benefits,0,y*p,inner*p,benefitH*p);Action(benefits,14*p);y+=benefitH+gap;
            Box(hintBenefit,0,y*p,inner*p,benefitH*p);Action(hintBenefit,14*p);y+=benefitH+gap;
            Box(permanent,0,y*p,inner*p,descriptionH*p);Font(permanent,13*p);y+=descriptionH+gap;
            Box(buy,0,y*p,inner*p,actionH*p);Action(buy,16*p);y+=actionH+gap;
            float restoreW=80,between=8,laterW=inner-restoreW-between;
            Box(later,-(restoreW+between)/2*p,y*p,laterW*p,footH*p);Action(later,14*p);
            Box(restore,(laterW+between)/2*p,y*p,restoreW*p,footH*p);Action(restore,13*p);y+=footH;
            Box(status,0,y*p,inner*p,statusH*p);Font(status,12*p);
        }
    }
}
