using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace StageGame.Tests
{
    public sealed partial class RankFlowTests
    {
        static Rect ScreenRect(RectTransform r,Camera camera,float ratio)
        {
            var c=new Vector3[4];r.GetWorldCorners(c);
            var a=RectTransformUtility.WorldToScreenPoint(camera,c[0])/ratio;
            var b=RectTransformUtility.WorldToScreenPoint(camera,c[2])/ratio;
            return Rect.MinMaxRect(a.x,a.y,b.x,b.y);
        }
        void AssertOnlineFits(float ratio)
        {
            var online=app.online;var layout=online.panel.GetComponent<OnlinePanelLayout>();
            var camera=app.GetComponent<Canvas>().worldCamera;
            var area=ScreenRect((RectTransform)online.panel.transform,camera,ratio);
            Assert.AreEqual(0,online.panel.GetComponentsInChildren<ScrollRect>().Length,"Active online UI must not scroll");
            foreach(var graphic in online.panel.GetComponentsInChildren<Graphic>())
            {
                if(!graphic.enabled)continue;
                var r=ScreenRect(graphic.rectTransform,camera,ratio);
                Assert.GreaterOrEqual(r.xMin,area.xMin-.15f,"Left: "+graphic.name);
                Assert.GreaterOrEqual(r.yMin,area.yMin-.15f,"Bottom: "+graphic.name);
                Assert.LessOrEqual(r.xMax,area.xMax+.15f,"Right: "+graphic.name);
                Assert.LessOrEqual(r.yMax,area.yMax+.15f,"Top: "+graphic.name);
                if(graphic is Text t)Assert.LessOrEqual(t.preferredHeight,t.rectTransform.rect.height+2,"Text clipped: "+t.name+" "+t.text);
            }
            foreach(var button in online.panel.GetComponentsInChildren<Button>())
            {
                var r=ScreenRect((RectTransform)button.transform,camera,ratio);
                bool column=button.name.StartsWith("Column_");
                if(column && !button.interactable)continue;
                Assert.GreaterOrEqual(r.width,column?23.99f:43.99f,"Touch width: "+button.name);
                Assert.GreaterOrEqual(r.height,43.99f,"Touch height: "+button.name);
            }
            if(online.game.activeSelf)
            {
                var board=ScreenRect(layout.board,camera,ratio);var footer=ScreenRect(layout.footer,camera,ratio);
                var turn=ScreenRect(layout.turn,camera,ratio);var first=ScreenRect(layout.first,camera,ratio);var profile=ScreenRect(layout.mine,camera,ratio);
                Assert.LessOrEqual(board.yMax,turn.yMin+.1f,"Board overlaps turn");
                Assert.GreaterOrEqual(board.yMin,footer.yMax-.1f,"Board overlaps footer");
                Assert.GreaterOrEqual(profile.yMin,first.yMax-.1f,"Profile overlaps starter");
                Assert.GreaterOrEqual(first.yMin,turn.yMax-.1f,"Starter overlaps turn");
                Assert.AreEqual(board.width,board.height,.1f,"Square board");
                var result=ScreenRect(layout.result,camera,ratio);var notice=ScreenRect(layout.notice,camera,ratio);
                Assert.GreaterOrEqual(result.yMin,notice.yMax-.1f,"Result overlaps notice");
                var action=ScreenRect(online.place.gameObject.activeSelf?layout.place:layout.rematch,camera,ratio);
                Assert.GreaterOrEqual(notice.yMin,action.yMax-.1f,"Notice overlaps action");
                Assert.GreaterOrEqual(action.height,47.99f);
                if(online.rematch.gameObject.activeSelf)Assert.AreEqual(action.yMin,ScreenRect(layout.leave,camera,ratio).yMin,.1f,"Both result actions stay on the same row");
            }
        }
        [UnityTest] public IEnumerator MultiplayerLayoutsFitWithoutScrollingInBothLanguages()
        {
            var online=app.online;online.Open();
            typeof(OnlineGameController).GetField("connected",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(online,true);
            foreach(string language in new[]{"ko","en"})
            {
                app.SetLanguage(language);
                foreach(int viewport in new[]{0,1,2,3,4})
                {
                    int width=viewport==0?1080:viewport==1?390:viewport==4?1080:320;
                    int height=viewport==0?1920:viewport==1?844:viewport==4?1920:568;
                    bool host=viewport==3||viewport==4;float ratio=viewport==4?3:1;
                    var s=new OnlineState{type="state",status="idle",rankKey="mp_rank_7",rating=9999,otherRankKey="mp_rank_19",otherRating=9999,roomId="layout",round=1,board=new int[42],me=0,first=0,turn=0,deadline=60000};
                    foreach(string phase in new[]{"menu","queue","host","invite","playing","reconnecting","confirm","win","request","waiting","left","error"})
                    {
                        if(phase=="menu")s.status="idle";
                        if(phase=="queue")s.status="queue";
                        if(phase=="host"){s.status="waiting";s.invite=new string('a',32);s.rated=false;}
                        if(phase=="invite"){s.status="idle";online.Receive(JsonUtility.ToJson(s));online.Receive("{\"type\":\"preview\",\"rating\":9999,\"rated\":true}");}
                        else
                        {
                            if(phase=="playing"||phase=="confirm"){s.status="playing";s.otherDisconnected=false;s.board[0]=1;s.board[1]=-1;s.board[7]=1;s.board[8]=-1;s.seq=4;s.lastOpponentMove=8;}
                            if(phase=="reconnecting")s.otherDisconnected=true;
                            if(phase=="win"){s.status="finished";s.result="win";s.delta=32;s.seq=7;s.board[14]=1;s.board[15]=-1;s.board[21]=1;}
                            if(phase=="request")s.opponentRequested=true;
                            if(phase=="waiting")s.rematchRequested=true;
                            if(phase=="left")s.opponentLeft=true;
                            online.Receive(JsonUtility.ToJson(s));
                        }
                        if(phase=="confirm")online.close.onClick.Invoke();else online.leaveCancel.onClick.Invoke();
                        if(phase=="left")Assert.AreEqual(app.UiText("mp_rematch"),online.rematchLabel.text,"Departure must stop the waiting label");
                        if(phase=="error"){typeof(OnlineGameController).GetField("messageKey",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(online,"mp_connection_lost");online.Refresh();}
                        if(s.status=="finished")StringAssert.Contains("9999",online.resultLabel.text,"Outcome remains visible with rematch/departure/connection notices");
                        yield return Capture(language+"-"+viewport+"-"+phase,app,width,height,host,validate:()=>AssertOnlineFits(ratio),directory:"Evidence/multiplayer-ui-20261018",scaledCanvas:true,pixelRatio:ratio);
                        if(phase=="error")typeof(OnlineGameController).GetField("messageKey",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(online,"");
                    }
                }
            }
            online.close.onClick.Invoke();
        }
    }
}
