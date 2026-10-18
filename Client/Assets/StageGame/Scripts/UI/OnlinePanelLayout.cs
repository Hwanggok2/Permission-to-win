using UnityEngine;
using UnityEngine.UI;

namespace StageGame
{
    // Positions the saved prefab only. The footer stays visible; the square board uses the remaining space.
    public sealed class OnlinePanelLayout : MonoBehaviour
    {
        public RectTransform menu, game, title, close, profile, description, random, friendly, invitation;
        public RectTransform rated, share, copy, join, url, mine, opponent, first, turn, board, footer, result, notice, place, rematch, leave;
        public RectTransform confirm, confirmFrame, confirmBody, confirmCancel, confirmAccept;
        public Text randomHint, friendlyHint;
        readonly Vector3[] corners=new Vector3[4];
        SafeAreaFitter safeArea;
        void OnEnable(){safeArea=GetComponentInParent<SafeAreaFitter>();Canvas.willRenderCanvases+=Fit;}
        void OnDisable(){Canvas.willRenderCanvases-=Fit;}
        static void Box(RectTransform r,float x,float top,float w,float h)
        {
            r.anchorMin=r.anchorMax=new Vector2(.5f,1);r.pivot=new Vector2(.5f,1);
            r.localScale=Vector3.one;r.anchoredPosition=new Vector2(x,-top);r.sizeDelta=new Vector2(w,h);
        }
        static void Fill(RectTransform r){r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.pivot=Vector2.one*.5f;r.offsetMin=r.offsetMax=Vector2.zero;r.localScale=Vector3.one;}
        static void Font(RectTransform r,float size){var t=r.GetComponent<Text>();if(t!=null)t.fontSize=Mathf.RoundToInt(size);}
        static float TextHeight(RectTransform r)=>r.GetComponent<Text>().preferredHeight;
        static void ButtonLabel(RectTransform r,float size,float padding)
        {
            var t=r.Find("Label").GetComponent<Text>();Fill(t.rectTransform);
            t.rectTransform.offsetMin=new Vector2(padding,padding);t.rectTransform.offsetMax=new Vector2(-padding,-padding);t.fontSize=Mathf.RoundToInt(size);
        }
        public void Fit()
        {
            var root=(RectTransform)transform;float w=root.rect.width,h=root.rect.height;
            var canvas=GetComponentInParent<Canvas>();if(canvas==null || w<=0 || h<=0 || board==null)return;
            var camera=canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.worldCamera;
            root.GetWorldCorners(corners);
            float pixels=Vector2.Distance(RectTransformUtility.WorldToScreenPoint(camera,corners[0]),RectTransformUtility.WorldToScreenPoint(camera,corners[3]));
            float css= pixels/Mathf.Max(.01f,safeArea==null?1:safeArea.RenderPixelsPerCssPixel);
            if(css<=0)return;
            float p=w/css,d=w/720f,ch=h/p;bool compact=ch<600,small=ch<480;
            float gap=compact?2*p:12*d,margin=compact?8*p:24*d;
            float header=Mathf.Max(44*p,96*d),action=Mathf.Max(48*p,108*d),bodyFont=Mathf.Max(14*p,28*d);
            float smallFont=small?12*p:Mathf.Max(13*p,25*d);
            float closeSize=Mathf.Max(44*p,88*d);
            Box(title,-closeSize/2,0,w-closeSize-2*margin,header);Font(title,Mathf.Max(20*p,34*d));
            Box(close,w/2-margin-closeSize/2,0,closeSize,closeSize);
            Fill(menu);Fill(game);Fill(invitation);Fill(confirm);

            float top=header+gap,inner=w-2*margin;
            float profileH=Mathf.Max(44*p,90*d),descH=Mathf.Max(64*p,112*d);
            Box(profile,0,top,inner,profileH);Font(profile,bodyFont);
            Box(description,0,top+profileH+gap,inner,descH);Font(description,bodyFont);
            descH=Mathf.Max(descH,TextHeight(description));description.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,descH);
            float modesTop=top+profileH+descH+gap*3;
            float cardH=Mathf.Max(80*p,150*d);
            Box(random,0,modesTop,inner,cardH);Box(friendly,0,modesTop+cardH+gap,inner,cardH);
            foreach(var r in new[]{random,friendly})
            {
                var label=(RectTransform)r.Find("Label");Box(label,0,cardH*.04f,inner-24*p,cardH*.42f);Font(label,Mathf.Max(18*p,32*d));
            }
            foreach(var hint in new[]{randomHint,friendlyHint}){Box(hint.rectTransform,0,cardH*.46f,inner-24*p,cardH*.5f);hint.fontSize=Mathf.RoundToInt(bodyFont);}
            Box(rated,0,modesTop,inner,action);Box(share,-(inner+gap)/4,modesTop+action+gap,(inner-gap)/2,action);
            Box(copy,(inner+gap)/4,modesTop+action+gap,(inner-gap)/2,action);Box(join,0,modesTop+action+gap,inner,action);
            Box(url,0,modesTop+2*(action+gap),inner,Mathf.Max(44*p,80*d));
            var link=(RectTransform)url.Find("Text");Fill(link);link.offsetMin=new Vector2(8*p,4*p);link.offsetMax=new Vector2(-8*p,-4*p);Font(link,Mathf.Max(12*p,18*d));

            float profiles=compact?(small?44:54)*p:112*d;
            Box(mine,-(inner+gap)/4,top,(inner-gap)/2,profiles);Box(opponent,(inner+gap)/4,top,(inner-gap)/2,profiles);
            foreach(var card in new[]{mine,opponent})
            {
                var text=(RectTransform)card.Find("Label");Fill(text);text.offsetMin=new Vector2(4*p,0);text.offsetMax=new Vector2(-4*p,0);Font(text,smallFont);
                profiles=Mathf.Max(profiles,TextHeight(text));
            }
            mine.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,profiles);opponent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,profiles);
            float firstH=compact?(small?16:18)*p:36*d,turnH=compact?(small?26:28)*p:64*d;
            Box(first,0,top+profiles,inner,firstH);Font(first,smallFont);
            firstH=Mathf.Max(firstH,TextHeight(first));first.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,firstH);
            Box(turn,0,top+profiles+firstH,inner,turnH);Font(turn,Mathf.Max(16*p,30*d));
            float boardTop=top+profiles+firstH+turnH+gap;

            float bottom=compact?0:16*d,resultH=compact?22*p:48*d,noticeH=compact?18*p:44*d;
            Box(result,0,0,inner,resultH);Font(result,Mathf.Max(14*p,28*d));resultH=Mathf.Max(resultH,TextHeight(result));
            Box(notice,0,0,inner,noticeH);Font(notice,smallFont);noticeH=Mathf.Max(noticeH,TextHeight(notice));
            float footerH=resultH+noticeH+gap+action+bottom;
            Box(footer,0,h-footerH,w,footerH);Box(result,0,0,inner,resultH);Box(notice,0,resultH,inner,noticeH);
            float actionTop=resultH+noticeH+gap;
            Box(place,0,actionTop,inner,action);Box(rematch,-(inner+gap)/4,actionTop,(inner-gap)/2,action);Box(leave,(inner+gap)/4,actionTop,(inner-gap)/2,action);
            float boardSize=Mathf.Max(1,Mathf.Min(inner,h-footerH-gap-boardTop));
            // Board children retain their authored 704-unit geometry, including stones and column hit areas.
            float authored=RankUiMetrics.U(704);Box(board,0,boardTop+(h-footerH-gap-boardTop-boardSize)/2,authored,authored);board.localScale=Vector3.one*(boardSize/authored);
            foreach(var r in new[]{rated,share,copy,join,place,rematch,leave,confirmCancel,confirmAccept})ButtonLabel(r,bodyFont,4*p);

            float modalW=Mathf.Min(inner,660*d),modalH=Mathf.Min(h-16*p,Mathf.Max(210*p,440*d));
            Box(confirmFrame,0,(h-modalH)/2,modalW,modalH);
            Box(confirmBody,0,12*p,modalW-24*p,modalH-action-36*p);Font(confirmBody,bodyFont);
            Box(confirmCancel,-(modalW-12*p)/4,modalH-action-12*p,(modalW-36*p)/2,action);
            Box(confirmAccept,(modalW-12*p)/4,modalH-action-12*p,(modalW-36*p)/2,action);
        }
    }
}
