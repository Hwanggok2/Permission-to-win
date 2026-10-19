using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace StageGame.Tests
{
    public sealed partial class RankFlowTests
    {
        static T OnlineField<T>(OnlineGameController online,string name)=>(T)typeof(OnlineGameController).GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(online);
        static void SetOnline(OnlineGameController online,string name,object value)=>typeof(OnlineGameController).GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(online,value);
        static OnlineState FeedbackState(int me=0)=>new OnlineState{type="state",status="playing",roomId="feedback",round=1,me=me,first=0,turn=0,rankKey="mp_rank_2",otherRankKey="mp_rank_2",board=new int[42],deadline=60000};
        static OnlineState BeforeFinal(int me=0)
        {
            var s=FeedbackState(me);s.seq=6;s.board[0]=s.board[7]=s.board[14]=1;s.board[1]=s.board[8]=s.board[15]=-1;s.lastOpponentMove=me==0?15:14;return s;
        }
        static void WinningState(OnlineState s)
        {
            s.status="finished";s.reason="four";s.seq=7;s.board[21]=1;s.result=s.me==0?"win":"loss";s.delta=s.me==0?16:-16;s.rating=200+s.delta;s.otherRating=200-s.delta;
            if(s.me==1)s.lastOpponentMove=21;
        }
        [UnityTest] public IEnumerator MultiplayerMusicContinuesAcrossMenuGameResultAndFocus()
        {
            Progress.Data.musicMuted=false;Progress.Data.soundEnabled=false;Call("OnApplicationFocus",true);app.ShowLobby();yield return new WaitForSecondsRealtime(.1f);
            Assert.IsTrue(app.musicSource.isPlaying);float at=app.musicSource.time;
            app.online.Open();yield return new WaitForSecondsRealtime(.12f);Assert.IsTrue(app.musicSource.isPlaying);Assert.GreaterOrEqual(app.musicSource.time,at);
            var s=FeedbackState();app.online.Receive(JsonUtility.ToJson(s));yield return null;Assert.IsTrue(app.musicSource.isPlaying);
            app.musicSource.time=2;var signal=new AudioSignal();yield return MeasureListenerOutput(signal,.35f);Assert.Greater(signal.rms,.00001f,"Online music must produce output samples.");
            System.IO.Directory.CreateDirectory("Evidence/multiplayer-feedback-20261019");System.IO.File.WriteAllText("Evidence/multiplayer-feedback-20261019/music-output.json",JsonUtility.ToJson(signal,true));
            s.status="finished";s.result="draw";s.reason="draw";app.online.Receive(JsonUtility.ToJson(s));yield return null;Assert.IsTrue(app.musicSource.isPlaying);
            s.round=2;s.status="playing";app.online.Receive(JsonUtility.ToJson(s));yield return null;Assert.IsTrue(app.musicSource.isPlaying);
            Call("OnApplicationFocus",false);yield return null;Assert.IsFalse(app.musicSource.isPlaying);
            Call("OnApplicationFocus",true);yield return null;Assert.IsTrue(app.musicSource.isPlaying);
            Progress.Data.musicMuted=true;Call("RefreshMusic");yield return null;Assert.IsFalse(app.musicSource.isPlaying);
            app.online.resultLeave.onClick.Invoke();yield return null;Assert.IsTrue(app.lobby.activeSelf);Assert.IsFalse(app.musicSource.isPlaying);
        }
        [UnityTest] public IEnumerator MultiplayerPlacementIsImmediateAndReconcilesWithoutDuplicateSound()
        {
            var online=app.online;online.Open();OnlineConnected();Progress.Data.soundEnabled=true;Call("OnApplicationFocus",true);
            var s=FeedbackState();online.Receive(JsonUtility.ToJson(s));online.columns[3].onClick.Invoke();
            int frame=Time.frameCount;online.place.onClick.Invoke();
            Assert.AreEqual(frame,Time.frameCount);Assert.IsTrue(online.stones[3].enabled,"Provisional stone is visible in the input frame.");
            Assert.AreEqual(0,OnlineField<OnlineState>(online,"state").board[3],"Do not mutate authoritative state.");
            Assert.AreEqual(0,OnlineField<OnlineState>(online,"state").seq);Assert.IsFalse(online.place.interactable);
            Assert.AreEqual(app.UiText("mp_move_pending"),online.turn.text);
            online.columns[4].onClick.Invoke();online.place.onClick.Invoke();Assert.IsFalse(online.stones[4].enabled,"Double input cannot create a second stone.");
            online.Receive(JsonUtility.ToJson(s));Assert.IsTrue(online.stones[3].enabled,"Unchanged snapshot preserves pending stone.");
            yield return new WaitForSecondsRealtime(.2f);Assert.IsTrue(online.stones[3].enabled);
            app.audioSource.Stop();s.board[3]=1;s.seq=1;s.turn=1;online.Receive(JsonUtility.ToJson(s));
            Assert.IsFalse(app.audioSource.isPlaying,"Acknowledgement cannot replay placement audio.");Assert.IsFalse(OnlineField<bool>(online,"sentMove"));
            online.Receive(JsonUtility.ToJson(FeedbackState()));Assert.AreEqual(1,OnlineField<OnlineState>(online,"state").seq,"Older snapshot ignored.");
            s.board[2]=-1;s.seq=2;s.turn=0;s.lastOpponentMove=2;online.Receive(JsonUtility.ToJson(s));Assert.IsTrue(app.audioSource.isPlaying,"Opponent placement has sound.");
            online.columns[4].onClick.Invoke();online.place.onClick.Invoke();Assert.IsTrue(online.stones[4].enabled);
            online.Receive("{\"type\":\"error\",\"code\":\"INVALID_MOVE\"}");Assert.IsFalse(online.stones[4].enabled);Assert.IsTrue(online.lastOpponentMove.gameObject.activeSelf);
            online.place.onClick.Invoke();Assert.IsTrue(online.stones[4].enabled);
            typeof(OnlineGameController).GetMethod("Disconnected",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(online,new object[]{1006});SetOnline(online,"reconnectAt",-1f);
            Assert.IsFalse(online.stones[4].enabled);Assert.IsFalse(online.place.interactable);
            s.round=2;s.seq=0;s.board=new int[42];online.Receive(JsonUtility.ToJson(s));Assert.IsFalse(online.stones[4].enabled);
        }
        [UnityTest] public IEnumerator MultiplayerFinalStoneAndRatingAnimateOnceForBothPlayers()
        {
            var online=app.online;online.Open();OnlineConnected();
            for(int me=0;me<2;me++)
            {
                var s=BeforeFinal(me);s.roomId="final-"+me;online.Receive(JsonUtility.ToJson(s));WinningState(s);online.Receive(JsonUtility.ToJson(s));
                Assert.AreEqual(app.UiText(me==0?"mp_outcome_win":"mp_outcome_loss"),online.turn.text);Assert.IsTrue(online.winningLine.gameObject.activeSelf);
                Assert.IsTrue(online.rematch.interactable);Assert.IsTrue(online.resultLeave.gameObject.activeSelf);
                var routine=OnlineField<Coroutine>(online,"feedbackRoutine");yield return new WaitForSecondsRealtime(.22f);
                Assert.Greater(online.stones[21].rectTransform.localScale.x,1.4f);
                s.opponentRequested=true;online.Receive(JsonUtility.ToJson(s));Assert.AreSame(routine,OnlineField<Coroutine>(online,"feedbackRoutine"));
                Assert.AreEqual(app.UiText("mp_rematch_request"),online.statusLabel.text);
                yield return new WaitForSecondsRealtime(1.1f);
                Assert.AreEqual(Vector3.one,online.stones[21].rectTransform.localScale);Assert.IsNull(OnlineField<Coroutine>(online,"feedbackRoutine"));
                StringAssert.Contains(s.rating.ToString(),online.resultLabel.text);StringAssert.Contains(s.delta.ToString("+0;-0;0"),online.resultLabel.text);
                online.Receive(JsonUtility.ToJson(s));Assert.IsNull(OnlineField<Coroutine>(online,"feedbackRoutine"));
            }
        }
        [UnityTest] public IEnumerator MultiplayerPresentationCancelsOnRematchLeaveAndFinishedReconnect()
        {
            var online=app.online;online.Open();OnlineConnected();var s=BeforeFinal();online.Receive(JsonUtility.ToJson(s));WinningState(s);online.Receive(JsonUtility.ToJson(s));
            yield return new WaitForSecondsRealtime(.15f);Assert.Greater(online.stones[21].rectTransform.localScale.x,1);
            var next=FeedbackState();next.round=2;online.Receive(JsonUtility.ToJson(next));
            Assert.AreEqual(Vector3.one,online.stones[21].rectTransform.localScale);Assert.IsFalse(online.finalMove.gameObject.activeSelf);Assert.IsFalse(online.winningLine.gameObject.activeSelf);
            online.Receive(JsonUtility.ToJson(s));Assert.AreEqual(2,OnlineField<OnlineState>(online,"state").round,"Previous round cannot overwrite rematch.");
            s.roomId="restored-finished";online.Receive(JsonUtility.ToJson(s));Assert.IsNull(OnlineField<Coroutine>(online,"feedbackRoutine"));StringAssert.Contains("216",online.resultLabel.text);
            s=BeforeFinal();s.roomId="leave-final";online.Receive(JsonUtility.ToJson(s));WinningState(s);online.Receive(JsonUtility.ToJson(s));yield return new WaitForSecondsRealtime(.15f);
            online.resultLeave.onClick.Invoke();Assert.AreEqual(Vector3.one,online.stones[21].rectTransform.localScale);Assert.IsNull(OnlineField<Coroutine>(online,"feedbackRoutine"));
            online.Receive("{\"type\":\"state\",\"status\":\"idle\",\"rankKey\":\"mp_rank_2\"}");Assert.IsTrue(app.lobby.activeSelf);
        }
        [UnityTest] public IEnumerator MultiplayerFeedbackResultsFitAndRemainReadable()
        {
            var online=app.online;online.Open();OnlineConnected();
            foreach(string language in new[]{"ko","en"})foreach(int viewport in new[]{0,1,2})
            {
                app.SetLanguage(language);int width=viewport==0?1080:viewport==1?390:320,height=viewport==0?1920:viewport==1?844:568;bool host=viewport==2;
                foreach(string outcome in new[]{"win","loss","draw","unrated","left","cancelled"})
                {
                    var s=BeforeFinal(outcome=="loss"?1:0);s.roomId=language+viewport+outcome;WinningState(s);s.result=outcome=="loss"?"loss":outcome=="draw"||outcome=="cancelled"?"draw":"win";
                    if(outcome=="draw"||outcome=="cancelled"){s.reason=outcome=="cancelled"?"server_pause":"draw";s.delta=0;s.rating=200;}
                    if(outcome=="unrated"){s.rated=false;s.delta=0;s.rating=200;}
                    if(outcome=="left")s.opponentLeft=true;
                    online.Receive(JsonUtility.ToJson(s));
                    yield return Capture("feedback-"+language+"-"+viewport+"-"+outcome,app,width,height,host,validate:()=>AssertOnlineFits(1),directory:"Evidence/multiplayer-feedback-20261019/screens",scaledCanvas:true);
                }
            }
        }
        [UnityTest] public IEnumerator MultiplayerReconnectAndDisableDoNotReplayFinishedFeedback()
        {
            var online=app.online;online.Open();OnlineConnected();var s=BeforeFinal();online.Receive(JsonUtility.ToJson(s));
            typeof(OnlineGameController).GetMethod("Disconnected",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(online,new object[]{1006});SetOnline(online,"reconnectAt",-1f);
            OnlineConnected();WinningState(s);online.Receive(JsonUtility.ToJson(s));
            Assert.IsNull(OnlineField<Coroutine>(online,"feedbackRoutine"));Assert.AreEqual(Vector3.one,online.stones[21].rectTransform.localScale);
            StringAssert.Contains("216",online.resultLabel.text);Assert.IsFalse(online.finalMove.gameObject.activeSelf);
            s=BeforeFinal();s.round=2;online.Receive(JsonUtility.ToJson(s));WinningState(s);online.Receive(JsonUtility.ToJson(s));yield return new WaitForSecondsRealtime(.2f);
            Assert.Greater(online.stones[21].rectTransform.localScale.x,1.4f);
            app.gameObject.SetActive(false);Assert.AreEqual(Vector3.one,online.stones[21].rectTransform.localScale);Assert.IsNull(OnlineField<Coroutine>(online,"feedbackRoutine"));
            app.gameObject.SetActive(true);yield return null;StringAssert.Contains("216",online.resultLabel.text);
        }
        [UnityTest] public IEnumerator MultiplayerPendingTimeoutRecoversWithoutResending()
        {
            var online=app.online;online.Open();OnlineConnected();var s=FeedbackState();online.Receive(JsonUtility.ToJson(s));online.columns[3].onClick.Invoke();online.place.onClick.Invoke();
            SetOnline(online,"pendingAt",Time.unscaledTime-6);
            typeof(OnlineGameController).GetMethod("Update",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(online,null);SetOnline(online,"reconnectAt",-1f);
            Assert.IsFalse(online.stones[3].enabled);Assert.IsFalse(OnlineField<bool>(online,"sentMove"));Assert.IsFalse(online.place.interactable);
            OnlineConnected();s.seq=1;s.board[3]=1;s.turn=1;online.Receive(JsonUtility.ToJson(s));Assert.IsTrue(online.stones[3].enabled);Assert.IsFalse(online.place.interactable);Assert.IsFalse(OnlineField<bool>(online,"sentMove"));
            yield return null;
        }
        [UnityTest] public IEnumerator MultiplayerEdgeFinalZoomStaysInsidePortraitScreens()
        {
            var online=app.online;online.Open();OnlineConnected();
            foreach(int column in new[]{0,6})foreach(int width in new[]{390,320})
            {
                int other=column==0?1:5;var s=FeedbackState(column==0?0:1);s.roomId="edge-"+column+width;s.seq=6;
                for(int row=0;row<3;row++){s.board[row*7+column]=1;s.board[row*7+other]=-1;}
                online.Receive(JsonUtility.ToJson(s));s.seq=7;s.status="finished";s.reason="four";s.result=column==0?"win":"loss";s.board[21+column]=1;
                online.Receive(JsonUtility.ToJson(s));yield return new WaitForSecondsRealtime(.22f);
                yield return Capture("zoom-"+column+"-"+width,app,width,width==390?844:568,validate:()=>
                {
                    Assert.Greater(online.stones[21+column].rectTransform.localScale.x,1.2f);AssertOnlineFits(1);
                },directory:"Evidence/multiplayer-feedback-20261019/screens",scaledCanvas:true);
                yield return new WaitForSecondsRealtime(.7f);Assert.AreEqual(Vector3.one,online.stones[21+column].rectTransform.localScale);
            }
        }
    }
}
