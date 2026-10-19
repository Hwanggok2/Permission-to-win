using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace StageGame
{
    public sealed partial class OnlineGameController
    {
        public OnlinePanelLayout layout;
        public Image outcomeBanner, resultBackdrop, winningLine;
        public LastMoveHighlight finalMove;
        int pendingCell=-1, finalCell=-1, displayedRating;
        float pendingAt;
        Coroutine feedbackRoutine;
        RectTransform animatedStone;
        int animatedSibling;
        static readonly Color WinInk=new Color32(38,83,39,255),LossInk=new Color32(143,48,38,255),NeutralInk=new Color32(57,62,49,255);

        void ClearPendingMove(){sentMove=false;pendingCell=-1;}
        static int ChangedCell(OnlineState before,OnlineState after)
        {
            if(before?.status!="playing" || before.roomId!=after.roomId || before.round!=after.round || after.seq!=before.seq+1 || before.board?.Length!=42 || after.board?.Length!=42)return -1;
            int cell=-1;
            for(int i=0;i<42;i++)if(before.board[i]!=after.board[i])
            {
                if(cell>=0 || before.board[i]!=0 || after.board[i]==0)return -1;
                cell=i;
            }
            return cell;
        }
        void PrepareFeedback(OnlineState before,int changedCell,bool restored)
        {
            if(restored){CancelFeedback();finalCell=-1;displayedRating=state.rating;return;}
            bool sameRound=before!=null && before.roomId==state.roomId && before.round==state.round;
            if(!sameRound || state.status!="finished"){CancelFeedback();finalCell=-1;displayedRating=state.rating;}
            if(state.status!="finished")return;
            // Reconnects and repeated result/rematch notices show the final score immediately.
            if(!sameRound || before.status!="playing"){if(feedbackRoutine==null)displayedRating=state.rating;return;}
            CancelFeedback();finalCell=state.reason=="four"||state.reason=="draw"?changedCell:-1;
            displayedRating=state.rating-state.delta;
            if(finalCell<0 && state.delta==0)return;
            feedbackRoutine=StartCoroutine(FinishFeedback());
        }
        IEnumerator FinishFeedback()
        {
            if(finalCell>=0)
            {
                animatedStone=stones[finalCell].rectTransform;animatedSibling=animatedStone.GetSiblingIndex();animatedStone.SetAsLastSibling();
                finalMove.transform.SetAsLastSibling();
                float elapsed=0;
                while(elapsed<.65f)
                {
                    float zoom=elapsed<.2f?Mathf.SmoothStep(1,1.65f,elapsed/.2f):elapsed<.3f?1.65f:Mathf.SmoothStep(1.65f,1,(elapsed-.3f)/.35f);
                    animatedStone.localScale=Vector3.one*zoom;
                    // The ring is already 1.32x the stone. Keep its outer edge inside narrow screens.
                    finalMove.rectTransform.localScale=Vector3.one*Mathf.Lerp(1,1.35f,(zoom-1)/.65f);
                    yield return null;elapsed+=Time.unscaledDeltaTime;
                }
                RestoreFinalStone();
            }
            int from=state.rating-state.delta,to=state.rating;
            if(from!=to)
            {
                float elapsed=0;
                while(elapsed<.55f)
                {
                    displayedRating=Mathf.RoundToInt(Mathf.Lerp(from,to,1-Mathf.Pow(1-elapsed/.55f,3)));
                    RefreshResultFeedback();yield return null;elapsed+=Time.unscaledDeltaTime;
                }
            }
            displayedRating=to;feedbackRoutine=null;RefreshResultFeedback();
        }
        void RestoreFinalStone()
        {
            if(animatedStone!=null){animatedStone.localScale=Vector3.one;animatedStone.SetSiblingIndex(animatedSibling);animatedStone=null;}
            if(finalMove!=null)finalMove.rectTransform.localScale=Vector3.one;
        }
        void CancelFeedback()
        {
            if(feedbackRoutine!=null)StopCoroutine(feedbackRoutine);
            feedbackRoutine=null;RestoreFinalStone();displayedRating=state?.rating??200;
        }
        void RefreshResultFeedback()
        {
            bool finished=state.status=="finished";
            layout.Finished=finished;outcomeBanner.gameObject.SetActive(finished);resultBackdrop.enabled=finished;
            var ink=state.result=="win"?WinInk:state.result=="loss"?LossInk:NeutralInk;
            turn.color=finished?ink:NeutralInk;
            outcomeBanner.color=state.result=="win"?new Color32(224,235,185,255):state.result=="loss"?new Color32(244,213,196,255):new Color32(223,221,199,255);
            resultLabel.color=finished?(state.delta>0?WinInk:state.delta<0?LossInk:NeutralInk):NeutralInk;
            resultLabel.text=finished?state.rated?F("mp_rating_result",displayedRating,state.delta.ToString("+0;-0;0")):F("mp_rating_unchanged",state.rating):T(state.rated?"mp_rated_on":"mp_rated_off");
        }
        void RefreshFinishMarks()
        {
            bool finished=state.status=="finished";
            finalMove.Show(stones,finished?finalCell:-1);winningLine.gameObject.SetActive(false);
            if(!finished || state.reason!="four")return;
            int winner=state.result=="win"?state.me:1-state.me,value=winner==0?1:-1;
            // This only illustrates the authoritative result; it never determines a win or rating.
            foreach(var direction in new[]{new Vector2Int(1,0),new Vector2Int(0,1),new Vector2Int(1,1),new Vector2Int(1,-1)})
                for(int cell=0;cell<42;cell++)
                {
                    int x=cell%7,y=cell/7,endX=x+3*direction.x,endY=y+3*direction.y;
                    if(endX<0||endX>=7||endY<0||endY>=6)continue;
                    bool four=true;for(int n=0;n<4;n++)if(state.board[(y+n*direction.y)*7+x+n*direction.x]!=value){four=false;break;}
                    if(!four)continue;
                    var a=stones[cell].rectTransform.anchoredPosition;var b=stones[endY*7+endX].rectTransform.anchoredPosition;
                    winningLine.rectTransform.anchoredPosition=(a+b)/2;winningLine.rectTransform.sizeDelta=new Vector2(Vector2.Distance(a,b),RankUiMetrics.U(5));
                    winningLine.rectTransform.localEulerAngles=new Vector3(0,0,Mathf.Atan2(b.y-a.y,b.x-a.x)*Mathf.Rad2Deg);
                    winningLine.gameObject.SetActive(true);return;
                }
        }
    }
}
