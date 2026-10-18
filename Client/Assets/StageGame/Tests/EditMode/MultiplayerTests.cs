using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace StageGame.Tests
{
    public sealed class OpponentMoveTests
    {
        [Test] public void LastOpponentMovePersistsThroughHumanMoveAndClone()
        {
            var board=new ConnectFourBoard();board.TryDrop(3);board.TryDrop(2);var undo=board.Clone();board.TryDrop(4);
            Assert.AreEqual(2,board.LastMoveFor(-1));Assert.AreEqual(2,board.LastMoveNumberFor(-1));
            board.TryDrop(2);Assert.AreEqual(9,board.LastMoveFor(-1));Assert.AreEqual(2,undo.LastMoveFor(-1));
            Assert.AreEqual(-1,new ConnectFourBoard().LastMoveFor(-1));
        }
    }
    public sealed partial class RankFlowTests
    {
        IEnumerator WaitOnline(Func<bool> condition,string step)
        {
            float end=Time.realtimeSinceStartup+8;
            while(!condition()&&Time.realtimeSinceStartup<end)yield return null;
            Assert.IsTrue(condition(),step);
        }
        [UnityTest] public IEnumerator MultiplayerNativeTransportPlaysWithASecondClient()
        {
            string endpoint=Environment.GetEnvironmentVariable("STAGEGAME_TEST_MULTIPLAYER_URL");
            if(string.IsNullOrEmpty(endpoint)){Assert.Ignore("Requires explicit local test server URL");yield break;}
            Assert.IsTrue(endpoint.StartsWith("ws://127.0.0.1:"),"Local test only");
            const string key="stagegame.multiplayer.token.v1";bool had=PlayerPrefs.HasKey(key);string old=PlayerPrefs.GetString(key,"");PlayerPrefs.DeleteKey(key);
            var online=app.online;string oldEndpoint=online.endpoint;var peerObject=new GameObject("MultiplayerTestPeer");var peer=peerObject.AddComponent<OnlineTransport>();
            OnlineState mine=null,theirs=null;bool peerAuth=false;
            Action<string> observe=json=>{var s=JsonUtility.FromJson<OnlineState>(json);if(s.type=="state")mine=s;};online.transport.Received+=observe;
            peer.Opened=()=>peer.Send(JsonUtility.ToJson(new OnlineCommand{type="auth",token=""}));
            peer.Received=json=>{var s=JsonUtility.FromJson<OnlineState>(json);if(s.type=="auth")peerAuth=true;if(s.type=="state")theirs=s;};
            try
            {
                online.endpoint=endpoint;online.Open();peer.Connect(endpoint);
                yield return WaitOnline(()=>mine!=null&&theirs!=null&&peerAuth,"Both clients authenticated");
                online.friendly.onClick.Invoke();yield return WaitOnline(()=>mine.status=="waiting","Invite room created");
                online.ratedToggle.onClick.Invoke();yield return WaitOnline(()=>!mine.rated,"Friendly rating toggle replicated");
                peer.Send(JsonUtility.ToJson(new OnlineCommand{type="join",invite=mine.invite,rated=false}));
                yield return WaitOnline(()=>mine.status=="playing"&&theirs.status=="playing","Both clients joined");
                foreach(int column in new[]{0,1,0,1,0,1,0})
                {
                    int seq=mine.seq;
                    if(mine.turn==mine.me){online.columns[column].onClick.Invoke();Assert.IsTrue(online.place.interactable);online.place.onClick.Invoke();}
                    else peer.Send(JsonUtility.ToJson(new OnlineCommand{type="move",roomId=theirs.roomId,round=theirs.round,seq=theirs.seq,column=column}));
                    yield return WaitOnline(()=>mine.seq==seq+1&&theirs.seq==seq+1,"Move replicated");
                }
                Assert.AreEqual("finished",mine.status);Assert.AreEqual(200,mine.rating);Assert.AreEqual(200,theirs.rating);
                yield return Capture("online-real-connection-result",app,390,844,scaledCanvas:true);
                peer.Send(JsonUtility.ToJson(new OnlineCommand{type="rematch",roomId=theirs.roomId,round=theirs.round}));
                yield return WaitOnline(()=>mine.opponentRequested,"Rematch notification");Assert.AreEqual(app.UiText("mp_rematch_request"),online.statusLabel.text);
                online.rematch.onClick.Invoke();yield return WaitOnline(()=>mine.round==2&&theirs.round==2,"Mutual rematch");
                peer.Send(JsonUtility.ToJson(new OnlineCommand{type="leave"}));yield return WaitOnline(()=>mine.opponentLeft,"Opponent departure");
                Assert.AreEqual(app.UiText("mp_opponent_left"),online.statusLabel.text);online.resultLeave.onClick.Invoke();
                yield return WaitOnline(()=>app.lobby.activeSelf,"Return to lobby");
            }
            finally
            {
                online.transport.Received-=observe;online.endpoint=oldEndpoint;peer.Close();UnityEngine.Object.Destroy(peerObject);
                if(had)PlayerPrefs.SetString(key,old);else PlayerPrefs.DeleteKey(key);PlayerPrefs.Save();
            }
        }
        [UnityTest] public IEnumerator LastOpponentHighlightResetsAndFollowsUndo()
        {
            app.StartStage(0);Assert.IsFalse(app.lastOpponentMove.gameObject.activeSelf);
            app.SelectColumn(3);app.PlaceSelected();yield return new WaitForSecondsRealtime(1.3f);
            var board=Field<ConnectFourBoard>("board");Assert.Greater(board.LastMoveFor(-1),-1);
            Assert.IsTrue(app.lastOpponentMove.gameObject.activeSelf);
            Assert.AreEqual(app.stones[board.LastMoveFor(-1)].rectTransform.anchoredPosition,app.lastOpponentMove.rectTransform.anchoredPosition);
            yield return Capture("opponent-last-move",app,390,844,scaledCanvas:true);
            app.SelectColumn(4);Assert.IsTrue(app.lastOpponentMove.gameObject.activeSelf,"Selecting a column does not place a stone");
            app.PlaceSelected();Assert.IsFalse(app.lastOpponentMove.gameObject.activeSelf,"Own placement immediately clears the opponent marker");
            Call("RefreshBoard");Assert.IsFalse(app.lastOpponentMove.gameObject.activeSelf);
            app.UndoTurn();Assert.IsTrue(app.lastOpponentMove.gameObject.activeSelf,"Undo restores the opponent's latest move");
            app.SelectColumn(4);app.PlaceSelected();yield return new WaitForSecondsRealtime(1.3f);
            Assert.IsTrue(app.lastOpponentMove.gameObject.activeSelf,"The next AI reply is highlighted");
            app.UndoTurn();Assert.IsTrue(app.lastOpponentMove.gameObject.activeSelf);
            app.UndoTurn();Assert.IsFalse(app.lastOpponentMove.gameObject.activeSelf);
            app.StartStage(0);Assert.IsFalse(app.lastOpponentMove.gameObject.activeSelf);
        }
        [UnityTest] public IEnumerator MultiplayerHighlightClearsOnPlacementAndReturnsOnReply()
        {
            var online=app.online;online.Open();
            // Use the saved UI with controlled server messages; no public connection or stored identity.
            typeof(OnlineGameController).GetField("connected",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).SetValue(online,true);
            for(int me=0;me<2;me++)for(int first=0;first<2;first++)
            {
                var state=new OnlineState{type="state",status="playing",roomId="highlight-"+me+first,round=1,me=me,first=first,turn=first,rankKey="mp_rank_2",otherRankKey="mp_rank_2",board=new int[42]};
                online.Receive(JsonUtility.ToJson(state));Assert.IsFalse(online.lastOpponentMove.gameObject.activeSelf);
                state.seq=first==me?2:1;state.turn=me;state.lastOpponentMove=1;state.board[1]=me==0?-1:1;
                if(first==me)state.board[0]=me==0?1:-1;
                online.Receive(JsonUtility.ToJson(state));Assert.IsTrue(online.lastOpponentMove.gameObject.activeSelf);
                online.columns[3].onClick.Invoke();Assert.IsTrue(online.lastOpponentMove.gameObject.activeSelf);
                online.place.onClick.Invoke();Assert.IsFalse(online.lastOpponentMove.gameObject.activeSelf,"Hide while the placement is in flight");
                online.Receive(JsonUtility.ToJson(state));Assert.IsFalse(online.lastOpponentMove.gameObject.activeSelf,"An unchanged snapshot must not restore a pending marker");
                online.Receive("{\"type\":\"error\",\"code\":\"INVALID_MOVE\"}");Assert.IsTrue(online.lastOpponentMove.gameObject.activeSelf,"Rejected placement restores the marker");
                online.place.onClick.Invoke();state.seq++;state.turn=1-me;state.board[3]=me==0?1:-1;
                online.Receive(JsonUtility.ToJson(state));Assert.IsFalse(online.lastOpponentMove.gameObject.activeSelf,"Accepted own move stays unmarked");
                online.Refresh();Assert.IsFalse(online.lastOpponentMove.gameObject.activeSelf);
                state.seq++;state.turn=me;state.lastOpponentMove=8;state.board[8]=me==0?-1:1;
                online.Receive(JsonUtility.ToJson(state));Assert.IsTrue(online.lastOpponentMove.gameObject.activeSelf);
                Assert.AreEqual(online.stones[8].rectTransform.anchoredPosition,online.lastOpponentMove.rectTransform.anchoredPosition);
                state.status="finished";state.result="loss";
                online.Receive(JsonUtility.ToJson(state));Assert.IsTrue(online.lastOpponentMove.gameObject.activeSelf,"Opponent's final move remains marked");
            }
            yield return null;
        }
        [UnityTest] public IEnumerator MultiplayerSavedPanelShowsInviteRatingStarterRematchAndDeparture()
        {
            var online=app.online;Assert.NotNull(online);online.Open();
            foreach(string language in new[]{"ko","en"})
            {
                app.SetLanguage(language);
                foreach(var size in new[]{new Vector2Int(390,844),new Vector2Int(320,568)})
                {
                    online.Receive(JsonUtility.ToJson(new OnlineState{type="state",status="idle",rankKey="mp_rank_2",rating=200}));
                    yield return Capture(language+"-"+size.x+"-online-menu",app,size.x,size.y,scaledCanvas:true);
                    online.Receive(JsonUtility.ToJson(new OnlineState{type="state",status="waiting",rankKey="mp_rank_2",roomId="test",invite=new string('a',32),rated=false}));
                    Assert.AreEqual(app.UiText("mp_rated_off"),online.ratedLabel.text);Assert.IsTrue(online.share.gameObject.activeSelf);
                    online.OnShareResult("copied");Assert.AreEqual(app.UiText("mp_copied"),online.description.text);
                    yield return Capture(language+"-"+size.x+"-online-invite",app,size.x,size.y,scaledCanvas:true);
                    online.Receive(JsonUtility.ToJson(new OnlineState{type="state",status="playing",rankKey="mp_rank_2",otherRankKey="mp_rank_3",otherRating=300,roomId="test",round=1,first=0,me=0,board=new int[42],deadline=60000}));
                    StringAssert.Contains(app.UiText("mp_you_first"),online.firstPlayer.text);
                    Assert.AreEqual(app.UiText("mp_rated_on"),online.resultLabel.text,"Invite feedback must not hide game status");
                    online.OnShareResult("copied");Assert.AreEqual(app.UiText("mp_rated_on"),online.resultLabel.text,"Late share callback must not hide game status");
                    yield return Capture(language+"-"+size.x+"-online-game",app,size.x,size.y,scaledCanvas:true);
                    online.Receive(JsonUtility.ToJson(new OnlineState{type="state",status="finished",rankKey="mp_rank_2",otherRankKey="mp_rank_3",roomId="test",round=1,result="win",delta=24,rating=224,board=new int[42],opponentRequested=true}));
                    Assert.AreEqual(app.UiText("mp_rematch_request"),online.statusLabel.text);
                    yield return Capture(language+"-"+size.x+"-online-rematch",app,size.x,size.y,scaledCanvas:true);
                    online.Receive(JsonUtility.ToJson(new OnlineState{type="state",status="finished",rankKey="mp_rank_2",otherRankKey="mp_rank_3",roomId="test",result="win",board=new int[42],opponentLeft=true}));
                    Assert.AreEqual(app.UiText("mp_opponent_left"),online.statusLabel.text);Assert.IsFalse(online.rematch.interactable);
                    yield return Capture(language+"-"+size.x+"-online-left",app,size.x,size.y,scaledCanvas:true);
                }
            }
            online.close.onClick.Invoke();Assert.IsTrue(app.lobby.activeSelf);
        }
    }
}
