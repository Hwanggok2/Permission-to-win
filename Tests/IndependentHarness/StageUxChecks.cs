using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using StageGame;
using UnityEngine;
using UnityEngine.UI;

static partial class Program
{
    static Dictionary<string,JsonElement> ApprovedCopyRows;
    static string ExpectedApprovedCopy(string key, string language, string previous)
    {
        if(!ApprovedCopyRows.TryGetValue(key,out var change)) return previous;
        Check(previous==change.GetProperty("before").GetProperty(language).GetString(),"approved copy baseline differs: "+key);
        string after=change.GetProperty("after").GetProperty(language).GetString();
        Check(after==previous.Split('\n')[0],"approved change must remove only the gold-line second sentence");
        return after;
    }
    static bool ClickEnabled(Button button)
    {
        // Direct listener dispatch with local flags; not Unity raycast or parent hierarchy proof.
        if (!button.interactable || !button.gameObject.activeSelf) return false;
        button.onClick.Invoke(); return true;
    }
    static bool Mint(Color color) => Math.Abs(color.r-108/255f)<0.00001 && Math.Abs(color.g-237/255f)<0.00001 && Math.Abs(color.b-199/255f)<0.00001;
    static bool Position(RectTransform rect, RectTransform expected) => rect.anchoredPosition.x==expected.anchoredPosition.x && rect.anchoredPosition.y==expected.anchoredPosition.y;
    static void FinishFixture(GameApp app, int outcome)
    {
        var terminal = Replay(outcome==0 ? new[]{0,6,1,6,2,5,3} : outcome==1 ? new[]{0,6,1,6,2,6,4,6} : Draw);
        Check(terminal.IsFinished && (outcome==2 ? terminal.IsDraw : terminal.Winner==(outcome==0?1:-1)), "terminal fixture outcome");
        Set(app,"board",terminal); Call(app,"RefreshBoard"); Call(app,"Finish"); AdvanceToResult(app);
    }
    static void RunStageUx(string[] args)
    {
        string evidence=Path.GetDirectoryName(args[0]);
        Run("stageux.catalog_adds_exact14_with_only_three_approved_copy_changes", "independent JSON expectations; actual StringTable formats", () => {
            using var prior=JsonDocument.Parse(File.ReadAllText(Path.Combine(evidence,"AuditInputs","previous-123-strings.json")));
            using var current=JsonDocument.Parse(ActualStringsJson);
            var old=prior.RootElement.GetProperty("strings").EnumerateArray().ToDictionary(r=>r.GetProperty("key").GetString());
            var rows=current.RootElement.GetProperty("strings").EnumerateArray().ToDictionary(r=>r.GetProperty("key").GetString());
            string[] added={"help_connect_caption","help_connect_title","help_drop_caption","help_drop_title","stage_cleared_progress","stage_lock_requirement","stage_map_chapter","stage_replay","stage_state_cleared","stage_state_current","stage_state_locked","stage_tier_1","stage_tier_2","stage_tier_3"};
            Check(old.Count==123 && rows.Count==137,"catalog sizes");
            Check(ApprovedCopyRows.Keys.OrderBy(x=>x).SequenceEqual(new[]{"result_loss_diagonal","result_loss_horizontal","result_loss_vertical"}),"unexpected copy exception keys");
            Check(rows.Keys.Except(old.Keys).OrderBy(x=>x).SequenceEqual(added.OrderBy(x=>x)),"added keys differ from 14-key requirement");
            foreach(var pair in old) foreach(string language in new[]{"ko","en"})
                Check(rows.ContainsKey(pair.Key) && rows[pair.Key].GetProperty(language).GetString()==ExpectedApprovedCopy(pair.Key,language,pair.Value.GetProperty(language).GetString()),"unapproved previous translation change: "+pair.Key);
            var table=new StringTable(ActualStringsJson);
            foreach(string language in new[]{"ko","en"})
            {
                table.SetLanguage(language);
                foreach(string key in added)
                {
                    int count=key=="stage_cleared_progress" || key=="stage_map_chapter" ? 2 : key=="stage_lock_requirement" ? 1 : 0;
                    object[] values=count==2 ? new object[]{29,30} : count==1 ? new object[]{29} : Array.Empty<object>();
                    Check(table.Format(key,values)==string.Format(CultureInfo.InvariantCulture,rows[key].GetProperty(language).GetString(),values),"stage UX format mismatch: "+key);
                }
            }
        });
        Run("stageux.initial_and_legacy_lobby_restore_actual_completion", "actual GameApp startup with memory save/UI doubles", () => {
            var fresh=MakeApp(true,false);
            Check(fresh.lobby.activeSelf && !fresh.match.activeSelf && (int)Get(fresh,"selected")==0,"fresh lobby selection");
            Check(fresh.stageNodeNumbers[0].text=="01" && ((Image)fresh.stageNodes[0].targetGraphic).sprite==fresh.theme.nodeSelected && fresh.play.interactable,"fresh start unavailable");
            Check(((ProgressStore)Get(fresh,"progress")).Data.highestClearedStage==0,"fresh clear claimed");
            foreach(int unlocked in new[]{13,30})
            {
                PlayerPrefs.Reset(); PlayerPrefs.SetString(ProgressKey,"{\"saveVersion\":1,\"unlockedStage\":"+unlocked+",\"completedMatches\":42,\"lastInterstitialMatch\":38,\"lastInterstitialUtc\":123456,\"soundEnabled\":false,\"language\":\"en\"}");
                var app=MakeApp(false,false); var p=(ProgressStore)Get(app,"progress"); var t=(StringTable)Get(app,"strings");
                Check((int)Get(app,"selected")==unlocked-1 && !app.match.activeSelf,"restore selected wrong stage or started a match");
                Check(app.stageNodeNumbers[(unlocked-1)%10].text==unlocked.ToString("00") && ((Image)app.stageNodes[(unlocked-1)%10].targetGraphic).sprite==app.theme.nodeSelected,"restored chapter slot");
                Check(app.stageNodeStates[(unlocked-1)%10].text==t.Text("stage_state_current"),"unlocked stage incorrectly cleared");
                Check(app.progressLabel.text==t.Format("stage_cleared_progress",unlocked-1,30),"progress label overclaims completion");
                app.SetLanguage("ko"); app.SetLanguage("en");
                var restart=MakeApp(false,false); var restored=(ProgressStore)Get(restart,"progress");
                Check(restored.Data.highestClearedStage==unlocked-1 && restored.Data.completedMatches==42 && restored.Data.lastInterstitialMatch==38 && restored.Data.lastInterstitialUtc==123456 && !restored.Data.soundEnabled && restored.Data.language=="en","UI/language changed saved history");
            }
        });
        Run("stageux.chapter_states_all30_ko_en_and_locked_start_guard", "actual GameApp node listeners; local UI flags", () => {
            foreach(string language in new[]{"ko","en"})
            {
                var app=MakeApp(true,false); var p=(ProgressStore)Get(app,"progress"); p.Data.unlockedStage=13; p.Data.highestClearedStage=12;
                app.SetLanguage(language); var t=(StringTable)Get(app,"strings");
                for(int chapter=0;chapter<3;chapter++)
                {
                    Check(app.chapterLabel.text==t.Format("stage_map_chapter",chapter+1,3) && app.chapterTier.text==t.Text("stage_tier_"+(chapter+1)),"chapter/tier text wrong");
                    Check(app.previous.interactable==(chapter>0) && app.following.interactable==(chapter<2),"chapter boundary buttons wrong");
                    for(int slot=0;slot<10;slot++)
                    {
                        int id=chapter*10+slot+1; Check(ClickEnabled(app.stageNodes[slot]),"node not inspectable");
                        Check((int)Get(app,"selected")==id-1 && app.stageNodeNumbers[slot].text==id.ToString("00") && ((Image)app.stageNodes[slot].targetGraphic).sprite==app.theme.nodeSelected,"wrong node selection");
                        for(int row=0;row<10;row++)
                        {
                            int nodeId=chapter*10+row+1;
                            string state=nodeId<=12 ? "✓ "+t.Text("stage_state_cleared") : t.Text(nodeId==13 ? "stage_state_current":"stage_state_locked");
                            Check(app.stageNodeStates[row].text==state,"non-color state missing/wrong");
                            string number=nodeId.ToString("00"); Check(app.stageNodeNumbers[row].text==number,"node number changed");
                            Check(((Image)app.stageNodes[row].targetGraphic).sprite==(row==slot?app.theme.nodeSelected:nodeId>13?app.theme.nodeLocked:nodeId<=12?app.theme.nodeCleared:app.theme.nodeCurrent),"selected/state sprite wrong");
                            Check(app.stageNodeIcons[row].sprite==(nodeId>13?app.theme.locked:nodeId<=12?app.theme.check:app.theme.play),"node glyph wrong");
                        }
                        Check(app.play.interactable==(id<=13),"locked start button state");
                        if(id>13)
                        {
                            Check(app.stageDetail.text==t.Format("stage_lock_requirement",id-1),"locked prerequisite wrong");
                            // Invoke even the disabled listener to independently verify the StartStage guard.
                            app.play.onClick.Invoke(); Check(app.lobby.activeSelf && !app.match.activeSelf && Get(app,"matchId")==null,"locked listener started match");
                        }
                    }
                    if(chapter<2) Check(ClickEnabled(app.following),"next chapter unavailable");
                }
                Check(!ClickEnabled(app.following),"chapter4 reachable");
                Check(ClickEnabled(app.previous) && (int)Get(app,"selected")==10,"back to chapter2 failed");
                Check(ClickEnabled(app.previous) && (int)Get(app,"selected")==0 && !ClickEnabled(app.previous),"chapter1 boundary failed");
                Check(p.Data.completedMatches==0 && p.Data.highestClearedStage==12 && p.Data.unlockedStage==13,"browsing changed progress");
            }
        });
        Run("stageux.each_unlocked_node_starts_exactly_its_mapped_stage", "actual GameApp listeners with synthetic legal starts", () => {
            var app=MakeApp(true,false); var p=(ProgressStore)Get(app,"progress"); p.Data.unlockedStage=30; p.Data.highestClearedStage=29;
            var ads=new DelayedAds(); Set(app,"ads",ads); Call(app,"RefreshLobby");
            var ids=new System.Collections.Generic.HashSet<string>();
            for(int chapter=0;chapter<3;chapter++)
            {
                for(int slot=0;slot<10;slot++)
                {
                    Set(app,"selected",chapter*10); Call(app,"RefreshLobby");
                    int stage=chapter*10+slot; ClickEnabled(app.stageNodes[slot]); int before=ads.CancelCalls;
                    Check(ClickEnabled(app.play),"unlocked stage start blocked");
                    Check((int)Get(app,"activeStage")==stage && app.match.activeSelf && !app.lobby.activeSelf,"wrong stage started");
                    Check(ids.Add((string)Get(app,"matchId")) && ads.CancelCalls==before+1,"duplicate start listener or reused match ID");
                    Check(((ConnectFourBoard)Get(app,"board")).MoveCount==0,"synthetic empty stage not loaded");
                    app.ShowLobby();
                }
                if(chapter<2) ClickEnabled(app.following);
            }
            Check(ids.Count==30 && p.Data.completedMatches==0 && p.Data.highestClearedStage==29,"starting/leaving falsely completed stages");
        });
        Run("stageux.win_loss_draw_two_actions_no_fill_and_language_switch", "actual result state and listener actions; no pixel proof", () => {
            foreach(string language in new[]{"ko","en"}) for(int outcome=0;outcome<3;outcome++)
            {
                var app=MakeApp(true,false); var p=(ProgressStore)Get(app,"progress"); p.Data.unlockedStage=10; p.Data.highestClearedStage=9;
                app.SetLanguage(language); app.StartStage(9); string old=(string)Get(app,"matchId"); FinishFixture(app,outcome);
                Check(app.result.activeSelf && !app.nextStage.gameObject.activeSelf && !app.resultClose.gameObject.activeSelf,"removed actions visible");
                var primary=app.retry;
                Check(((Image)primary.targetGraphic).sprite==app.theme.primary && ((Image)app.resultLobby.targetGraphic).sprite==app.theme.secondary,"result button artwork binding wrong");
                Check(p.Data.completedMatches==1 && p.Data.highestClearedStage==(outcome==0?10:9),"result counting/completion wrong");
                Call(app,"Finish"); Check(p.Data.completedMatches==1,"duplicate Finish counted");
                app.SetLanguage(language=="ko"?"en":"ko");
                Check(p.Data.completedMatches==1 && ((Image)primary.targetGraphic).sprite==app.theme.primary && !app.nextStage.gameObject.activeSelf,"language switch changed result/priority");
                var ads=new DelayedAds(); Set(app,"ads",ads); Check(ClickEnabled(primary),"NoFill blocks primary CTA");
                Check(ads.CancelCalls==1 && (int)Get(app,"activeStage")==9 && (string)Get(app,"matchId")!=old,"retry wrong stage or duplicate transition");
                Check(!app.result.activeSelf && app.match.activeSelf && p.Data.completedMatches==1,"CTA left old result or added a result");
            }
        });
        Run("stageux.final30_is_retry_and_reloads_as_cleared_map", "actual GameApp final clear and saved restart", () => {
            var app=MakeApp(true,false); var p=(ProgressStore)Get(app,"progress"); p.Data.unlockedStage=30; p.Data.highestClearedStage=29;
            app.SetLanguage("en"); app.StartStage(29); FinishFixture(app,0);
            Check(p.Data.highestClearedStage==30 && p.Data.unlockedStage==30 && !app.nextStage.gameObject.activeSelf && ((Image)app.retry.targetGraphic).sprite==app.theme.primary,"final stage not distinguished");
            string before=(string)Get(app,"matchId"); Check(ClickEnabled(app.retry) && (int)Get(app,"activeStage")==29 && before!=(string)Get(app,"matchId"),"final retry entered31 or reused match");
            app.ShowLobby(); var t=(StringTable)Get(app,"strings");
            Check(app.stageNodeNumbers[9].text=="30" && ((Image)app.stageNodes[9].targetGraphic).sprite==app.theme.nodeSelected && app.stageNodeStates.All(s=>s.text=="✓ "+t.Text("stage_state_cleared")) && app.playLabel.text==t.Text("stage_replay"),"cleared map/replay label wrong");
            var restart=MakeApp(false,false); var restored=(ProgressStore)Get(restart,"progress");
            Check(restored.Data.highestClearedStage==30 && restored.Data.completedMatches==1 && (int)Get(restart,"selected")==29,"final completion restore wrong");
        });
        Run("stageux.primary_actions_respect_ad_boundary_and_stale_callbacks", "actual GameApp with delayed ad/coroutine doubles", () => {
            for(int outcome=0;outcome<3;outcome++) foreach(var resolution in new[]{AdResult.Completed,AdResult.Unavailable,AdResult.Failed})
            {
                var app=MakeApp(); var p=(ProgressStore)Get(app,"progress"); p.Data.completedMatches=3;
                var ads=new DelayedAds(); Set(app,"ads",ads); string id=(string)Get(app,"matchId"); FinishFixture(app,outcome);
                Check(ads.Interstitials.Count==1,"eligible result did not request one ad");
                Check(new[]{app.retry,app.nextStage,app.resultLobby,app.resultClose}.All(b=>!b.interactable),"actions enabled during ad boundary");
                var primary=app.retry; Check(!ClickEnabled(primary) && (string)Get(app,"matchId")==id,"pending CTA started another match");
                ads.Interstitials[0](resolution); app.AdvanceCoroutines();
                Check(app.retry.interactable && app.resultLobby.interactable && !app.nextStage.interactable && !app.resultClose.interactable,"terminal ad actions wrong");
                int last=p.Data.lastInterstitialMatch; Check(last==(resolution==AdResult.Completed?4:0),"ad history changed on non-completion");
                Check(ClickEnabled(primary),"resolved CTA blocked"); string next=(string)Get(app,"matchId");
                ads.Interstitials[0](AdResult.Completed); app.AdvanceCoroutines();
                Check((string)Get(app,"matchId")==next && !app.result.activeSelf && p.Data.completedMatches==4 && p.Data.lastInterstitialMatch==last,"stale result ad affected next match/history");
            }
        });
        Run("stageux.save_warning_replaces_clear_count_without_losing_state", "actual lobby state with memory save failure", () => {
            var app=MakeApp(true,false); var p=(ProgressStore)Get(app,"progress"); p.Data.unlockedStage=2; p.Data.highestClearedStage=1;
            PlayerPrefs.ThrowOnSave=true; Check(!p.Save(),"failure injection inactive"); Call(app,"RefreshLobby");
            var t=(StringTable)Get(app,"strings");
            Check(!app.progressLabel.gameObject.activeSelf && app.lobbyNotice.text==t.Text("client_error_save"),"save warning overlaps/exposes clear count instead of replacing it");
            Check(app.stageNodeStates[0].text=="✓ "+t.Text("stage_state_cleared") && app.play.interactable,"warning lost node progression");
            PlayerPrefs.ThrowOnSave=false; Check(p.Save(),"save recovery failed"); Call(app,"RefreshLobby");
            Check(app.progressLabel.gameObject.activeSelf && app.lobbyNotice.text=="" && p.Data.highestClearedStage==1,"warning recovery state wrong");
        });
    }
}
