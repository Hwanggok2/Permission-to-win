using System;
using System.Linq;
using StageGame;
using UnityEngine;
using UnityEngine.UI;

static partial class Program
{
    // Deterministic logical coroutine scheduler, not Unity frame timing or renderer proof.
    static void Tick(GameApp app,float seconds=.05f) { Time.unscaledDeltaTime=seconds; Time.realtimeSinceStartup+=seconds; app.AdvanceCoroutines(); }
    static void AdvanceToResult(GameApp app)
    {
        for(int i=0;i<30 && !app.result.activeSelf;i++) Tick(app);
        Check(app.result.activeSelf,"presentation did not reach result within1.5 simulated active seconds");
    }
    static ConnectFourBoard BeginTerminal(GameApp app,int outcome)
    {
        var board=Replay(outcome==0?new[]{0,6,1,6,2,5,3}:outcome==1?new[]{0,6,1,6,2,6,4,6}:Draw);
        Set(app,"board",board); Call(app,"RefreshBoard"); Call(app,"Finish"); return board;
    }
    static bool Same(Vector2 a,Vector2 b)=>Math.Abs(a.x-b.x)<.0001f && Math.Abs(a.y-b.y)<.0001f;
    static bool Unit(Transform t)=>Math.Abs(t.localScale.x-1)<.0001f && Math.Abs(t.localScale.y-1)<.0001f && Math.Abs(t.localScale.z-1)<.0001f;
    static void CheckLocked(GameApp app,DelayedAds ads,ConnectFourBoard board)
    {
        string before=BoardState(board);int rewards=ads.Rewards.Count;
        Check(app.columns.All(c=>!c.interactable) && !app.hint.interactable && !app.pause.interactable,"resolved board input flags active");
        Call(app,"Drop",0); app.hint.onClick.Invoke(); app.pause.onClick.Invoke();
        Check(BoardState(board)==before && ads.Rewards.Count==rewards && !app.confirm.activeSelf,"terminal direct input changed board/reward/pause");
    }
    static void RunGraphics()
    {
        Run("graphics.both_winners_timeline_front_order_input_and_ad_once","actual GameApp; logical .05s scheduler and sprite identity/sibling-list doubles",()=>{
            for(int outcome=0;outcome<2;outcome++)
            {
                var app=MakeApp();var p=(ProgressStore)Get(app,"progress");p.Data.completedMatches=3;
                var ads=new DelayedAds();Set(app,"ads",ads);var board=BeginTerminal(app,outcome);
                var last=app.stones[board.LastMove].rectTransform;var position=(Vector2)Get(app,"animatedPosition");int original=(int)Get(app,"animatedSibling");
                Check(Math.Abs(last.localScale.x-1.45f)<.0001f && last.GetSiblingIndex()==41 && Math.Abs(last.anchoredPosition.y-position.y-34)<.0001f,"initial scale/lift/front order missing");
                Check(!app.result.activeSelf && ads.Interstitials.Count==0 && p.Data.completedMatches==4,"result/ad too early or result not saved once");
                int routines=app.Coroutines.Count;Call(app,"Finish");Check(app.Coroutines.Count==routines && p.Data.completedMatches==4,"duplicate finish starts presentation/saves");
                CheckLocked(app,ads,board);
                for(int i=0;i<7;i++)Tick(app);
                Check(!app.result.activeSelf && !((bool)Get(app,"winningRevealed")) && last.localScale.x>1 && last.GetSiblingIndex()==41,"400ms presentation shortened");
                Tick(app);
                Check((bool)Get(app,"winningRevealed") && Unit(last) && Same(last.anchoredPosition,position) && last.GetSiblingIndex()==original,"placement restoration or reveal timing wrong");
                for(int i=0;i<42;i++)
                {
                    int token=board[i%7,i/7];bool won=board.WinningCells.Contains(i);
                    var expected=token==0?app.theme.hole:token==1?(won?app.theme.playerWin:app.theme.player):(won?app.theme.opponentWin:app.theme.opponent);
                    Check(app.stones[i].sprite==expected && app.stoneLabels[i].text=="","board token/winning sprite mismatch");
                }
                for(int i=0;i<8;i++)Tick(app);
                Check(!app.result.activeSelf && ads.Interstitials.Count==0,"500ms line observation shortened");
                AdvanceToResult(app);
                Check(ads.Interstitials.Count==1 && !app.retry.interactable && !app.resultLobby.interactable,"ad boundary missing or actions enabled early");
                Check(!app.pause.gameObject.activeSelf && !app.hint.gameObject.activeSelf && !app.nextStage.gameObject.activeSelf && !app.resultClose.gameObject.activeSelf,"unwanted result control visible");
                CheckLocked(app,ads,board);
                ads.Interstitials[0](AdResult.Completed);ads.Interstitials[0](AdResult.Completed);Tick(app);
                Check(app.retry.interactable && app.resultLobby.interactable && !app.nextStage.interactable && !app.resultClose.interactable && !(bool)Get(app,"resolving"),"settled actions/state wrong");
                Check(p.Data.completedMatches==4 && p.Data.lastInterstitialMatch==4,"duplicate callback changed result count");
            }
        });
        Run("graphics.application_pause_freezes_active_presentation_time","actual pause handler; clock doubles",()=>{
            foreach(float beforePause in new[]{.1f,.45f})
            {
                var app=MakeApp();var board=BeginTerminal(app,0);for(int i=0;i<(int)Math.Round(beforePause/.05);i++)Tick(app);
                var stone=app.stones[board.LastMove].rectTransform;float scale=stone.localScale.x;bool reveal=(bool)Get(app,"winningRevealed");
                Call(app,"OnApplicationPause",true);for(int i=0;i<20;i++)Tick(app);
                Check(!app.result.activeSelf && Math.Abs(stone.localScale.x-scale)<.0001f && (bool)Get(app,"winningRevealed")==reveal,"paused presentation advanced");
                Call(app,"OnApplicationPause",false);AdvanceToResult(app);
                Check(app.retry.interactable && Unit(stone),"resume did not settle NoFill result");
            }
        });
        Run("graphics.cancel_at_animation_observation_or_ad_restores_and_ignores_stale","actual cancel methods; delayed SDK and hierarchy doubles",()=>{
            foreach(int phase in new[]{0,1,2})foreach(bool lobby in new[]{true,false})
            {
                var app=MakeApp();var p=(ProgressStore)Get(app,"progress");p.Data.completedMatches=3;var ads=new DelayedAds();Set(app,"ads",ads);
                var board=BeginTerminal(app,0);var last=app.stones[board.LastMove].rectTransform;var original=(Vector2)Get(app,"animatedPosition");int sibling=(int)Get(app,"animatedSibling");
                if(phase==1)for(int i=0;i<10;i++)Tick(app); if(phase==2)AdvanceToResult(app);
                if(lobby)app.ShowLobby();else app.StartStage(0);
                string next=(string)Get(app,"matchId");int count=p.Data.completedMatches;
                Check(Unit(last) && Same(last.anchoredPosition,original) && last.GetSiblingIndex()==sibling && !(bool)Get(app,"resolving"),"cancel did not restore transform/order");
                foreach(var callback in ads.Interstitials)callback(AdResult.Completed);for(int i=0;i<30;i++)Tick(app);
                Check(!app.result.activeSelf && (string)Get(app,"matchId")==next && p.Data.completedMatches==count && p.Data.lastInterstitialMatch==0,"cancelled presentation/ad changed next state");
                if(!lobby)Check(app.pause.gameObject.activeSelf && app.pause.interactable && app.hint.gameObject.activeSelf && app.columns.Any(c=>c.interactable),"new match did not restore controls");
            }
            var destroyed=MakeApp();var b=BeginTerminal(destroyed,1);var t=destroyed.stones[b.LastMove].rectTransform;int order=(int)Get(destroyed,"animatedSibling");var pos=(Vector2)Get(destroyed,"animatedPosition");
            Call(destroyed,"OnDestroy");for(int i=0;i<25;i++)Tick(destroyed);
            Check(Unit(t) && Same(t.anchoredPosition,pos) && t.GetSiblingIndex()==order && !destroyed.result.activeSelf,"destroy during animation failed restoration");
        });
        Run("graphics.lobby_selects_highest_unlocked_and_retry_same_stage","actual result button listeners and memory reload",()=>{
            foreach(int initial in new[]{1,10,30})foreach(int outcome in new[]{0,1,2})
            {
                var app=MakeApp();var p=(ProgressStore)Get(app,"progress");p.Data.unlockedStage=initial;p.Data.highestClearedStage=initial-1;app.StartStage(initial-1);FinishFixture(app,outcome);
                int unlocked=outcome==0?Math.Min(30,initial+1):initial;
                Check(p.Data.unlockedStage==unlocked && p.Data.highestClearedStage==(outcome==0?initial:initial-1),"result progression mismatch");
                Check(ClickEnabled(app.resultLobby) && app.lobby.activeSelf && !app.match.activeSelf && (int)Get(app,"selected")==unlocked-1,"lobby selection/start wrong");
                Check(new ProgressStore(30).Data.unlockedStage==unlocked,"result not persisted");
            }
            var replay=MakeApp();var progress=(ProgressStore)Get(replay,"progress");progress.Data.unlockedStage=20;progress.Data.highestClearedStage=19;
            replay.StartStage(0);FinishFixture(replay,0);ClickEnabled(replay.resultLobby);
            Check((int)Get(replay,"selected")==19 && progress.Data.highestClearedStage==19,"lower replay moved highest lobby selection/clear");
        });
        Run("graphics.draw_skips_win_animation_but_settles_two_actions_once","actual terminal draw and result state; delayed ad",()=>{
            var app=MakeApp();var p=(ProgressStore)Get(app,"progress");p.Data.completedMatches=3;var ads=new DelayedAds();Set(app,"ads",ads);var board=BeginTerminal(app,2);
            Check(app.result.activeSelf && Get(app,"animatedStone")==null && !(bool)Get(app,"winningRevealed") && ads.Interstitials.Count==1,"draw invoked winner animation or skipped settlement");
            CheckLocked(app,ads,board);Call(app,"Finish");ads.Interstitials[0](AdResult.Unavailable);Tick(app);
            Check(p.Data.completedMatches==4 && p.Data.highestClearedStage==0 && app.retry.interactable && app.resultLobby.interactable,"draw counted/advanced or remains blocked");
            Check(!app.pause.gameObject.activeSelf && !app.nextStage.gameObject.activeSelf && !app.resultClose.gameObject.activeSelf,"draw leaves extra control");
        });
        Run("graphics.actual_player_drop_and_ai_think_enter_finish_sequence","actual GameApp Drop/Think plus deterministic terminal opportunities",()=>{
            var player=MakeApp();Set(player,"board",Replay(new[]{0,6,1,6,2,5}));Call(player,"Drop",3);
            Check((bool)Get(player,"resolving") && ((ConnectFourBoard)Get(player,"board")).Winner==1,"player winning drop bypasses sequence");AdvanceToResult(player);
            var ai=MakeApp();Set(ai,"board",Replay(new[]{0,6,1,6,2,6,4}));var iterator=(System.Collections.IEnumerator)Call(ai,"Think",false);
            // Start the returned real iterator through the same test scheduler as MonoBehaviour.
            var coroutine=new Coroutine{Iterator=iterator};coroutine.Stack.Push(iterator);ai.Coroutines.Add(coroutine);coroutine.Step();
            for(int i=0;i<200 && !(bool)Get(ai,"resolving");i++)Tick(ai,.01f);
            Check(((ConnectFourBoard)Get(ai,"board")).Winner==-1 && (bool)Get(ai,"resolving"),"AI winning move bypasses sequence");AdvanceToResult(ai);
        });
    }
}
