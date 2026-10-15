using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using StageGame;

// Independent bounded win/loss oracle: no production heuristic or rating is used to build the suite.
public static class RatingCurveAudit
{
    static readonly int[] Order={3,2,4,1,5,0,6};
    public sealed class Fixture { public string category,moves; public int horizon; public int[] scores; }
    public sealed class Metric { public string category; public int choices,correct; public double percent; }
    public sealed class Result { public Metric[] metrics; public int searches,maxNodes; public double meanNodes,maxMilliseconds; }
    static ConnectFourBoard Replay(string moves)
    {
        var board=new ConnectFourBoard();
        foreach(string move in moves.Split(','))if(move.Length>0&&!board.TryDrop(int.Parse(move)))throw new Exception("Invalid fixture");
        return board;
    }
    static int Solve(ConnectFourBoard board,int depth,int alpha,int beta)
    {
        if(board.Winner!=0)return -1;
        if(board.IsDraw||depth==0)return 0;
        int best=-1;
        foreach(int column in Order)
        {
            var next=board.Clone();if(!next.TryDrop(column))continue;
            int value=-Solve(next,depth-1,-beta,-alpha);
            best=Math.Max(best,value);alpha=Math.Max(alpha,best);
            if(alpha>=beta)break;
        }
        return best;
    }
    static int[] Scores(ConnectFourBoard board,int depth)
    {
        var scores=Enumerable.Repeat(-99,7).ToArray();
        foreach(int column in Order)
        {
            var next=board.Clone();if(next.TryDrop(column))scores[column]=-Solve(next,depth-1,-1,1);
        }
        return scores;
    }
    public static Fixture[] Generate(int perCategory, int seed=20261016)
    {
        var random=new Random(seed);var fixtures=new List<Fixture>();var seen=new HashSet<string>();
        string[] names={"attack_1","defense_2","attack_3","defense_4","attack_5","defense_6"};
        var counts=names.ToDictionary(x=>x,x=>0);
        for(int attempt=0;attempt<10000&&counts.Values.Any(x=>x<perCategory);attempt++)
        {
            var board=new ConnectFourBoard();var moves=new List<int>();
            int length=random.Next(3,25);
            for(int i=0;i<length;i++)
            {
                var legal=new List<int>();
                for(int c=0;c<7;c++){var next=board.Clone();if(next.TryDrop(c)&&!next.IsFinished)legal.Add(c);}
                if(legal.Count==0)break;
                int move=legal[random.Next(legal.Count)];board.TryDrop(move);moves.Add(move);
            }
            string key=string.Join("",Enumerable.Range(0,42).Select(i=>(char)('1'+board[i%7,i/7])));
            if(!seen.Add(key))continue;
            for(int horizon=1;horizon<=6;horizon++)
            {
                var scores=Scores(board,horizon);var legal=scores.Where(s=>s>-99).ToArray();
                int best=legal.Max(),worst=legal.Min();
                if(best<0)break;
                bool attack=horizon%2==1&&best==1;
                bool defense=horizon%2==0&&worst==-1;
                if(!attack&&!defense)continue;
                string name=names[horizon-1];
                if(counts[name]<perCategory&&best>worst)
                {
                    fixtures.Add(new Fixture{category=name,moves=string.Join(",",moves),horizon=horizon,scores=scores});counts[name]++;
                }
                break;
            }
        }
        if(counts.Values.Any(x=>x<perCategory))throw new Exception("Incomplete suite: "+string.Join(",",counts.Select(x=>x.Key+"="+x.Value)));
        return fixtures.ToArray();
    }
    public static Result Evaluate(Fixture[] fixtures,int depth,int budget,int randomPermille,int oversight,int seeds,int seedSalt=0)
    {
        var metrics=new Dictionary<string,Metric>();var result=new Result();long nodes=0;
        foreach(var fixture in fixtures)
        {
            var board=Replay(fixture.moves);int best=fixture.scores.Max();
            if(!metrics.ContainsKey(fixture.category))metrics[fixture.category]=new Metric{category=fixture.category};
            var metric=metrics[fixture.category];
            // With no stochastic branch, one actual search is sufficient for this deterministic board.
            int trials=randomPermille==0&&oversight==0?1:seeds;
            for(int seed=0;seed<trials;seed++)
            {
                var watch=Stopwatch.StartNew();
                int searchSeed=seedSalt==0?seed:seedSalt+seed*997+board.MoveCount*31;
                using(var search=new AiSearch(board,depth,budget,randomPermille,searchSeed,oversightPermille:oversight))
                {
                    while(!search.Complete){int before=search.NodesVisited;search.Step(16);if(search.NodesVisited-before>16)throw new Exception("Step quota exceeded");}
                    watch.Stop();if(!board.CanDrop(search.ChosenColumn)||search.NodesVisited>budget)throw new Exception("Invalid search result");
                    result.searches++;nodes+=search.NodesVisited;result.maxNodes=Math.Max(result.maxNodes,search.NodesVisited);
                    result.maxMilliseconds=Math.Max(result.maxMilliseconds,watch.Elapsed.TotalMilliseconds);
                    metric.choices++;if(fixture.scores[search.ChosenColumn]==best)metric.correct++;
                }
            }
        }
        foreach(var m in metrics.Values)m.percent=100.0*m.correct/m.choices;
        result.metrics=metrics.Values.OrderBy(m=>m.category).ToArray();result.meanNodes=(double)nodes/result.searches;return result;
    }
}
