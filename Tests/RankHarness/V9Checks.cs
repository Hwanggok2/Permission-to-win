using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using StageGame;
using UnityEngine;

static partial class Program
{
    static string MatchState(RankGameApp app) => State(Board(app))+"|"+Get<string>(app,"matchId")+"|"+Get<int>(app,"activeStage")+"|"+
        Get<int>(app,"selectedColumn")+"|"+string.Join("/",Get<List<ConnectFourBoard>>(app,"undoHistory").Select(State))+"|"+
        Gate(app).Granted+"|"+JsonUtility.ToJson(Progress(app).Data);
    static int LiveToasts(RankGameApp app) => app.Coroutines.Count(c=>!c.Stopped&&c.Iterator.GetType().Name.Contains("TurnToastSequence"));
    static void AcceptPanel(RankGameApp app)
    {
        Check(app.actionConfirm.activeSelf,"action confirmation required");Click(app.actionAccept);
        Check(!app.actionConfirm.activeSelf,"accepted action must close panel");
    }
    static void RegisterV9Tests()
    {
        Run("v9.data_approved_delta_and_frozen_hashes",()=>
        {
            var hashes=new Dictionary<string,string>{{"stages","6f76d1575f71fd0240cf7b5774bd2a3fed41fb1828bd0cb85464f34e72122e7e"},{"strings","4fa6c54394fd59846131794852ea025d4fa7dcb6725ab2a1dfb7435f1fa50de4"},{"ads","d19f181faf52db28287bbed1b42291770dc3bf83d35e5cca8af4a6dcce9e194a"}};
            foreach(var row in hashes)Check(Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(Evidence,"ClientData",row.Key+".json")))).Equals(row.Value,StringComparison.OrdinalIgnoreCase),"approved final hash "+row.Key);
            var oldRows=JsonUtility.FromJson<StringFile>(File.ReadAllText(Path.Combine(Evidence,"V8Data","strings.json"))).strings.ToDictionary(s=>s.key);
            var newRows=JsonUtility.FromJson<StringFile>(StringsJson).strings.ToDictionary(s=>s.key);
            var revisions=new HashSet<string>();var additions=new HashSet<string>();
            foreach(string file in new[]{"v9-ui-changes.json","story-changes.json"})
            {
                using var changes=JsonDocument.Parse(File.ReadAllText(Path.Combine(Evidence,"AuditInputs",file)));
                foreach(var row in changes.RootElement.GetProperty("revisions").EnumerateArray())
                {
                    string key=row.GetProperty("key").GetString();Check(revisions.Add(key),"duplicate approved revision");
                    Check(oldRows[key].ko==row.GetProperty("before").GetProperty("ko").GetString()&&oldRows[key].en==row.GetProperty("before").GetProperty("en").GetString(),"approved before "+key);
                    Check(newRows[key].ko==row.GetProperty("after").GetProperty("ko").GetString()&&newRows[key].en==row.GetProperty("after").GetProperty("en").GetString(),"approved after "+key);
                }
                foreach(var row in changes.RootElement.GetProperty("additions").EnumerateArray())
                {
                    string key=row.GetProperty("key").GetString();Check(additions.Add(key)&&!oldRows.ContainsKey(key),"new approved key");
                    Check(newRows[key].ko==row.GetProperty("ko").GetString()&&newRows[key].en==row.GetProperty("en").GetString(),"approved new text "+key);
                }
            }
            Check(revisions.Count==37&&additions.Count==9&&oldRows.Count==228&&newRows.Count==237,"v9 delta counts");
            int preserved=0;foreach(var before in oldRows.Values)if(!revisions.Contains(before.key)){Check(newRows[before.key].ko==before.ko&&newRows[before.key].en==before.en,"unapproved string mutation "+before.key);preserved++;}
            Check(preserved==191&&newRows.Keys.Except(oldRows.Keys).ToHashSet().SetEquals(additions),"preserved/new key coverage");
            using var oldStages=JsonDocument.Parse(File.ReadAllText(Path.Combine(Evidence,"V8Data","stages.json")));using var newStages=JsonDocument.Parse(StagesJson);
            var current=newStages.RootElement.GetProperty("stages").EnumerateArray().ToArray();int changedFields=0;
            foreach(var before in oldStages.RootElement.GetProperty("stages").EnumerateArray())
            {
                int id=before.GetProperty("id").GetInt32();var after=current[id-1];
                foreach(var field in before.EnumerateObject())if(field.Value.GetRawText()!=after.GetProperty(field.Name).GetRawText())
                {Check(id==1&&new[]{"aiIntroKey","aiWinKey","aiLoseKey"}.Contains(field.Name),"unapproved stage mutation "+id+"/"+field.Name);changedFields++;}
            }
            Check(changedFields==3,"exact stage1 story reference delta");
            Check(File.ReadAllBytes(Path.Combine(Evidence,"V8Data","ads.json")).SequenceEqual(File.ReadAllBytes(Path.Combine(Evidence,"ClientData","ads.json"))),"ads changed");
        });
        Run("v9.confirm_cancel_player_state_and_escape",()=>
        {
            foreach(bool escape in new[]{false,true})
            {
                var app=Opening(new[]{3,2});PlaceAndReply(app,4);app.SelectColumn(0);string before=MatchState(app);string save=PlayerPrefs.GetString(SaveKey);
                Click(app.matchRetry);Check(app.actionConfirm.activeSelf&&MatchState(app)==before,"request changed player state");
                Check(!app.place.interactable&&!app.undo.interactable&&!app.matchRetry.interactable&&!app.pause.interactable,"modal input not suspended");
                app.SelectColumn(1);app.PlaceSelected();app.UndoTurn();Call(app,"RequestHint");Check(MatchState(app)==before,"background gameplay crossed confirmation");
                if(escape){Input.EscapePressed=true;Call(app,"Update");}else Click(app.actionCancel);
                Check(!app.actionConfirm.activeSelf&&MatchState(app)==before&&PlayerPrefs.GetString(SaveKey)==save,"cancel did not preserve state");
                Check(app.place.interactable&&app.movePreview.gameObject.activeSelf&&!app.confirm.activeSelf,"cancel did not restore controls/preview");
                Check(!app.turnToast.gameObject.activeSelf&&LiveToasts(app)==0,"cancel replayed old toast");
            }
        });
        Run("v9.confirm_ai_search_cancel_resume",()=>
        {
            var app=Opening(new[]{3,2});app.SelectColumn(4);app.PlaceSelected();var search=Get<Coroutine>(app,"aiRoutine");string before=MatchState(app);
            Click(app.matchRetry);Step(app,20);Check(MatchState(app)==before&&ReferenceEquals(search,Get<Coroutine>(app,"aiRoutine"))&&!search.Stopped,"confirm replaced or advanced pending AI");
            Click(app.actionCancel);Until(app,()=>!Get<bool>(app,"busy"),"AI resume after cancel");
            Check(Board(app).MoveCount==4&&Get<List<ConnectFourBoard>>(app,"undoHistory").Count==1&&Progress(app).Data.completedMatches==0,"AI resume duplicated turn/result");
            Check(LiveToasts(app)==1,"AI reply must start one toast");
        });
        Run("v9.confirm_pause_leave_cancel_restores_pause",()=>
        {
            var app=Opening(new[]{3,2});app.SelectColumn(4);app.PlaceSelected();Click(app.pause);string before=MatchState(app);
            Click(app.confirmLeave);Check(app.actionConfirm.activeSelf&&app.confirm.activeSelf,"leave confirmation from pause");Step(app,15);Check(MatchState(app)==before,"AI moved in nested confirmation");
            Click(app.actionCancel);Check(app.confirm.activeSelf&&!app.actionConfirm.activeSelf&&MatchState(app)==before,"cancel must return to pause");Step(app,10);Check(MatchState(app)==before,"AI resumed before pause dismissed");
            Click(app.confirmResume);Until(app,()=>!Get<bool>(app,"busy"),"resume original match");Check(Board(app).MoveCount==4,"resume lost or duplicated AI reply");
        });
        Run("v9.confirm_result_cancel_all_outcomes",()=>
        {
            foreach(int winner in new[]{1,-1,0})foreach(int stage in new[]{1,30})foreach(bool restart in new[]{false,true})
            {
                var app=Terminal(winner,stage);Until(app,()=>app.result.activeSelf&&!Get<bool>(app,"resolving"),"result settled");string before=MatchState(app),speech=app.aiSpeech.text;bool next=app.nextStage.gameObject.activeSelf;
                Click(restart?app.retry:app.resultLobby);Check(app.actionConfirm.activeSelf&&app.result.activeSelf&&MatchState(app)==before,"result request changed terminal state");
                var strings=Get<StringTable>(app,"strings");Check(app.actionQuestion.text==strings.Text(restart?"confirm_retry_title":"confirm_exit_title")&&app.actionAcceptLabel.text==strings.Text(restart?"retry":"confirm_leave"),"wrong confirmation text/action");
                Click(app.actionCancel);Check(MatchState(app)==before&&app.result.activeSelf&&!app.actionConfirm.activeSelf&&app.aiSpeech.text==speech&&app.nextStage.gameObject.activeSelf==next,"result cancel changed state");
                Check(app.retry.interactable&&app.resultLobby.interactable,"result actions not restored");
            }
        });
        Run("v9.confirm_accept_once_all_user_origins",()=>
        {
            foreach(string origin in new[]{"match-retry","pause-leave","result-retry","result-lobby"})
            {
                bool terminal=origin.StartsWith("result");var app=terminal?Terminal():Opening(new[]{3,2});if(terminal)Until(app,()=>app.result.activeSelf&&!Get<bool>(app,"resolving"),"result ready");
                if(!terminal)PlaceAndReply(app,4);string oldId=Get<string>(app,"matchId");int completed=Progress(app).Data.completedMatches;
                if(origin=="pause-leave"){Click(app.pause);Click(app.confirmLeave);}else Click(origin=="match-retry"?app.matchRetry:origin=="result-retry"?app.retry:app.resultLobby);
                Check(Get<string>(app,"matchId")==oldId,"action executed before acceptance");AcceptPanel(app);string newId=Get<string>(app,"matchId"),state=State(Board(app));
                app.actionAccept.onClick.Invoke();app.actionAccept.onClick.Invoke();app.actionCancel.onClick.Invoke();
                Check(Get<string>(app,"matchId")==newId&&State(Board(app))==state&&Progress(app).Data.completedMatches==completed,"duplicate accepted action "+origin);
                if(origin.EndsWith("retry"))Check(app.match.activeSelf&&newId!=oldId&&Get<int>(app,"activeStage")==0&&Get<List<ConnectFourBoard>>(app,"undoHistory").Count==0&&Get<int>(app,"selectedColumn")==-1,"retry did not reset one match");
                else Check(app.lobby.activeSelf&&!app.match.activeSelf&&newId==null&&Get<int>(app,"selected")==Progress(app).Data.unlockedStage-1,"lobby acceptance");
            }
        });
        Run("v9.confirm_stale_events_and_late_ai_after_accept",()=>
        {
            var app=Opening(new[]{3,2});app.SelectColumn(4);app.PlaceSelected();var stale=Get<Coroutine>(app,"aiRoutine");Click(app.matchRetry);AcceptPanel(app);string state=MatchState(app);ResumeStale(stale);Step(app,10);Check(MatchState(app)==state,"accepted Retry polluted by old AI");
            Click(app.matchRetry);Progress(app).Data.unlockedStage=2;app.StartStage(1);state=MatchState(app);app.actionAccept.onClick.Invoke();app.actionCancel.onClick.Invoke();Check(MatchState(app)==state&&!app.actionConfirm.activeSelf,"old confirmation applied to new stage");
            Click(app.matchRetry);Call(app,"OnDestroy");state=MatchState(app);app.actionAccept.onClick.Invoke();app.actionCancel.onClick.Invoke();Check(MatchState(app)==state,"confirmation event after destroy");
        });
        Run("v9.confirm_locale_refresh_and_immediate_next",()=>
        {
            var app=Opening();Click(app.matchRetry);int routines=app.Coroutines.Count;app.SetLanguage("en");Check(app.actionQuestion.text=="Restart this game?"&&app.actionAcceptLabel.text=="Restart"&&app.Coroutines.Count==routines,"confirmation locale refresh");
            app.SetLanguage("ko");Check(app.actionQuestion.text=="다시하시겠습니까?"&&app.actionAcceptLabel.text=="다시하기","Korean confirmation");Click(app.actionCancel);
            foreach(int stage in new[]{1,29}){var result=Terminal(1,stage);Until(result,()=>result.result.activeSelf&&!Get<bool>(result,"resolving"),"Next ready");Click(result.nextStage);string id=Get<string>(result,"matchId");Check(!result.actionConfirm.activeSelf&&Get<int>(result,"activeStage")==stage,"Next must be immediate");result.nextStage.onClick.Invoke();Check(Get<string>(result,"matchId")==id,"duplicate Next restarted new match");}
        });
        Run("v9.toast_once_fade_language_and_hint",()=>
        {
            var app=Opening();Check(app.turnToast.gameObject.activeSelf&&app.turnToast.alpha==1&&LiveToasts(app)==1&&!app.playerAvatar.gameObject.activeSelf&&!app.turnLabel.gameObject.activeSelf,"new turn toast/removed persistent UI");
            int count=app.Coroutines.Count;app.SetLanguage("en");Check(app.turnToastLabel.text=="Your turn"&&app.Coroutines.Count==count&&LiveToasts(app)==1,"language replayed toast");
            Step(app,12);Check(app.turnToast.alpha>0&&app.turnToast.alpha<1,"toast did not fade");Until(app,()=>!app.turnToast.gameObject.activeSelf,"toast expires",10);Check(LiveToasts(app)==0,"toast coroutine remained");
            app.SetLanguage("ko");Check(!app.turnToast.gameObject.activeSelf&&LiveToasts(app)==0,"hidden language refresh replayed toast");
            var ads=new DelayedAds();Set(app,"ads",ads);Call(app,"RequestHint");ads.Rewards[0](AdResult.Completed);Until(app,()=>!Get<bool>(app,"busy"),"hint complete");Check(LiveToasts(app)==0&&!app.turnToast.gameObject.activeSelf,"hint replayed same player turn toast");
        });
        Run("v9.toast_real_ai_undo_and_cancelled_routines",()=>
        {
            var app=Opening(new[]{3,2});var initial=Get<Coroutine>(app,"toastRoutine");app.SelectColumn(4);app.PlaceSelected();Check(initial.Stopped&&!app.turnToast.gameObject.activeSelf,"Place did not cancel toast");
            Until(app,()=>!Get<bool>(app,"busy"),"AI turn toast");Check(LiveToasts(app)==1&&app.turnToast.gameObject.activeSelf,"AI reply toast");var previous=Get<Coroutine>(app,"toastRoutine");app.UndoTurn();Check(previous.Stopped&&LiveToasts(app)==1,"Undo duplicate toast");
            var undoToast=Get<Coroutine>(app,"toastRoutine");app.UndoTurn();Check(ReferenceEquals(undoToast,Get<Coroutine>(app,"toastRoutine")),"rejected Undo replayed toast");
            app.StartStage(0);var current=Get<Coroutine>(app,"toastRoutine");Check(undoToast.Stopped&&current!=undoToast&&LiveToasts(app)==1,"new match stale toast remains");Step(app,2);Check(app.turnToast.gameObject.activeSelf&&ReferenceEquals(current,Get<Coroutine>(app,"toastRoutine")),"cancelled coroutine altered new toast");
            app.ShowLobby();Check(current.Stopped&&LiveToasts(app)==0&&!app.turnToast.gameObject.activeSelf,"lobby stale toast");
            // A cancelled Unity coroutine is not an external SDK callback: never force MoveNext on it to invent native behavior.
        });
        Run("v9.toast_modal_result_pause_destroy_cleanup",()=>
        {
            foreach(string action in new[]{"confirm","pause","application-pause","result","destroy"})
            {
                var app=Opening();var old=Get<Coroutine>(app,"toastRoutine");
                if(action=="confirm")Click(app.matchRetry);else if(action=="pause")Click(app.pause);else if(action=="application-pause")Call(app,"OnApplicationPause",true);else if(action=="destroy")Call(app,"OnDestroy");else{Set(app,"board",Replay(new[]{0,6,1,6,2,5,3}));Call(app,"Finish");}
                Check(old.Stopped&&LiveToasts(app)==0&&!app.turnToast.gameObject.activeSelf&&app.turnToast.alpha==0,"toast leaked into "+action);Step(app,5);Check(!app.turnToast.gameObject.activeSelf,"stopped toast reappeared "+action);
                if(action=="confirm")Click(app.actionCancel);else if(action=="pause")Click(app.confirmResume);else if(action=="application-pause")Call(app,"OnApplicationPause",false);
                Check(LiveToasts(app)==0,"resume/cancel unexpectedly replayed toast "+action);
            }
        });
        Run("v9.qa014_destroy_reward_and_ai_callback_guards",()=>
        {
            foreach(bool reward in new[]{false,true})
            {
                var app=Opening();var ads=new DelayedAds();Set(app,"ads",ads);Coroutine ai=null;
                if(reward)Call(app,"RequestHint");else{app.SelectColumn(3);app.PlaceSelected();ai=Get<Coroutine>(app,"aiRoutine");}
                Call(app,"OnDestroy");string before=MatchState(app),notice=app.matchNotice.text,save=PlayerPrefs.GetString(SaveKey);int cancels=ads.CancelCalls;
                if(reward){ads.Rewards[0](AdResult.Completed);ads.Rewards[0](AdResult.Unavailable);}else ResumeStale(ai);
                Check(MatchState(app)==before&&app.matchNotice.text==notice&&PlayerPrefs.GetString(SaveKey)==save&&!Gate(app).Granted&&!Gate(app).Pending&&Get<string>(app,"matchId")==null,"destroyed work changed state");
                Check(cancels>0&&Get<bool>(app,"destroyed")&&LiveToasts(app)==0,"destroy lifecycle not cancelled");
            }
        });
        Run("v9.reward_old_request_cannot_cancel_new_request",()=>
        {
            var app=Opening();var ads=new DelayedAds();Set(app,"ads",ads);Call(app,"RequestHint");Click(app.matchRetry);Click(app.actionCancel);Call(app,"RequestHint");Check(ads.Rewards.Count==2&&Gate(app).Pending,"new hint request missing");
            string notice=app.matchNotice.text;ads.Rewards[0](AdResult.Unavailable);ads.Rewards[0](AdResult.Completed);Check(Gate(app).Pending&&!Gate(app).Granted&&app.matchNotice.text==notice,"stale reward altered new request");
            ads.Rewards[1](AdResult.Completed);Until(app,()=>!Get<bool>(app,"busy"),"new valid reward");Check(Gate(app).Granted&&!Gate(app).Pending,"new valid hint not granted");
        });
        Run("v9.story_exact_first_lines_and_stage_locales",()=>
        {
            var table=new StringTable(StringsJson);
            Check(table.Text("story_first_intro")=="이 게임의 프로라고 했지? 그런데 내가 이길 것 같아."&&table.Text("story_first_win")=="프로라는 말, 거짓말이었나?"&&table.Text("story_first_lose")=="프로는 역시 다르네. 다음엔 내가 이긴다.","exact Korean first story text");
            table.SetLanguage("en");Check(table.Text("story_first_intro")=="You said you were a pro at this game? I think I can beat you."&&table.Text("story_first_win")=="Was that pro story just a bluff?"&&table.Text("story_first_lose")=="A pro really is different. Next time, I win.","approved English first story text");
            foreach(int id in new[]{1,2,3,4,28,29,30})foreach(string locale in new[]{"ko","en"})
            {
                var app=Make(raw:"{\"saveVersion\":1,\"unlockedStage\":30}");app.SetLanguage(locale);app.StartStage(id-1);Call(app,"CancelSearch");var stage=Get<GameData>(app,"data").Stages[id-1];var strings=Get<StringTable>(app,"strings");
                Check(app.aiSpeech.text==strings.Text(stage.aiIntroKey)&&ReferenceEquals(app.rankHat.sprite,app.theme.rankHats[(id-1)/3]),"intro/hat mapped to wrong stage");
                foreach(int winner in new[]{1,-1})
                {
                    app.StartStage(id-1);Call(app,"CancelSearch");Set(app,"board",Replay(new[]{0,6,1,6,2,5,3},winner));Call(app,"Finish");Until(app,()=>app.result.activeSelf&&!Get<bool>(app,"resolving"),"localized story result");
                    Check(app.aiSpeech.text==strings.Text(winner==1?stage.aiLoseKey:stage.aiWinKey),"story winner meaning inverted "+id+"/"+locale);
                }
            }
        });
        Run("v9.result_ad_save_warning_updates_scroll_state",()=>
        {
            var ads=new DelayedAds();var app=Terminal(1,1,ads);Until(app,()=>ads.Interstitials.Count==1,"save-warning ad");float before=app.matchScroll.content.sizeDelta.y;PlayerPrefs.ThrowOnSave=true;ads.Interstitials[0](AdResult.Completed);Step(app);
            Check(app.retry.interactable&&app.resultAdStatus.gameObject.activeSelf&&app.resultAdStatus.text==Get<StringTable>(app,"strings").Text("client_error_save"),"post-ad save warning not refreshed");
            Check(app.matchScroll.content.sizeDelta.y==before+55,"warning row must extend scroll content");
        });
    }
}
