using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using StageGame;
using StageGameIndependentQa;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

static class Program
{
    const string SaveKey = "stagegame.progress.v1";
    static string Evidence, Filter, StagesJson, StringsJson, AdsJson;
    static int assertions, failed, passed;
    static readonly List<object> Results = new List<object>();
    static readonly int[] Draw = {0,2,2,2,2,2,2,0,0,0,0,0,1,3,3,3,3,3,3,1,1,1,1,1,4,6,6,6,6,6,6,4,4,4,4,4,5,5,5,5,5,5};
    static void Check(bool condition, string label) { assertions++; if (!condition) throw new Exception(label); }
    static void Run(string name, Action test)
    {
        if (Filter.Length > 0 && !Filter.Split(',').Any(name.StartsWith)) return;
        int before = assertions; var watch = Stopwatch.StartNew(); string error = null;
        try { test(); passed++; }
        catch (Exception e) { while (e is TargetInvocationException && e.InnerException != null) e=e.InnerException; failed++; error=e.GetType().Name+": "+e.Message; }
        Results.Add(new {name, status=error==null?"PASS":"FAIL", assertions=assertions-before, milliseconds=watch.ElapsedMilliseconds, error});
        Console.WriteLine((error==null?"PASS ":"FAIL ")+name+(error==null?"":" | "+error));
    }
    static T Get<T>(object instance,string name) => (T)instance.GetType().GetField(name,BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).GetValue(instance);
    static void Set(object instance,string name,object value) => instance.GetType().GetField(name,BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(instance,value);
    static object Call(object instance,string name,params object[] args) => instance.GetType().GetMethod(name,BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).Invoke(instance,args);
    static ConnectFourBoard Board(RankGameApp app) => Get<ConnectFourBoard>(app,"board");
    static ProgressStore Progress(RankGameApp app) => Get<ProgressStore>(app,"progress");
    static RewardGate Gate(RankGameApp app) => Get<RewardGate>(app,"rewardGate");
    static string State(ConnectFourBoard b) => b.CurrentPlayer+":"+b.MoveCount+":"+b.Winner+":"+b.LastMove+":"+string.Join(",",Enumerable.Range(0,42).Select(i=>b[i%7,i/7]))+":"+string.Join(",",b.WinningCells.OrderBy(i=>i));
    static void Click(Button button) { Check(button.interactable && button.gameObject.activeSelf,"button must be enabled/visible before simulated click"); button.onClick.Invoke(); }
    static void Step(RankGameApp app,int count=1)
    {
        for(int i=0;i<count;i++) { Time.unscaledDeltaTime=.05f; Time.realtimeSinceStartup+=.05f; app.AdvanceCoroutines(); }
    }
    static void Until(RankGameApp app,Func<bool> ready,string label,int maximum=200)
    {
        int n=0; while(!ready() && n++<maximum) Step(app); Check(ready(),label+" did not settle");
    }
    static void ResumeStale(Coroutine coroutine)
    {
        if(coroutine==null)return;
        // Deliberately resume an already-cancelled iterator to exercise its own generation guards.
        int n=0; while(coroutine.Iterator.MoveNext() && n++<100) Time.realtimeSinceStartup+=.05f;
        Check(n<100,"cancelled AI iterator did not exit");
    }
    sealed class DelayedAds : IAdService
    {
        public string Status => "ad_test_mode";
        public readonly List<Action<AdResult>> Rewards=new List<Action<AdResult>>(), Interstitials=new List<Action<AdResult>>();
        public int CancelCalls;
        public void ShowRewarded(Action<AdResult> callback) => Rewards.Add(callback);
        public void ShowInterstitial(Action<AdResult> callback) => Interstitials.Add(callback);
        public void CancelPending() { CancelCalls++; } // Retain callbacks to inject stale/duplicate SDK notifications.
    }
    static RankGameApp Make(bool fresh=false,string raw=null,bool reset=true,bool failSave=false,string stageOverride=null)
    {
        if(reset)PlayerPrefs.Reset(); PlayerPrefs.ThrowOnSave=failSave; Debug.Errors.Clear();
        Time.realtimeSinceStartup=0; Time.unscaledDeltaTime=.05f;
        if(reset && !fresh)PlayerPrefs.SetString(SaveKey,raw??"{\"saveVersion\":1,\"unlockedStage\":1}");
        Resources.Assets["GameData/stages"]=new TextAsset{text=stageOverride??StagesJson};
        Resources.Assets["GameData/strings"]=new TextAsset{text=StringsJson};
        Resources.Assets["GameData/ads"]=new TextAsset{text=AdsJson};
        var app=new RankGameApp();
        foreach(var field in typeof(RankGameApp).GetFields(BindingFlags.Public|BindingFlags.Instance))
        {
            Type t=field.FieldType;
            if(t==typeof(GameObject)||t==typeof(Text)||t==typeof(Button)||t==typeof(Image)||t==typeof(RectTransform)||t==typeof(ScrollRect)||t==typeof(AudioSource)||t==typeof(AudioClip))
                field.SetValue(app,Activator.CreateInstance(t));
            if(t==typeof(Button))((Button)field.GetValue(app)).gameObject.SetActive(true);
        }
        app.theme=new RankTheme();
        foreach(var field in typeof(RankTheme).GetFields())if(field.FieldType==typeof(Sprite))field.SetValue(app.theme,new Sprite{name=field.Name});
        app.stones=Enumerable.Range(0,42).Select(_=>new Image()).ToArray();
        app.detailStones=Enumerable.Range(0,42).Select(_=>new Image()).ToArray();
        var parent=new Transform();
        for(int i=0;i<42;i++){app.stones[i].transform.SetParent(parent);app.stones[i].rectTransform.anchoredPosition=new Vector2(i%7*100,i/7*100);}
        app.columns=Enumerable.Range(0,7).Select(_=>new Button()).ToArray();
        foreach(var button in app.columns)button.gameObject.SetActive(true);
        app.cards=Enumerable.Range(0,30).Select(_=>new RankStageCard{button=new Button(),rank=new Text(),number=new Text(),stars=new Text(),state=new Image()}).ToArray();
        foreach(var card in app.cards)card.button.gameObject.SetActive(true);
        app.localizedLabels=Array.Empty<LocalizedLabel>();
        app.normalBoardSlot.anchoredPosition=new Vector2(0,40); app.resultBoardSlot.anchoredPosition=new Vector2(0,100);
        app.normalBoardSlot.localScale=Vector3.one;app.resultBoardSlot.localScale=Vector3.one*.8f;
        Call(app,"Start"); return app;
    }
    static RankGameApp Opening(int[] moves=null,int starter=1)
    {
        var app=Make(); Check(Debug.Errors.Count==0,"valid app startup error");
        var stage=Get<GameData>(app,"data").Stages[0]; stage.openingMoves=string.Join(",",moves??Array.Empty<int>());stage.starter=starter;
        app.StartStage(0); return app;
    }
    static void PlaceAndReply(RankGameApp app,int column)
    {
        int before=Board(app).MoveCount; app.SelectColumn(column);app.PlaceSelected();
        Check(Board(app).MoveCount==before+1,"player move accepted exactly once");
        Until(app,()=>!Get<bool>(app,"busy"),"actual AI reply");
        Check(Board(app).MoveCount==before+2,"one real AI reply");
    }
    // Independent all-four-cell-window oracle; no production LastMove ray scan.
    static ConnectFourBoard Replay(int[] moves,int starter=1)
    {
        var actual=new ConnectFourBoard(starter); int[] cells=new int[42],heights=new int[7];int winner=0,last=-1;
        for(int ply=0;ply<moves.Length;ply++)
        {
            int col=moves[ply]; Check(winner==0 && col>=0 && col<7 && heights[col]<6,"reference legal prefix");
            last=heights[col]++*7+col;cells[last]=ply%2==0?starter:-starter;
            var winning=new HashSet<int>();
            foreach(var d in new[]{(1,0),(0,1),(1,1),(-1,1)})for(int y=0;y<6;y++)for(int x=0;x<7;x++)
            {
                int endX=x+3*d.Item1,endY=y+3*d.Item2;if(endX<0||endX>=7||endY<0||endY>=6||cells[y*7+x]==0)continue;
                int[] line=Enumerable.Range(0,4).Select(n=>(y+n*d.Item2)*7+x+n*d.Item1).ToArray();
                if(line.All(i=>cells[i]==cells[line[0]])){winner=cells[line[0]];winning.UnionWith(line);}
            }
            Check(actual.TryDrop(col),"actual drop rejected reference move");
            Check(actual.MoveCount==ply+1&&actual.LastMove==last&&actual.Winner==winner&&actual.CurrentPlayer==(ply%2==0?-starter:starter),"board metadata vs oracle");
            Check(winning.SetEquals(actual.WinningCells),"winning cells vs oracle");
            for(int i=0;i<42;i++)Check(actual[i%7,i/7]==cells[i],"board cell vs oracle");
        }
        return actual;
    }
    static RankGameApp Terminal(int winner=1,int stageId=1,DelayedAds ads=null)
    {
        var app=Make(raw:"{\"saveVersion\":1,\"unlockedStage\":30}");app.StartStage(stageId-1);Call(app,"CancelSearch");
        Set(app,"board",winner==0?Replay(Draw):Replay(new[]{0,6,1,6,2,5,3},winner));
        if(ads!=null){Set(app,"ads",ads);Progress(app).Data.completedMatches=3;}
        Call(app,"Finish");return app;
    }
    static void AssertLine(RankGameApp app)
    {
        Check(app.winLine.gameObject.activeSelf,"winning line not active");
        var rt=app.winLine.rectTransform;float angle=rt.localEulerAngles.z/Mathf.Rad2Deg;
        Vector2 offset=new Vector2(MathF.Cos(angle),MathF.Sin(angle))*rt.sizeDelta.x/2;
        Vector2 a=rt.anchoredPosition-offset,b=rt.anchoredPosition+offset;
        int Cell(Vector2 p){int x=(int)MathF.Round(p.x/100),y=(int)MathF.Round(p.y/100);Check(MathF.Abs(p.x-x*100)<.02f&&MathF.Abs(p.y-y*100)<.02f,"line endpoints outside cell centers");return y*7+x;}
        var board=Board(app);int[] cells=Enumerable.Range(0,42).Select(i=>board[i%7,i/7]).ToArray();
        Check(WinningSegmentOracle.IsValid(cells,Cell(a),Cell(b),board.LastMove,board.Winner),"rendered endpoints fail independent straight winning segment oracle");
    }

    static int Main(string[] args)
    {
        Evidence=args[0];Filter=args.Length>1?args[1]:"";
        StagesJson=File.ReadAllText(Path.Combine(Evidence,"ClientData","stages.json"));
        StringsJson=File.ReadAllText(Path.Combine(Evidence,"ClientData","strings.json"));
        AdsJson=File.ReadAllText(Path.Combine(Evidence,"ClientData","ads.json"));
        RegisterTests();
        File.WriteAllText(Path.Combine(Evidence,"results.json"),JsonSerializer.Serialize(new {
            passed,failed,assertions,tests=Results,
            actualProductTypes=new[]{"RankGameApp","RankTheme","RankStageCard","RankDetailBlocker","GameData","ProgressStore","ConnectFourBoard","AiSearch","StringTable","Advertising"},
            environment=".NET10 actual snapshotted C#; explicit Unity UI/PlayerPrefs/JsonUtility/manual coroutine doubles; real AiSearch; retained fake ad callbacks",
            limitations="Injected legal opening/terminal positions where stated. No native serialization, saved prefab geometry, real pointer/raycast, native frame timing, physical persistence, device performance, real ads, IAP, Toss, build or release acceptance. Old GameApp/Tabletop compiled only for shared LocalizedLabel dependency, never tested as Rank UI."
        },new JsonSerializerOptions{WriteIndented=true}));
        Console.WriteLine($"RESULT: {passed} passed, {failed} failed, {assertions} assertions");return failed==0?0:1;
    }
    static void RegisterTests()
    {
        Run("data.frozen_rank_contract",()=>
        {
            foreach(string name in new[]{"stages","strings","ads"})Check(File.ReadAllBytes(Path.Combine(Evidence,"ClientData",name+".json")).SequenceEqual(File.ReadAllBytes(Path.Combine(Evidence,"GeneratedData",name+".json"))),"Client/Generated mismatch "+name);
            var data=new GameData(StagesJson,AdsJson,StringsJson);data.ValidateRankData();
            Check(data.Stages.Length==30&&JsonUtility.FromJson<StringFile>(StringsJson).strings.Length==228,"frozen counts");
            foreach(var stage in data.Stages)
            {
                int rank=(stage.id-1)/3+1;Check(stage.rankKey==$"rank_{rank:00}"&&stage.difficultyStars==(stage.id-1)/6+1,"rank/stars");
                Check(stage.aiIntroKey==$"ai_rank_{rank:00}_intro"&&stage.aiWinKey==$"ai_rank_{rank:00}_win"&&stage.aiLoseKey==$"ai_rank_{rank:00}_lose","AI dialogue direction");
            }
        });
        Run("data.invalid_rank_fields_rejected_safe_start",()=>
        {
            foreach(string field in new[]{"rankKey","descriptionKey","aiIntroKey","aiWinKey","aiLoseKey"})foreach(string value in new string[]{null,""," ","missing-key"})
            {
                var file=JsonUtility.FromJson<StageFile>(StagesJson);typeof(StageDefinition).GetField(field).SetValue(file.stages[0],value);string json=JsonUtility.ToJson(file);
                bool rejected=false;try{new GameData(json,AdsJson,StringsJson).ValidateRankData();}catch(FormatException){rejected=true;}Check(rejected,field+" invalid accepted");
                var app=Make(stageOverride:json);Check(Debug.Errors.Count==1&&app.lobby.activeSelf&&!app.match.activeSelf&&app.cards.All(c=>!c.button.interactable),"invalid data safe startup "+field);
            }
            foreach(int stars in new[]{0,6,2}){var file=JsonUtility.FromJson<StageFile>(StagesJson);file.stages[0].difficultyStars=stars;bool rejected=false;try{new GameData(JsonUtility.ToJson(file),AdsJson,StringsJson).ValidateRankData();}catch(FormatException){rejected=true;}Check(rejected,"invalid stars accepted "+stars);}
        });
        Run("firstload.fresh_once_and_existing_stage1",()=>
        {
            var app=Make(fresh:true);Check(Debug.Errors.Count==0&&app.match.activeSelf&&!app.lobby.activeSelf&&Get<int>(app,"activeStage")==0,"fresh must start stage1");
            Check(PlayerPrefs.HasKey(SaveKey)&&Board(app).MoveCount==0,"fresh marker saved before play");
            app.ShowLobby();var second=Make(reset:false);Check(second.lobby.activeSelf&&!second.match.activeSelf&&Board(second)==null,"later launch must use lobby");
            var existing=Make(raw:"{\"saveVersion\":1,\"unlockedStage\":1,\"language\":\"en\",\"soundEnabled\":false}");
            Check(existing.lobby.activeSelf&&Board(existing)==null&&Progress(existing).Data.language=="en"&&!Progress(existing).Data.soundEnabled,"existing saved stage1 incorrectly treated as fresh");
        });
        Run("firstload.legacy_saves_and_corrupt_recovery",()=>
        {
            using var doc=JsonDocument.Parse(File.ReadAllText(Path.Combine(Evidence,"AuditInputs","stage-ux-legacy-saves.json")));
            foreach(var row in doc.RootElement.GetProperty("cases").EnumerateArray())
            {
                if(row.GetProperty("rawSaveJson").ValueKind==JsonValueKind.Null)continue;
                string raw=row.GetProperty("rawSaveJson").GetString();var expected=row.GetProperty("expected");var app=Make(raw:raw);var p=Progress(app).Data;
                Check(app.lobby.activeSelf&&Board(app)==null,"legacy auto-start "+row.GetProperty("id"));
                Check(p.unlockedStage==expected.GetProperty("unlockedStage").GetInt32()&&p.highestClearedStage==expected.GetProperty("highestActuallyCompletedStage").GetInt32(),"legacy progress");
                Check(p.completedMatches==expected.GetProperty("completedMatches").GetInt32()&&p.lastInterstitialMatch==expected.GetProperty("lastInterstitialMatch").GetInt32()&&p.lastInterstitialUtc==expected.GetProperty("lastInterstitialUtc").GetInt64(),"legacy ad counters");
                Check(p.language==expected.GetProperty("language").GetString()&&p.soundEnabled==expected.GetProperty("soundEnabled").GetBoolean(),"legacy preferences");
                Check(p.processedMatchIds.SequenceEqual(expected.GetProperty("processedMatchIds").EnumerateArray().Select(s=>s.GetString())),"legacy dedup ids");
                Check(PlayerPrefs.GetString(SaveKey)==raw,"startup must not rewrite existing raw save");
            }
            var corrupt=Make(raw:"{bad-json");Check(corrupt.lobby.activeSelf&&Board(corrupt)==null&&Progress(corrupt).LoadWarning!=null,"corrupt key is existing not fresh");
            Check(PlayerPrefs.GetString(SaveKey)=="{bad-json"&&PlayerPrefs.GetString(SaveKey+".recovery")=="{bad-json","corrupt raw recovery preservation");
        });
        Run("firstload.save_failure_keeps_playable_session",()=>
        {
            var app=Make(fresh:true,failSave:true);Check(app.match.activeSelf&&Board(app).MoveCount==0&&Progress(app).SaveWarning=="client_error_save","fresh save failure prevents play or hides warning state");
            app.SelectColumn(3);app.PlaceSelected();Check(Board(app).MoveCount==1,"fresh save failure blocks move");
            // In-memory SetString succeeds in the double. This does not prove persistence across process death.
        });
        Run("input.preview_nonmutating_lowest_empty_full_column",()=>
        {
            var app=Opening(new[]{3,2,3,2});string before=State(Board(app)),id=Get<string>(app,"matchId"),save=PlayerPrefs.GetString(SaveKey);
            foreach(int col in new[]{4,1,3})app.SelectColumn(col);
            Check(State(Board(app))==before&&Get<string>(app,"matchId")==id&&PlayerPrefs.GetString(SaveKey)==save,"preview mutated board/match/save");
            Check(app.movePreview.gameObject.activeSelf&&app.place.interactable&&app.movePreview.rectTransform.anchoredPosition.x==300&&app.movePreview.rectTransform.anchoredPosition.y==200,"preview must use lowest empty cell");
            app=Opening(new[]{0,0,0,0,0,0});before=State(Board(app));app.SelectColumn(3);app.SelectColumn(0);app.PlaceSelected();
            Check(State(Board(app))==before&&!app.movePreview.gameObject.activeSelf&&!app.place.interactable&&Get<int>(app,"selectedColumn")==-1,"full column must clear preview and reject Place");
            foreach(int col in new[]{-1,7}){app.SelectColumn(col);app.PlaceSelected();Check(State(Board(app))==before,"invalid column mutated board");}
        });
        Run("input.place_repeat_real_ai_exactly_once",()=>
        {
            var app=Opening(new[]{3,2});app.PlaceSelected();Check(Board(app).MoveCount==2,"empty selection accepted");
            app.SelectColumn(4);app.PlaceSelected();app.PlaceSelected();app.SelectColumn(0);app.PlaceSelected();
            Check(Board(app).MoveCount==3&&Get<bool>(app,"busy")&&!app.place.interactable,"repeat Place while AI searching");
            Until(app,()=>!Get<bool>(app,"busy"),"one AI reply");Step(app,8);
            Check(Board(app).MoveCount==4&&Board(app).CurrentPlayer==1&&Get<List<ConnectFourBoard>>(app,"undoHistory").Count==1,"duplicate AI or history");
            Check(Progress(app).Data.completedMatches==0,"nonterminal move recorded result");
        });
        Run("input.hidden_screen_and_duplicate_next",()=>
        {
            var app=Opening(new[]{3,2});PlaceAndReply(app,4);app.ShowLobby();string state=State(Board(app));var ads=new DelayedAds();Set(app,"ads",ads);
            app.SelectColumn(0);app.PlaceSelected();app.UndoTurn();Call(app,"OpenPause");Call(app,"RequestHint");app.matchRetry.onClick.Invoke();
            Check(State(Board(app))==state&&app.lobby.activeSelf&&!app.match.activeSelf&&!app.confirm.activeSelf&&ads.Rewards.Count==0,"hidden match handler mutated state");
            var result=Terminal();Until(result,()=>result.result.activeSelf&&!Get<bool>(result,"resolving"),"Next setup");
            result.nextStage.onClick.Invoke();string id=Get<string>(result,"matchId");Check(Get<int>(result,"activeStage")==1,"first Next");result.nextStage.onClick.Invoke();result.retry.onClick.Invoke();
            Check(Get<int>(result,"activeStage")==1&&Get<string>(result,"matchId")==id&&!result.result.activeSelf,"duplicate hidden result handler starts new match");
        });
        Run("undo.pending_search_and_stale_response",()=>
        {
            var app=Opening(new[]{3,2});string before=State(Board(app));int generation=Get<int>(app,"searchGeneration");
            app.SelectColumn(4);app.PlaceSelected();var stale=Get<Coroutine>(app,"aiRoutine");app.UndoTurn();
            Check(State(Board(app))==before&&Get<int>(app,"searchGeneration")>generation&&!Get<bool>(app,"busy"),"pending Undo failed");
            ResumeStale(stale);Step(app,20);Check(State(Board(app))==before&&!app.movePreview.gameObject.activeSelf&&!app.place.interactable,"stale response after Undo");
            Check(Progress(app).Data.completedMatches==0,"Undo changed results");
        });
        Run("undo.completed_pairs_opening_floor_and_alternative",()=>
        {
            var app=Opening(new[]{3,2});string opening=State(Board(app));PlaceAndReply(app,4);string pair=State(Board(app));PlaceAndReply(app,0);
            app.UndoTurn();Check(State(Board(app))==pair,"second pair rollback");app.UndoTurn();Check(State(Board(app))==opening,"first pair rollback");
            app.UndoTurn();Check(State(Board(app))==opening&&!app.undo.interactable,"opening floor");
            PlaceAndReply(app,0);Check(Board(app)[0,0]==1,"alternative Place after Undo");app.UndoTurn();Check(State(Board(app))==opening,"alternative rollback");
            Check(Progress(app).Data.completedMatches==0&&Progress(app).Data.unlockedStage==1,"Undo changed progression");
        });
        Run("undo.ai_starter_and_completion_order_race",()=>
        {
            var aiStart=Opening(new[]{3},-1);string initial=State(Board(aiStart));PlaceAndReply(aiStart,4);aiStart.UndoTurn();aiStart.UndoTurn();Check(State(Board(aiStart))==initial&&Board(aiStart).CurrentPlayer==1,"AI opening floor");
            foreach(bool replyFirst in new[]{false,true})
            {
                var app=Opening(new[]{3,2});string before=State(Board(app));app.SelectColumn(4);app.PlaceSelected();var stale=Get<Coroutine>(app,"aiRoutine");
                if(replyFirst)Until(app,()=>!Get<bool>(app,"busy"),"AI before Undo");app.UndoTurn();if(!replyFirst)ResumeStale(stale);
                Check(State(Board(app))==before,"AI/Undo event order changed rollback");
            }
        });
        Run("cancel.retry_lobby_new_stage_reject_old_ai",()=>
        {
            foreach(string action in new[]{"retry","lobby","other-stage"})
            {
                var app=Opening(new[]{3,2});string id=Get<string>(app,"matchId");app.SelectColumn(4);app.PlaceSelected();var stale=Get<Coroutine>(app,"aiRoutine");
                if(action=="retry")Click(app.matchRetry);else if(action=="lobby")app.ShowLobby();else{Progress(app).Data.unlockedStage=2;app.StartStage(1);}
                string state=State(Board(app));ResumeStale(stale);Step(app,5);Check(State(Board(app))==state,"old AI contaminated "+action);
                Check(Progress(app).Data.completedMatches==0,"cancellation recorded result");
                Check(action=="lobby"?Get<string>(app,"matchId")==null:Get<string>(app,"matchId")!=id,"match identity after "+action);
            }
        });
        Run("pause.search_language_change_resume",()=>
        {
            foreach(bool application in new[]{false,true})
            {
                var app=Opening(new[]{3,2});app.SelectColumn(4);app.PlaceSelected();string state=State(Board(app));
                if(application)Call(app,"OnApplicationPause",true);else Click(app.pause);
                app.SetLanguage("en");Step(app,20);Check(State(Board(app))==state,"AI advanced while paused");
                app.SelectColumn(0);app.PlaceSelected();app.UndoTurn();Check(State(Board(app))==state,"paused input changed board");
                if(application)Call(app,"OnApplicationPause",false);else Click(app.confirmResume);
                Until(app,()=>!Get<bool>(app,"busy"),"resume AI");Check(Board(app).MoveCount==4&&Progress(app).Data.language=="en","resume or language preservation");
            }
        });
        Run("hint.completed_duplicate_undo_consumption_preserved",()=>
        {
            var app=Opening(new[]{3,2});PlaceAndReply(app,4);var ads=new DelayedAds();Set(app,"ads",ads);string state=State(Board(app));
            Click(app.pause);Click(app.hint);Check(ads.Rewards.Count==1&&Gate(app).Pending&&!app.confirm.activeSelf,"hint request via pause");
            ads.Rewards[0](AdResult.Completed);ads.Rewards[0](AdResult.Completed);Until(app,()=>!Get<bool>(app,"busy"),"hint computation");
            Check(State(Board(app))==state&&Gate(app).Granted&&app.movePreview.gameObject.activeSelf,"hint must select only");
            app.UndoTurn();Check(Gate(app).Granted&&!Gate(app).Pending&&!app.hint.interactable&&Get<int>(app,"selectedColumn")==-1&&!app.movePreview.gameObject.activeSelf,"Undo restored hint entitlement or display");
            Call(app,"RequestHint");Check(ads.Rewards.Count==1&&Progress(app).Data.completedMatches==0,"Undo granted a second hint/result");
        });
        Run("hint.cancel_timeout_and_stale_callbacks",()=>
        {
            foreach(string action in new[]{"undo","retry","lobby","pause","timeout"})
            {
                var app=Opening(new[]{3,2});PlaceAndReply(app,4);var ads=new DelayedAds();Set(app,"ads",ads);Call(app,"RequestHint");var original=Gate(app);Check(original.Pending,"pending hint");
                if(action=="undo")app.UndoTurn();else if(action=="retry")app.StartStage(0);else if(action=="lobby")app.ShowLobby();else if(action=="pause")Call(app,"OnApplicationPause",true);else Step(app,125);
                string state=State(Board(app));ads.Rewards[0](AdResult.Completed);Check(!original.Granted&&!original.Pending&&State(Board(app))==state,"stale reward after "+action);
            }
            foreach(AdResult value in new[]{AdResult.Failed,AdResult.Unavailable,AdResult.Cancelled})
            {
                var app=Opening();var ads=new DelayedAds();Set(app,"ads",ads);Call(app,"RequestHint");ads.Rewards[0](value);Check(!Gate(app).Granted&&!Gate(app).Pending,"noncompletion rewarded");app.SelectColumn(3);app.PlaceSelected();Check(Board(app).MoveCount==1,"failed reward blocked progress");
            }
        });
        Run("result.both_winners_timeline_input_lock_single_save",()=>
        {
            foreach(int winner in new[]{1,-1})
            {
                var ads=new DelayedAds();var app=Terminal(winner,1,ads);var stone=app.stones[Board(app).LastMove].rectTransform;
                Check(Get<bool>(app,"resolving")&&!app.result.activeSelf&&MathF.Abs(stone.localScale.x-1.45f)<.001f,"initial winning animation");
                Check(!app.turnLabel.gameObject.activeSelf,"Turn text must hide from start of terminal animation");
                Check(stone.GetSiblingIndex()==41&&!app.undo.interactable&&!app.place.interactable&&!app.matchRetry.interactable&&!app.pause.interactable,"resolving ordering/input");
                string state=State(Board(app));app.SelectColumn(0);app.PlaceSelected();app.UndoTurn();Call(app,"OpenPause");Call(app,"RequestHint");Call(app,"Finish");
                Check(State(Board(app))==state&&!app.confirm.activeSelf&&Progress(app).Data.completedMatches==4&&ads.Rewards.Count==0,"resolving input/save duplicate");
                Step(app,7);Check(!app.result.activeSelf&&ads.Interstitials.Count==0,"ad/result before .4 seconds");
                Step(app,3);Check(app.winLine.gameObject.activeSelf&&stone.localScale.x==1&&!app.result.activeSelf&&ads.Interstitials.Count==0,"win line observation interval");AssertLine(app);
                Until(app,()=>ads.Interstitials.Count==1,"post-observation ad");Check(Time.realtimeSinceStartup>=.9f&&app.result.activeSelf&&!app.retry.interactable,"ad timing/buttons");
                Check(!app.pause.gameObject.activeSelf&&!app.undo.gameObject.activeSelf&&!app.place.gameObject.activeSelf&&!app.matchRetry.gameObject.activeSelf,"result hides in-game controls");
                ads.Interstitials[0](AdResult.Unavailable);Step(app);Check(app.retry.interactable&&app.resultLobby.interactable&&!Get<bool>(app,"resolving"),"NoFill result progression");
                string expected=Get<StringTable>(app,"strings").Text(winner==1?Get<GameData>(app,"data").Stages[0].aiLoseKey:Get<GameData>(app,"data").Stages[0].aiWinKey);
                Check(app.aiSpeech.text==expected,"AI result dialogue inverted");app.UndoTurn();Call(app,"Finish");Check(State(Board(app))==state&&Progress(app).Data.completedMatches==4,"result Undo/re-Finish changed completion");
            }
        });
        Run("result.real_place_and_real_ai_terminal_paths",()=>
        {
            var player=Opening(new[]{0,6,1,6,2,5});player.SelectColumn(3);player.PlaceSelected();Check(Board(player).Winner==1&&Progress(player).Data.completedMatches==1,"actual player finish");
            Until(player,()=>player.result.activeSelf&&!Get<bool>(player,"resolving"),"player result");Check(player.nextStage.gameObject.activeSelf,"player Next missing");
            var ai=Opening(new[]{0,6,1,6,2,6});ai.SelectColumn(4);ai.PlaceSelected();Until(ai,()=>Board(ai).IsFinished,"actual AI immediate win");Check(Board(ai).Winner==-1&&Progress(ai).Data.completedMatches==1,"actual AI finish");
            Until(ai,()=>ai.result.activeSelf&&!Get<bool>(ai,"resolving"),"AI result");Check(!ai.nextStage.gameObject.activeSelf,"AI win showed Next");
            var draw=Opening(Draw.Take(41).ToArray());Until(draw,()=>Board(draw).IsFinished,"actual AI fills draw");Check(Board(draw).IsDraw&&draw.result.activeSelf&&!draw.winLine.gameObject.activeSelf,"actual draw flow");
        });
        Run("result.next_retry_lobby_stage_boundaries",()=>
        {
            foreach(int stage in new[]{1,9,10,29,30})foreach(int winner in new[]{1,-1,0})
            {
                var app=Terminal(winner,stage);Until(app,()=>app.result.activeSelf&&!Get<bool>(app,"resolving"),"boundary result");
                bool hasNext=winner==1&&stage<30;Check(app.nextStage.gameObject.activeSelf==hasNext&&app.retry.interactable&&app.resultLobby.interactable,"Next visibility "+stage+"/"+winner);
                int completed=Progress(app).Data.completedMatches;string id=Get<string>(app,"matchId");
                if(hasNext){Click(app.nextStage);Check(Get<int>(app,"activeStage")==stage,"Next entered incorrect stage");}
                else{Click(app.retry);Check(Get<int>(app,"activeStage")==stage-1,"Retry changed stage");}
                Check(Get<string>(app,"matchId")!=id&&Progress(app).Data.completedMatches==completed&&!app.result.activeSelf,"new match identity/result count");
                app.ShowLobby();Check(Get<int>(app,"selected")==Progress(app).Data.unlockedStage-1,"Lobby must select highest unlocked");
            }
        });
        Run("result.ad_results_duplicate_and_timeout",()=>
        {
            foreach(AdResult value in new[]{AdResult.Completed,AdResult.Cancelled,AdResult.Failed,AdResult.Unavailable})
            {
                var ads=new DelayedAds();var app=Terminal(1,1,ads);Until(app,()=>ads.Interstitials.Count==1,"ad request");
                ads.Interstitials[0](value);ads.Interstitials[0](AdResult.Completed);Step(app);
                Check(app.retry.interactable&&!Get<bool>(app,"resolving")&&Progress(app).Data.completedMatches==4,"ad blocks result or double counts");
                Check(Progress(app).Data.lastInterstitialMatch==(value==AdResult.Completed?4:0),"noncompletion/duplicate ad counter");
            }
            var delayed=new DelayedAds();var timed=Terminal(1,1,delayed);Until(timed,()=>delayed.Interstitials.Count==1,"timed ad request");Step(timed,85);
            Check(timed.retry.interactable&&!Get<bool>(timed,"resolving"),"ad timeout blocks progression");delayed.Interstitials[0](AdResult.Completed);Check(Progress(timed).Data.lastInterstitialMatch==0,"late timed-out ad updates counters");
        });
        Run("result.cancel_animation_observation_ad_and_pause",()=>
        {
            foreach(int frames in new[]{2,11,22})foreach(bool lobby in new[]{false,true})
            {
                var ads=new DelayedAds();var app=Terminal(1,1,ads);var stone=app.stones[Board(app).LastMove].rectTransform;Vector2 original=Get<Vector2>(app,"animatedPosition");int sibling=Get<int>(app,"animatedSibling");Step(app,frames);
                if(lobby)app.ShowLobby();else app.StartStage(0);string state=State(Board(app));foreach(var cb in ads.Interstitials)cb(AdResult.Completed);Step(app,100);
                Check(State(Board(app))==state&&!app.result.activeSelf&&!Get<bool>(app,"resolving")&&Progress(app).Data.lastInterstitialMatch==0,"stale result after transition");
                Check(stone.localScale.x==1&&stone.anchoredPosition.x==original.x&&stone.anchoredPosition.y==original.y&&stone.GetSiblingIndex()==sibling,"cancel did not restore animated stone");
            }
            var pausedAds=new DelayedAds();var paused=Terminal(1,1,pausedAds);Step(paused,2);Call(paused,"OnApplicationPause",true);float scale=paused.stones[Board(paused).LastMove].rectTransform.localScale.x;Step(paused,20);
            Check(pausedAds.Interstitials.Count==0&&!paused.result.activeSelf&&paused.stones[Board(paused).LastMove].rectTransform.localScale.x==scale,"paused animation advanced");Call(paused,"OnApplicationPause",false);
            Until(paused,()=>pausedAds.Interstitials.Count==1,"resume animation to ad");
        });
        Run("result.destroy_cancels_ad_and_stale_callback",()=>
        {
            var ads=new DelayedAds();var app=Terminal(1,1,ads);Until(app,()=>ads.Interstitials.Count==1,"destroy pending ad");int cancels=ads.CancelCalls;
            Call(app,"OnDestroy");ads.Interstitials[0](AdResult.Completed);
            Check(ads.CancelCalls>cancels&&Progress(app).Data.lastInterstitialMatch==0,"OnDestroy did not cancel pending interstitial or rejected no stale completion; cancelDelta="+(ads.CancelCalls-cancels)+", lastInterstitialMatch="+Progress(app).Data.lastInterstitialMatch);
        });
        Run("result.save_failure_still_shows_result",()=>
        {
            var app=Opening(new[]{0,6,1,6,2,5});PlayerPrefs.ThrowOnSave=true;app.SelectColumn(3);app.PlaceSelected();Until(app,()=>app.result.activeSelf&&!Get<bool>(app,"resolving"),"save-failed result");
            Check(app.retry.interactable&&app.resultAdStatus.text==Get<StringTable>(app,"strings").Text("client_error_save")&&Progress(app).Data.completedMatches==1,"save failure blocks result/warning");
        });
        Run("winning_line.crossing_both_sides_and_mirrors",()=>
        {
            using var doc=JsonDocument.Parse(File.ReadAllText(Path.Combine(Evidence,"AuditInputs","rank-ui-crossing-win-cases.json")));
            foreach(var row in doc.RootElement.GetProperty("cases").EnumerateArray())
            {
                int[] moves=row.GetProperty("columns").EnumerateArray().Select(x=>x.GetInt32()).ToArray();var board=Replay(moves,row.GetProperty("starter").GetInt32());
                Check(board.Winner==row.GetProperty("winner").GetInt32(),"crossing winner");var app=Opening();Set(app,"board",board);Call(app,"ShowWinLine");AssertLine(app);
                int[] cells=Enumerable.Range(0,42).Select(i=>board[i%7,i/7]).ToArray();int[] bad=row.GetProperty("invalidUnionFarthestPair").EnumerateArray().Select(x=>x.GetInt32()).ToArray();
                Check(!WinningSegmentOracle.IsValid(cells,bad[0],bad[1],board.LastMove,board.Winner),"oracle accepts known crossing error");
            }
        });
        Run("detail.cards_boundaries_scroll_blocker_lifecycle",()=>
        {
            var app=Make();var data=Get<GameData>(app,"data");var strings=Get<StringTable>(app,"strings");
            Check(app.cards.Length==30&&!app.multiplayer.interactable,"30 cards/coming-soon multiplayer");
            for(int i=0;i<30;i++){Check(app.cards[i].rank.text==strings.Text(data.Stages[i].rankKey)&&app.cards[i].stars.text.Length==data.Stages[i].difficultyStars,"card rank/star "+i);}
            app.stageScroll.verticalNormalizedPosition=.42f;app.OpenDetail(12);Check(Get<bool>(app,"detailSliding")&&!app.play.interactable&&!app.detailClose.interactable,"sliding input guard");
            app.OpenDetail(0);Check(Get<int>(app,"selected")==12,"background card changed selection");Until(app,()=>!Get<bool>(app,"detailSliding"),"detail opening");
            Check(!app.play.interactable&&app.playLabel.text==strings.Text("stage_locked"),"locked detail playable");string previousId=Get<string>(app,"matchId");app.play.onClick.Invoke();Check(Get<string>(app,"matchId")==previousId&&Board(app)==null,"locked start listener bypass");
            var opening=GameData.CreateBoard(data.Stages[12]);for(int i=0;i<42;i++)Check(app.detailStones[i].enabled==(opening[i%7,i/7]!=0),"detail opening stone "+i);
            app.CloseDetail();Until(app,()=>!Get<bool>(app,"detailSliding"),"detail closing");Check(!app.detail.gameObject.activeSelf&&app.stageScroll.verticalNormalizedPosition==.42f,"close/scroll preservation");
            app.OpenDetail(0);Until(app,()=>!Get<bool>(app,"detailSliding"),"first detail");Check(!app.previous.interactable&&app.following.interactable&&app.play.interactable,"first boundary");
            Call(app,"SelectDetail",-1);Check(Get<int>(app,"selected")==0,"selected before stage1");Call(app,"SelectDetail",50);Check(Get<int>(app,"selected")==29&&!app.following.interactable,"last boundary");
            var blocker=new RankDetailBlocker{blocker=new Image{enabled=false}};Call(blocker,"OnEnable");Check(blocker.blocker.enabled,"stationary blocker enable");Call(blocker,"OnDisable");Check(!blocker.blocker.enabled,"stationary blocker disable");
            // Does not simulate Unity overlay raycasts; underlying Settings/Help pointer blocking is native/browser scope.
        });
        Run("language.switch_preserves_match_preview_and_result",()=>
        {
            var app=Opening(new[]{3,2});app.SelectColumn(4);string state=State(Board(app)),id=Get<string>(app,"matchId");app.SetLanguage("en");
            Check(State(Board(app))==state&&Get<string>(app,"matchId")==id&&Get<int>(app,"selectedColumn")==4&&app.movePreview.gameObject.activeSelf,"language changed match/preview");
            Check(app.matchTitle.text=="Rank by Rank"&&Progress(app).Data.language=="en","English title/persistence");
            var terminal=Terminal(-1);Until(terminal,()=>terminal.result.activeSelf&&!Get<bool>(terminal,"resolving"),"localized result");int count=Progress(terminal).Data.completedMatches;terminal.SetLanguage("en");
            Check(terminal.aiSpeech.text==Get<StringTable>(terminal,"strings").Text(Get<GameData>(terminal,"data").Stages[0].aiWinKey)&&Progress(terminal).Data.completedMatches==count,"localized AI win/result duplicate");
        });
    }
}
