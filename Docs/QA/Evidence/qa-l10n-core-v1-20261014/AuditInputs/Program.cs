using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using StageGame;
using UnityEngine;
using UnityEngine.UI;

static partial class Program
{
    static readonly List<object> Results = new List<object>();
    static int assertions, failed, passed;
    static int ruleSequences, aiPositions;
    static string Filter = "";
    static string ActualStringsJson;
    static readonly int[] Draw = { 0,2,2,2,2,2,2,0,0,0,0,0,1,3,3,3,3,3,3,1,1,1,1,1,4,6,6,6,6,6,6,4,4,4,4,4,5,5,5,5,5,5 };
    static void Check(bool condition, string message)
    {
        assertions++;
        if (!condition) throw new Exception(message);
    }
    static void Run(string name, string layer, Action test)
    {
        if(Filter.Length>0 && !name.StartsWith(Filter,StringComparison.Ordinal)) return;
        int before = assertions;
        var clock = Stopwatch.StartNew();
        string error = null;
        try { test(); passed++; }
        catch (Exception e)
        {
            while (e is TargetInvocationException && e.InnerException != null) e = e.InnerException;
            error = e.GetType().Name + ": " + e.Message;
            failed++;
        }
        Results.Add(new { name, layer, status = error == null ? "PASS" : "FAIL", assertions = assertions - before, milliseconds = clock.ElapsedMilliseconds, error });
        Console.WriteLine((error == null ? "PASS " : "FAIL ") + name + (error == null ? "" : " | " + error));
    }
    static void Reject(Action action, string label)
    {
        try { action(); }
        catch (FormatException) { assertions++; return; }
        throw new Exception(label + " was accepted");
    }

    // Whole-board four-cell windows, independent of the production last-move ray scan.
    sealed class Oracle
    {
        public readonly int[,] Cells = new int[7, 6];
        public int Next, Count, Winner, Last = -1;
        public readonly HashSet<int> Winning = new HashSet<int>();
        public Oracle(int starter) { Next = starter; }
        public bool Finished => Winner != 0 || Count == 42;
        public int Landing(int col)
        {
            if (Finished || col < 0 || col > 6) return -1;
            for (int row = 0; row < 6; row++) if (Cells[col, row] == 0) return row;
            return -1;
        }
        public bool Drop(int col)
        {
            int row = Landing(col);
            if (row < 0) return false;
            Cells[col, row] = Next; Count++; Last = row * 7 + col; Next = -Next;
            foreach (var direction in new[] { (1, 0), (0, 1), (1, 1), (-1, 1) })
                for (int x = 0; x < 7; x++)
                    for (int y = 0; y < 6; y++)
                    {
                        int endX = x + direction.Item1 * 3, endY = y + direction.Item2 * 3;
                        if (endX < 0 || endX > 6 || endY < 0 || endY > 5 || Cells[x,y] == 0) continue;
                        int side = Cells[x,y];
                        if (Enumerable.Range(0,4).All(n => Cells[x+n*direction.Item1,y+n*direction.Item2] == side))
                        {
                            Winner = side;
                            for (int n = 0; n < 4; n++) Winning.Add((y+n*direction.Item2)*7+x+n*direction.Item1);
                        }
                    }
            return true;
        }
    }
    static void Compare(ConnectFourBoard actual, Oracle expected, string label)
    {
        Check(actual.Winner == expected.Winner && actual.CurrentPlayer == expected.Next && actual.MoveCount == expected.Count &&
            actual.IsFinished == expected.Finished && actual.IsDraw == (expected.Count == 42 && expected.Winner == 0) && actual.LastMove == expected.Last,
            label + " state mismatch");
        Check(new HashSet<int>(actual.WinningCells).SetEquals(expected.Winning), label + " winning cells");
        for (int x = 0; x < 7; x++)
            for (int y = 0; y < 6; y++) Check(actual[x,y] == expected.Cells[x,y], label + " cell " + x + "," + y);
        for (int x = -1; x <= 7; x++)
            Check(actual.LandingRow(x) == expected.Landing(x) && actual.CanDrop(x) == (expected.Landing(x) >= 0), label + " legal column " + x);
    }
    static ConnectFourBoard Replay(IEnumerable<int> moves, int starter = 1)
    {
        var board = new ConnectFourBoard(starter);
        var oracle = new Oracle(starter);
        foreach (int col in moves)
        {
            Check(board.TryDrop(col) == oracle.Drop(col), "replay acceptance col=" + col);
            Compare(board, oracle, "replay");
        }
        return board;
    }
    static string BoardState(ConnectFourBoard b) => b.CurrentPlayer + ":" + b.MoveCount + ":" + b.Winner + ":" +
        string.Join(",", Enumerable.Range(0,42).Select(i => b[i%7,i/7])) + ":" + string.Join(",", b.WinningCells);
    static int Search(ConnectFourBoard b, int depth, int budget, int mistakes, int seed)
    {
        string state = BoardState(b);
        using var search = new AiSearch(b, depth, budget, mistakes, seed);
        int ticks = 0;
        while (!search.Complete && ticks++ < budget + 100)
        {
            int before = search.NodesVisited;
            search.Step(1);
            Check(search.NodesVisited - before <= 1, "Step(1) exceeded reported node quota");
        }
        Check(search.Complete, "AI never completed");
        Check(search.NodesVisited <= budget, "AI exceeded reported node budget");
        Check(BoardState(b) == state, "AI changed input board");
        Check(b.IsFinished ? search.ChosenColumn == -1 : b.CanDrop(search.ChosenColumn), "AI chose illegal column " + search.ChosenColumn);
        return search.ChosenColumn;
    }
    static AdSettings Settings() => new AdSettings { schemaVersion=1, interstitialGraceMatches=3, interstitialEveryMatches=2, interstitialCooldownSeconds=90, rewardedHintsPerMatch=1, bannerLobby=true, bannerResult=true };
    static string[] DataJson(int count = 30)
    {
        var stages = Enumerable.Range(1,count).Select(i => new StageDefinition { id=i, chapter=(i-1)/10+1, titleKey="stage_"+i, aiDepth=1, aiNodeBudget=100, aiRandomPermille=0, starter=1, openingMoves="" }).ToArray();
        var strings = stages.Select(s => new StringEntry { key=s.titleKey, ko="스테이지 " + s.id, en="Stage " + s.id })
            .Concat(JsonUtility.FromJson<StringFile>(ActualStringsJson).strings).ToArray();
        return new[] { JsonUtility.ToJson(new StageFile { schemaVersion=1, stages=stages }), JsonUtility.ToJson(Settings()), JsonUtility.ToJson(new StringFile { schemaVersion=1, strings=strings }) };
    }
    static GameData LoadData(string[] json) => new GameData(json[0],json[1],json[2]);
    static void InvalidField(Action<StageDefinition> mutate)
    {
        var json = DataJson();
        var file = JsonUtility.FromJson<StageFile>(json[0]);
        mutate(file.stages[0]); json[0] = JsonUtility.ToJson(file);
        Reject(() => LoadData(json), "mutated stage");
    }
    static object Get(object instance, string name) => instance.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).GetValue(instance);
    static void Set(object instance, string name, object value) => instance.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).SetValue(instance, value);
    static object Call(object instance, string name, params object[] args) => instance.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(instance,args);
    sealed class DelayedAds : IAdService
    {
        public string Status => "ad_test_mode";
        public readonly List<Action<AdResult>> Rewards = new List<Action<AdResult>>();
        public readonly List<Action<AdResult>> Interstitials = new List<Action<AdResult>>();
        public void ShowRewarded(Action<AdResult> callback) { Rewards.Add(callback); }
        public void ShowInterstitial(Action<AdResult> callback) { Interstitials.Add(callback); }
        // Keep delegates deliberately, so tests can inject callbacks from a misbehaving cancelled SDK.
        public void CancelPending() { }
    }
    static GameApp MakeApp(bool reset = true)
    {
        if (reset) PlayerPrefs.Reset(); Time.realtimeSinceStartup = 0;
        var json = DataJson();
        for (int i=0;i<3;i++) Resources.Assets["GameData/" + new[] { "stages","ads","strings" }[i]] = new TextAsset { text=json[i] };
        var app = new GameApp();
        foreach (var field in typeof(GameApp).GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            if (field.FieldType == typeof(GameObject) || field.FieldType == typeof(Text) || field.FieldType == typeof(Button))
                field.SetValue(app, Activator.CreateInstance(field.FieldType));
        }
        app.stones = Enumerable.Range(0,42).Select(_ => new Image()).ToArray();
        app.stoneLabels = Enumerable.Range(0,42).Select(_ => new Text()).ToArray();
        app.columnLabels = Enumerable.Range(0,7).Select(_ => new Text()).ToArray();
        app.localizedLabels = Array.Empty<LocalizedLabel>();
        app.columnMarkers = Enumerable.Range(0,7).Select(_ => new Image()).ToArray();
        app.columns = Enumerable.Range(0,7).Select(_ => new Button()).ToArray();
        Call(app,"Start"); app.StartStage(0);
        return app;
    }

    static int Main(string[] args)
    {
        if(args.Length>=3 && args[0]=="--prove-fixed") { ProveFixed(args[1],args[2],args.Length>3?args[3]:"10,12,20,22,24,26"); return 0; }
        Filter=args.Length>3?args[3]:"";
        if (Filter == "all") Filter = "";
        ActualStringsJson=File.ReadAllText(Path.Combine(args[1],"strings.json"));
        Run("rules.fixed_vectors_both_starters", "actual pure C#", () =>
        {
            int[][] wins = { new[]{0,6,1,6,2,5,3}, new[]{0,1,0,1,0,2,0}, new[]{0,1,1,2,3,2,2,3,4,3,3}, new[]{6,5,5,4,3,4,4,3,2,3,3} };
            foreach (int starter in new[]{1,-1}) foreach (var moves in wins)
            {
                var b=Replay(moves,starter); Check(b.Winner==starter,"fixed winner");
                string old=BoardState(b); for(int c=-1;c<=7;c++) Check(!b.TryDrop(c),"terminal input accepted");
                Check(old==BoardState(b),"terminal mutation");
            }
            var draw=Replay(Draw); Check(draw.IsDraw,"draw vector");
            var full=Replay(new[]{0,0,0,0,0,0}); string state=BoardState(full);
            Check(!full.TryDrop(0) && !full.TryDrop(-1) && !full.TryDrop(7) && state==BoardState(full),"invalid mutation");
        });
        Run("rules.1000_seeded_sequences", "actual pure C# vs whole-board oracle", () =>
        {
            var random=new Random(20261014);
            for(int sample=0;sample<1000;sample++)
            {
                int starter=sample%2==0?1:-1;
                var b=new ConnectFourBoard(starter);var o=new Oracle(starter);
                while(!o.Finished)
                {
                    var legal=Enumerable.Range(0,7).Where(c=>o.Landing(c)>=0).ToArray();
                    int c=legal[random.Next(legal.Length)];
                    Check(b.TryDrop(c)==o.Drop(c),"sample "+sample+" acceptance"); Compare(b,o,"sample "+sample);
                    var clone=b.Clone(); Compare(clone,o,"clone "+sample);
                    string state=BoardState(b); clone.TryDrop(legal[0]); Check(BoardState(b)==state,"clone shares state");
                }
                string terminal=BoardState(b);for(int c=0;c<7;c++) Check(!b.TryDrop(c),"finished move");
                Check(terminal==BoardState(b),"finished board changed");
                ruleSequences++;
            }
        });
        Run("ai.immediate_win_and_block_all_depths", "actual pure C#", () =>
        {
            foreach(int depth in Enumerable.Range(1,6)) foreach(int mistakes in new[]{0,1000}) foreach(int seed in new[]{1,2,3,4,5})
            {
                Check(Search(Replay(new[]{0,6,1,6,2}),depth,100,mistakes,seed)==3,"failed immediate block");
                Check(Search(Replay(new[]{0,6,1,6,2,6,4}),depth,100,mistakes,seed)==6,"failed winning priority");
            }
        });
        Run("ai.1000_legal_positions_budget_and_immutability", "actual pure C#", () =>
        {
            var random=new Random(7319);
            for(int sample=0;sample<1000;sample++)
            {
                var b=new ConnectFourBoard(sample%2==0?1:-1);
                int length=random.Next(35);
                for(int n=0;n<length && !b.IsFinished;n++)
                {
                    var legal=Enumerable.Range(0,7).Where(b.CanDrop).ToArray(); b.TryDrop(legal[random.Next(legal.Length)]);
                }
                int depth=sample%6+1,budget=new[]{1,100,1000}[sample%3],mistakes=sample%4==0?1000:0;
                int chosen=Search(b,depth,budget,mistakes,sample);
                if(sample<50) Check(Search(b,depth,budget,mistakes,sample)==chosen,"seed not reproducible");
                aiPositions++;
            }
        });
        Run("ai.last_legal_move_and_dispose", "actual pure C#", () =>
        {
            Check(Search(Replay(Draw.Take(41)),6,100,1000,1)==5,"only legal move ignored");
            var b=new ConnectFourBoard(); using var a=new AiSearch(b,6,30000,0,1);a.Step(1);a.Dispose();int nodes=a.NodesVisited;a.Step();
            Check(a.Complete && nodes==a.NodesVisited,"dispose continues search");
        });
        Run("data.valid_30_and_opening_boundaries", "actual validation; System.Text.Json double", () =>
        {
            Check(LoadData(DataJson()).Stages.Length==30,"valid30");
            foreach(string moves in new[]{"","0","0,0,0,0,0,0"}) foreach(int starter in new[]{1,-1})
                Check(!GameData.CreateBoard(new StageDefinition{id=1,starter=starter,openingMoves=moves}).IsFinished,"valid opening");
            foreach(string moves in new[]{"-1","7","x","0,,1","0,0,0,0,0,0,0","0,6,1,6,2,5,3",string.Join(",",Draw)})
                Reject(()=>GameData.CreateBoard(new StageDefinition{id=1,starter=1,openingMoves=moves}),"opening "+moves);
            foreach(Action<StageDefinition> mutation in new Action<StageDefinition>[] {s=>s.id=2,s=>s.chapter=4,s=>s.aiDepth=0,s=>s.aiDepth=7,s=>s.aiNodeBudget=99,s=>s.aiNodeBudget=30001,s=>s.aiRandomPermille=-1,s=>s.aiRandomPermille=1001,s=>s.starter=0,s=>s.titleKey="missing",s=>s.openingMoves=null}) InvalidField(mutation);
        });
        Run("data.reject_truncated_stage_catalog", "actual validation; System.Text.Json double", () => Reject(()=>LoadData(DataJson(1)),"1-stage catalog under 30-stage contract"));
        Run("ads.reward_duplicate_cancel_failure_retry", "actual pure C#", () =>
        {
            foreach(var result in new[]{AdResult.Cancelled,AdResult.Failed,AdResult.Unavailable})
            {
                var gate=new RewardGate();Check(gate.TryBegin(out int t),"begin");Check(!gate.TryBegin(out _),"parallel begin");
                Check(!gate.Complete(t,result) && !gate.Granted && !gate.Pending,"failure grants");
                Check(!gate.Complete(t,AdResult.Completed),"late success after terminal failure");
                Check(gate.TryBegin(out int next),"retry blocked");Check(!gate.Complete(t,AdResult.Completed),"old callback affects new request");
                Check(gate.Pending,"old callback clears pending");Check(gate.Complete(next,AdResult.Completed),"retry reward");
                Check(!gate.Complete(next,AdResult.Completed) && !gate.TryBegin(out _),"duplicate reward");
            }
            var cancelled=new RewardGate();cancelled.TryBegin(out int token);cancelled.Cancel();
            Check(!cancelled.Complete(token,AdResult.Completed) && !cancelled.Granted,"cancelled late grant");
        });
        Run("ads.grace_interval_cooldown_boundaries", "actual pure C#; first clock origin unconfirmed", () =>
        {
            var s=Settings();var p=new ProgressData();
            for(int m=0;m<=3;m++){p.completedMatches=m;Check(!AdPolicy.Eligible(s,p,1000),"grace "+m);}
            p.completedMatches=4;Check(AdPolicy.Eligible(s,p,1000),"first opportunity after grace implementation");
            p.lastInterstitialMatch=4;p.lastInterstitialUtc=1000;
            p.completedMatches=5;Check(!AdPolicy.Eligible(s,p,2000),"one match interval");
            p.completedMatches=6;Check(!AdPolicy.Eligible(s,p,1089),"89sec");Check(AdPolicy.Eligible(s,p,1090),"90sec");
            Check(!AdPolicy.Eligible(s,p,999),"clock rollback");
            var ads=new NoFillAds();int calls=0;ads.ShowRewarded(r=>{calls++;Check(r==AdResult.Unavailable,"default rewards");});Check(calls==1,"no-fill callback count");
            Check(typeof(RewardGate).Assembly.GetType("StageGame.EditorTestAds")==null,"editor adapter present in non-editor build");
        });
        Run("progress.same_result_and_unlock_bounds", "actual source; memory PlayerPrefs/JSON double", () =>
        {
            PlayerPrefs.Reset();var p=new ProgressStore(30);
            Check(p.RecordResult("A",1,true,30),"initial result");Check(!p.RecordResult("A",1,true,30),"adjacent duplicate");
            Check(p.Data.completedMatches==1 && p.Data.unlockedStage==2,"first progression");
            p.RecordResult("B",30,true,30);Check(p.Data.unlockedStage==30,"stage31 unlocked");
            p.RecordResult("C",1,false,30);Check(p.Data.unlockedStage==30,"unlock regressed");
            var loaded=new ProgressStore(30);Check(loaded.Data.unlockedStage==30 && loaded.Data.completedMatches==3,"memory serialized roundtrip");
        });
        Run("progress.nonconsecutive_duplicate", "actual source; memory PlayerPrefs double", () =>
        {
            PlayerPrefs.Reset();var p=new ProgressStore(30);p.RecordResult("A",1,true,30);p.RecordResult("B",2,false,30);
            Check(!p.RecordResult("A",1,true,30),"A,B,A accepted; completedMatches="+p.Data.completedMatches);
        });
        Run("progress.duplicate_after_reload", "actual source; memory PlayerPrefs double", () =>
        {
            PlayerPrefs.Reset();var p=new ProgressStore(30);p.RecordResult("A",1,true,30);var loaded=new ProgressStore(30);
            Check(!loaded.RecordResult("A",1,true,30),"reloaded A accepted; completedMatches="+loaded.Data.completedMatches);
        });
        Run("progress.corrupt_save_recovery", "actual source; memory PlayerPrefs/JSON double", () =>
        {
            PlayerPrefs.Reset();PlayerPrefs.SetString("stagegame.progress.v1","{broken");var p=new ProgressStore(30);
            Check(p.Data.unlockedStage==1 && p.LoadWarning!=null,"recovery warning");
            Check(PlayerPrefs.GetString("stagegame.progress.v1.recovery")=="{broken","original lost");
        });
        Run("app.reward_callback_after_new_match", "actual GameApp with UI/coroutine doubles", () =>
        {
            var app=MakeApp();var ads=new DelayedAds();Set(app,"ads",ads);Call(app,"RequestHint");
            Check(ads.Rewards.Count==1,"request missing");app.StartStage(0);ads.Rewards[0](AdResult.Completed);
            Check(!((RewardGate)Get(app,"rewardGate")).Granted,"old callback rewards new match");
            Check(!(bool)Get(app,"busy"),"old callback starts search");
        });
        Run("app.reward_failure_duplicate_and_lobby", "actual GameApp with UI/coroutine doubles", () =>
        {
            var app=MakeApp();var ads=new DelayedAds();Set(app,"ads",ads);Call(app,"RequestHint");
            ads.Rewards[0](AdResult.Failed);Check(!((RewardGate)Get(app,"rewardGate")).Granted && app.hint.interactable,"failure blocks retry");
            Call(app,"RequestHint");ads.Rewards[0](AdResult.Completed);Check(!((RewardGate)Get(app,"rewardGate")).Granted,"old token grant");
            ads.Rewards[1](AdResult.Completed);int routines=app.Coroutines.Count;ads.Rewards[1](AdResult.Completed);
            Check(app.Coroutines.Count==routines,"duplicate starts second hint");
            app.ShowLobby();ads.Rewards[1](AdResult.Completed);Check(!app.match.activeSelf,"late callback reopens match");
        });
        Run("app.interstitial_callback_after_stage_change", "actual GameApp with UI/coroutine doubles", () =>
        {
            var app=MakeApp();var ads=new DelayedAds();Set(app,"ads",ads);var p=(ProgressStore)Get(app,"progress");p.Data.completedMatches=3;
            Set(app,"board",Replay(new[]{0,6,1,6,2,5,3}));Call(app,"Finish");Check(ads.Interstitials.Count==1,"eligible ad missing");
            Check(!app.retry.interactable,"actions enabled before resolution");app.StartStage(1);ads.Interstitials[0](AdResult.Completed);
            Check(p.Data.lastInterstitialMatch==0,"stale completion updates new match");Check(!app.result.activeSelf,"old result active");
        });
        Run("app.interstitial_no_callback_timeout", "actual GameApp with clock/coroutine doubles", () =>
        {
            var app=MakeApp();var ads=new DelayedAds();Set(app,"ads",ads);((ProgressStore)Get(app,"progress")).Data.completedMatches=3;
            Set(app,"board",Replay(new[]{0,6,1,6,2,5,3}));Call(app,"Finish");Time.realtimeSinceStartup=5;app.AdvanceCoroutines();
            Check(app.retry.interactable && app.resultLobby.interactable,"result remains blocked after timeout");
        });
        Run("app.reward_no_callback_timeout", "actual GameApp with clock/coroutine doubles", () =>
        {
            var app=MakeApp();Set(app,"ads",new DelayedAds());Call(app,"RequestHint");Time.realtimeSinceStartup=120;
            for(int i=0;i<5;i++) app.AdvanceCoroutines();
            Check(!((RewardGate)Get(app,"rewardGate")).Pending && app.hint.interactable,"reward stays pending indefinitely after 120 simulated seconds");
        });
        Run("app.pending_reward_blocks_moves_and_pause_cancels", "actual GameApp with UI/coroutine doubles", () =>
        {
            var app=MakeApp();var ads=new DelayedAds();Set(app,"ads",ads);Call(app,"RequestHint");
            string state=BoardState((ConnectFourBoard)Get(app,"board"));Call(app,"Drop",3);
            Check(state==BoardState((ConnectFourBoard)Get(app,"board")) && app.columns.All(c=>!c.interactable),"pending reward permits a move");
            Call(app,"OnApplicationPause",true);ads.Rewards[0](AdResult.Completed);Call(app,"OnApplicationPause",false);
            Check(!((RewardGate)Get(app,"rewardGate")).Granted && !((RewardGate)Get(app,"rewardGate")).Pending,"pause late grant");
            Check(app.columns.Any(c=>c.interactable),"pause cancellation locks input");
        });
        Run("app.save_failure_still_exits_finished_match", "actual GameApp; injected PlayerPrefs.Save exception", () =>
        {
            var app=MakeApp();Set(app,"board",Replay(new[]{0,6,1,6,2,5,3}));PlayerPrefs.ThrowOnSave=true;
            try { Call(app,"Finish"); } catch(TargetInvocationException) { }
            finally { PlayerPrefs.ThrowOnSave=false; }
            Check(app.result.activeSelf || app.lobby.activeSelf,"finished match has neither result nor lobby; resultRecorded="+Get(app,"resultRecorded"));
            var warning=((ProgressStore)Get(app,"progress")).SaveWarning;
            Check(!string.IsNullOrEmpty(warning) && app.resultAdStatus.text==((StringTable)Get(app,"strings")).Text(warning),"save failure warning is not visible");
        });
        if(args.Length>1)
            Run("data.actual_client_json", "actual GameData validation; JSON double", () =>
            {
                var data=LoadData(new[]{"stages","ads","strings"}.Select(n=>File.ReadAllText(Path.Combine(args[1],n+".json"))).ToArray());
                Check(data.Stages.Length==30,"actual imported stage count="+data.Stages.Length+", expected 30");
                foreach(var stage in data.Stages) Check(!GameData.CreateBoard(stage).IsFinished,"terminal imported stage "+stage.id);
            });
        RunLocalization(args);
        var report=new { runId=Path.GetFileName(Path.GetDirectoryName(args[0])), framework=System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
            passed, failed, assertions, scope=Filter.Length==0?"all":Filter, randomRuleSequences=ruleSequences, randomAiPositions=aiPositions,
            limitations="Unity APIs replaced by explicitly scoped doubles; no Editor, device, native JSON, real persistence, real ads, or performance validation.", results=Results };
        File.WriteAllText(args[0],JsonSerializer.Serialize(report,new JsonSerializerOptions{WriteIndented=true}));
        Console.WriteLine($"SUMMARY pass={passed} fail={failed} assertions={assertions}");
        if(args.Length>2 && Filter.Length==0 && !args.Contains("--skip-stage-exploration")) ExploreStages(args[2],Path.Combine(Path.GetDirectoryName(args[0]),"stage-paths.json"));
        return failed==0?0:1;
    }

    static int PlainSearch(ConnectFourBoard board,int depth,int budget,int mistakes,int seed)
    {
        using var search=new AiSearch(board,depth,budget,mistakes,seed);
        while(!search.Complete) search.Step(512);
        return search.ChosenColumn;
    }
    static void ProveFixed(string directory,string output,string ids)
    {
        var data=LoadData(new[]{"stages","ads","strings"}.Select(n=>File.ReadAllText(Path.Combine(directory,n+".json"))).ToArray());
        var records=new List<object>();
        var selected=ids.Split(',').Select(int.Parse).ToArray();
        foreach(var stage in data.Stages.Where(s=>selected.Contains(s.id)))
        {
            foreach(int depth in new[]{8,12,16,24,32})
            {
                int visited=0;var clock=Stopwatch.StartNew();var cache=new Dictionary<string,int>();var aiCache=new Dictionary<string,int>();var playerChoices=new Dictionary<string,int>();
                int Solve(ConnectFourBoard board,int left)
                {
                    if(++visited>200000 || clock.Elapsed.TotalSeconds>10) throw new TimeoutException();
                    if(board.IsFinished) return board.Winner;
                    if(left==0) return 2;
                    string state=BoardState(board),key=state+"/"+left;
                    if(cache.TryGetValue(key,out int value)) return value;
                    if(board.CurrentPlayer==-1)
                    {
                        if(!aiCache.TryGetValue(state,out int col))
                        {
                            col=PlainSearch(board,stage.aiDepth,stage.aiNodeBudget,stage.aiRandomPermille,(stage.id-1)*997+board.MoveCount*31);
                            aiCache[state]=col;
                        }
                        var next=board.Clone();if(!next.TryDrop(col))throw new Exception("fixed AI illegal");
                        return cache[key]=Solve(next,left-1);
                    }
                    bool unknown=false,draw=false;
                    foreach(int col in new[]{3,2,4,1,5,0,6}.Where(board.CanDrop))
                    {
                        var next=board.Clone();next.TryDrop(col);int child=Solve(next,left-1);
                        if(child==1) { playerChoices[key]=col; return cache[key]=1; }
                        if(child==2) unknown=true;if(child==0)draw=true;
                    }
                    return cache[key]=unknown?2:draw?0:-1;
                }
                string status;
                try
                {
                    int result=Solve(GameData.CreateBoard(stage),depth);
                    status=result==-1?"EVERY_PLAYER_PATH_LOSES_TO_FIXED_AI":result==1?"PLAYER_WIN_EXISTS_TO_FIXED_AI":result==0?"DRAW_AVAILABLE_NO_WIN_IN_COMPLETE_TREE":"INCOMPLETE_DEPTH_NOT_A_VERDICT";
                }
                catch(TimeoutException){status="SEARCH_LIMIT_NOT_A_VERDICT";}
                List<int> winningContinuation=null;
                if(status=="PLAYER_WIN_EXISTS_TO_FIXED_AI")
                {
                    winningContinuation=new List<int>();var board=GameData.CreateBoard(stage);int left=depth;
                    while(!board.IsFinished)
                    {
                        string state=BoardState(board);
                        int col=board.CurrentPlayer==1?playerChoices[state+"/"+left]:aiCache[state];
                        if(!board.TryDrop(col))throw new Exception("winning witness illegal");
                        winningContinuation.Add(col);left--;
                    }
                    var oracle=new Oracle(stage.starter);
                    var opening=stage.openingMoves.Length==0?Array.Empty<int>():stage.openingMoves.Split(',').Select(int.Parse).ToArray();
                    foreach(int col in opening.Concat(winningContinuation))if(!oracle.Drop(col))throw new Exception("winning witness oracle rejects");
                    if(oracle.Winner!=1)throw new Exception("winning witness oracle disagrees");
                }
                records.Add(new{stage=stage.id,depth,status,visited,aiCalls=aiCache.Count,seconds=clock.Elapsed.TotalSeconds,winningContinuation});
                File.WriteAllText(output,JsonSerializer.Serialize(new{method="All player legal moves, exactly one actual fixed-seed AI move at each AI turn; terminal outcomes only, unknown frontier never counted as loss. Actual C# rules previously cross-checked with independent oracle. No GameApp wall-clock cutoff.",results=records},new JsonSerializerOptions{WriteIndented=true}));
                Console.WriteLine("FIXED "+stage.id+" depth="+depth+" "+status+" nodes="+visited);
                if(status=="EVERY_PLAYER_PATH_LOSES_TO_FIXED_AI" || status=="PLAYER_WIN_EXISTS_TO_FIXED_AI" || status=="DRAW_AVAILABLE_NO_WIN_IN_COMPLETE_TREE") break;
            }
        }
    }
    static void ExploreStages(string directory,string output)
    {
        var data=LoadData(new[]{"stages","ads","strings"}.Select(n=>File.ReadAllText(Path.Combine(directory,n+".json"))).ToArray());
        var records=new List<object>();
        foreach(var stage in data.Stages)
        {
            var initial=GameData.CreateBoard(stage);
            var prefix=stage.openingMoves.Length==0?new List<int>():stage.openingMoves.Split(',').Select(int.Parse).ToList();
            var immediateProof=new Dictionary<int,int[]>();
            bool forcedReplyLoss=initial.CurrentPlayer==1;
            foreach(int choice in Enumerable.Range(0,7).Where(initial.CanDrop))
            {
                var oracle=new Oracle(stage.starter);foreach(int col in prefix) oracle.Drop(col);oracle.Drop(choice);
                if(oracle.Winner==1 || oracle.Finished){forcedReplyLoss=false;immediateProof[choice]=Array.Empty<int>();continue;}
                var replies=new List<int>();
                foreach(int reply in Enumerable.Range(0,7).Where(c=>oracle.Landing(c)>=0))
                {
                    var response=new Oracle(stage.starter);foreach(int col in prefix) response.Drop(col);response.Drop(choice);response.Drop(reply);
                    if(response.Winner==-1) replies.Add(reply);
                }
                immediateProof[choice]=replies.ToArray();if(replies.Count==0) forcedReplyLoss=false;
            }
            var attempts=new List<object>();List<int> winningPath=null;
            if(!forcedReplyLoss)
                for(int attempt=0;attempt<12 && winningPath==null;attempt++)
                {
                    var board=GameData.CreateBoard(stage);var path=new List<int>();int playerTurns=0;
                    while(!board.IsFinished)
                    {
                        int column;
                        if(board.CurrentPlayer==-1)
                            column=PlainSearch(board,stage.aiDepth,stage.aiNodeBudget,stage.aiRandomPermille,(stage.id-1)*997+board.MoveCount*31);
                        else
                        {
                            int depth=attempt==1?4:attempt==2?5:6;
                            int mistakes=attempt>=3 && playerTurns<4?300:0;
                            column=PlainSearch(board,depth,30000,mistakes,stage.id*100000+attempt*100+board.MoveCount);
                            playerTurns++;
                        }
                        if(!board.TryDrop(column)) throw new Exception("stage exploration illegal move "+stage.id);
                        path.Add(column);
                    }
                    var oracle=new Oracle(stage.starter);foreach(int c in prefix.Concat(path)) if(!oracle.Drop(c)) throw new Exception("oracle path invalid");
                    if(oracle.Winner!=board.Winner) throw new Exception("oracle exploration winner mismatch");
                    attempts.Add(new{attempt,winner=board.Winner,continuation=path.ToArray()});
                    if(board.Winner==1) winningPath=path;
                }
            string status=forcedReplyLoss?"FORCED_LOSS_NEXT_AI_REPLY":winningPath!=null?"EXAMPLE_WIN_FOUND":"NO_WIN_FOUND_NOT_PROOF";
            records.Add(new{stage=stage.id,status,openingMoves=stage.openingMoves,starter=stage.starter,immediateReplyProof=immediateProof,winningContinuation=winningPath,attempts});
            File.WriteAllText(output,JsonSerializer.Serialize(new{
                method="Player AiSearch depth 4/5/6, budget 30000; up to 12 diversified attempts. Stage AI uses GameApp fixed seed formula (id-1)*997+MoveCount*31 and completes search; does NOT emulate GameApp 0.65s wall-clock cutoff. Whole-board oracle checks terminal path. Immediate-loss proof enumerates every player move and all immediate AI wins.",
                limitations="Example paths only, not optimal-play forced wins, human playability, balance, fun, device behavior, or full solvability proof. Runtime cutoff can change AI moves.",stages=records
            },new JsonSerializerOptions{WriteIndented=true}));
            Console.WriteLine("STAGE "+stage.id+" "+status+" attempts="+attempts.Count);
        }
    }
}
