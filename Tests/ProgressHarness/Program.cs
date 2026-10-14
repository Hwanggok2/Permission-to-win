using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using StageGame;
using UnityEngine;

static class Program
{
    const string Key = "stagegame.progress.v1";
    static readonly List<object> Results = new List<object>();
    static int passed, failed, assertions;
    static void Check(bool condition, string message) { assertions++; if (!condition) throw new Exception(message); }
    static void Run(string name, Action action)
    {
        int before = assertions;
        try { action(); passed++; Results.Add(new { name, status="PASS", assertions=assertions-before, error=(string)null }); Console.WriteLine("PASS " + name); }
        catch (Exception error) { failed++; Results.Add(new { name, status="FAIL", assertions=assertions-before, error=error.Message }); Console.WriteLine("FAIL " + name + " | " + error.Message); }
    }
    static ProgressStore Load(string json)
    {
        PlayerPrefs.Reset();
        if (json != null) PlayerPrefs.SetString(Key, json);
        return new ProgressStore(30);
    }
    static void State(ProgressStore p, int unlocked, int cleared, int matches)
    {
        Check(p.Data.unlockedStage == unlocked, $"unlock expected {unlocked}, actual {p.Data.unlockedStage}");
        Check(p.Data.highestClearedStage == cleared, $"cleared expected {cleared}, actual {p.Data.highestClearedStage}");
        Check(p.Data.completedMatches == matches, $"matches expected {matches}, actual {p.Data.completedMatches}");
    }
    static void Expected(ProgressStore p, JsonElement e)
    {
        State(p, e.GetProperty("unlockedStage").GetInt32(), e.GetProperty("highestActuallyCompletedStage").GetInt32(), e.GetProperty("completedMatches").GetInt32());
        Check(p.Data.saveVersion == 1, "save version changed");
        Check(p.Data.language == e.GetProperty("language").GetString(), "language lost");
        Check(p.Data.soundEnabled == e.GetProperty("soundEnabled").GetBoolean(), "sound lost");
        Check(p.Data.lastInterstitialMatch == e.GetProperty("lastInterstitialMatch").GetInt32(), "ad match count lost");
        Check(p.Data.lastInterstitialUtc == e.GetProperty("lastInterstitialUtc").GetInt64(), "ad time lost");
        Check(p.Data.processedMatchIds.SequenceEqual(e.GetProperty("processedMatchIds").EnumerateArray().Select(x => x.GetString())), "processed IDs lost/reordered");
        Check(p.LoadWarning == null, "valid save incorrectly recovered");
    }
    static int Main(string[] args)
    {
        using var fixture = JsonDocument.Parse(File.ReadAllText(args[0]));
        var cases = fixture.RootElement.GetProperty("cases").EnumerateArray().ToArray();
        Run("progress_ux.legacy_raw_fixtures_and_idempotent_roundtrip", () => {
            Check(cases.Length == 9, "expected fresh install plus 8 legacy inputs");
            foreach (var item in cases)
            {
                var raw = item.GetProperty("rawSaveJson");
                string json = raw.ValueKind == JsonValueKind.Null ? null : raw.GetString();
                var p = Load(json); var expected = item.GetProperty("expected"); Expected(p, expected);
                if (json != null) Check(PlayerPrefs.GetString(Key) == json, "load rewrote original save before explicit save");
                else Check(!PlayerPrefs.HasKey(Key), "fresh load implicitly writes progression");
                Check(p.Save(), "save failed");
                var reloaded = new ProgressStore(30); Expected(reloaded, expected);
                foreach (string old in expected.GetProperty("processedMatchIds").EnumerateArray().Select(x => x.GetString()))
                    Check(!reloaded.RecordResult(old, 1, true, 30), "migration lost old duplicate guard");
                Expected(reloaded, expected);
                Check(reloaded.Save(), "second save failed"); Expected(new ProgressStore(30), expected);
            }
        });
        Run("progress_ux.all_thirty_wins_and_completion_monotonicity", () => {
            var p = Load(null);
            for (int stage=1; stage<=30; stage++)
            {
                Check(p.RecordResult("stage-"+stage, stage, true, 30), "new win rejected");
                State(p, Math.Min(30,stage+1), stage, stage);
                Check(!p.RecordResult("stage-"+stage, stage, true, 30), "duplicate win accepted");
                State(p, Math.Min(30,stage+1), stage, stage);
            }
            p.RecordResult("lower-retry", 1, true, 30); p.RecordResult("last-loss", 30, false, 30);
            State(p,30,30,32); State(new ProgressStore(30),30,30,32);
        });
        Run("progress_ux.unlock30_loss_draw_do_not_claim_all_clear", () => {
            var item=cases.Single(x=>x.GetProperty("id").GetString()=="legacy_unlock_30_is_not_30_complete");
            var p=Load(item.GetProperty("rawSaveJson").GetString()); State(p,30,29,42);
            Check(p.RecordResult("last-loss",30,false,30),"loss rejected"); State(p,30,29,43);
            Check(p.RecordResult("last-draw",30,false,30),"draw-as-not-won rejected"); State(p,30,29,44);
            State(new ProgressStore(30),30,29,44);
            Check(p.RecordResult("last-win",30,true,30),"last win rejected"); State(p,30,30,45);
            var reloaded=new ProgressStore(30); State(reloaded,30,30,45);
            Check(!reloaded.RecordResult("last-win",30,true,30),"reloaded final win duplicated");
            Check(reloaded.Data.language=="en" && !reloaded.Data.soundEnabled && reloaded.Data.lastInterstitialMatch==38 && reloaded.Data.lastInterstitialUtc==123456,"final clear changed preferences/ad history");
        });
        Run("progress_ux.new_retry_is_one_match_not_another_cleared_stage", () => {
            var p=Load(null);
            Check(p.RecordResult("A",1,true,30),"A rejected");
            Check(p.RecordResult("B",1,true,30),"new retry B rejected"); State(p,2,1,2);
            Check(!p.RecordResult("A",1,true,30),"A/B/A duplicated");
            var loaded=new ProgressStore(30); Check(!loaded.RecordResult("B",1,true,30),"B duplicated after reload");
            Check(loaded.RecordResult("C",2,false,30),"nonwinning match rejected"); State(loaded,2,1,3);
            Check(loaded.RecordResult("D",2,true,30),"next win rejected"); State(loaded,3,2,4);
        });
        Run("progress_ux.explicit_final_completion_and_preferences_survive", () => {
            var data=new ProgressData { unlockedStage=30, highestClearedStage=30, completedMatches=101, lastInterstitialMatch=99, lastInterstitialUtc=2147483648L, language="en", soundEnabled=false, processedMatchIds=new List<string>{"done30","retry1"} };
            var p=Load(JsonUtility.ToJson(data)); State(p,30,30,101);
            Check(p.Save(),"explicit completion save failed"); var r=new ProgressStore(30); State(r,30,30,101);
            Check(r.Data.language=="en" && !r.Data.soundEnabled && r.Data.lastInterstitialMatch==99 && r.Data.lastInterstitialUtc==2147483648L,"existing fields changed");
            Check(r.Data.processedMatchIds.SequenceEqual(new[]{"done30","retry1"}),"processed IDs changed");
            Check(!r.RecordResult("done30",30,true,30),"explicit existing ID duplicated");
        });
        Run("progress_ux.invalid_results_do_not_poison_future_match_id", () => {
            var p=Load(null);
            Check(!p.RecordResult(null,1,true,30),"null ID accepted"); Check(!p.RecordResult("",1,true,30),"empty ID accepted");
            Check(!p.RecordResult("later",0,true,30),"stage0 accepted"); Check(!p.RecordResult("later",31,true,30),"stage31 accepted");
            State(p,1,0,0); Check(p.Data.processedMatchIds.Count==0,"rejected ID remembered");
            Check(p.RecordResult("later",1,true,30),"invalid earlier request poisoned valid ID"); State(p,2,1,1);
        });
        Run("progress_ux.corrupt_completion_and_existing_recovery_preserve_raw", () => {
            foreach (string raw in new[]{"{broken", "{\"saveVersion\":1,\"unlockedStage\":30,\"highestClearedStage\":-1}", "{\"saveVersion\":1,\"unlockedStage\":30,\"highestClearedStage\":31}", "{\"saveVersion\":2,\"unlockedStage\":30,\"highestClearedStage\":30}"})
            {
                var p=Load(raw); State(p,1,0,0); Check(p.LoadWarning=="error_load","missing recovery warning");
                Check(PlayerPrefs.GetString(Key+".recovery")==raw && PlayerPrefs.GetString(Key)==raw,"corrupt original overwritten");
            }
        });
        Run("progress_ux.save_exception_preserves_memory_and_retry_warning", () => {
            var p=Load(null); PlayerPrefs.ThrowOnSave=true;
            Check(p.RecordResult("A",1,true,30),"save failure escaped result path"); State(p,2,1,1);
            Check(p.SaveWarning=="client_error_save","save failure warning missing");
            Check(!p.RecordResult("A",1,true,30),"failed save duplicated current result");
            PlayerPrefs.ThrowOnSave=false; Check(p.Save() && p.SaveWarning==null,"save retry did not recover warning");
            State(new ProgressStore(30),2,1,1);
        });
        File.WriteAllText(args[1],JsonSerializer.Serialize(new { passed,failed,assertions,results=Results,
            limits="Actual ProgressStore and StringTable source snapshots. System.Text.Json and memory PlayerPrefs are explicit doubles, not native Unity JSON/durable storage. Save double writes memory before its injected Save exception. Loss/draw both use RecordResult won=false; no GameApp outcome/CTA, UI, native events, browser, ads SDK or device execution." },new JsonSerializerOptions{WriteIndented=true}));
        return failed==0?0:1;
    }
}
