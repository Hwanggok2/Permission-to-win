using System;
using System.Diagnostics;
using StageGame;

// Enumerates every legal player choice through their fourth move for fixed match seeds.
public static class ShortGameAudit
{
    public sealed class Report
    {
        public int starter, seeds, playerBranches, aiMoves, earlyPlayerWins, aiWins, maxNodes;
        public string firstLoss;
        public double maxSearchMilliseconds;
    }
    public static Report Run(int starter,int depth,int budget,int randomPermille,int oversight,int seeds)
    {
        var report=new Report{starter=starter,seeds=seeds};
        for(int seed=0;seed<seeds;seed++) Visit(new ConnectFourBoard(starter),seed,depth,budget,randomPermille,oversight,"",report);
        return report;
    }
    static void Visit(ConnectFourBoard board,int seed,int depth,int budget,int randomPermille,int oversight,string moves,Report report)
    {
        if(board.IsFinished)
        {
            if(board.Winner==1){report.earlyPlayerWins++;if(report.firstLoss==null)report.firstLoss="seed="+seed+": "+moves;}
            if(board.Winner==-1)report.aiWins++;
            return;
        }
        if(board.MoveCount>=(report.starter==1?7:8))return;
        if(board.CurrentPlayer==1)
        {
            for(int column=0;column<7;column++)
            {
                var next=board.Clone();if(!next.TryDrop(column))continue;
                report.playerBranches++;Visit(next,seed,depth,budget,randomPermille,oversight,moves+column+",",report);
            }
        }
        else
        {
            var watch=Stopwatch.StartNew();
            using(var search=new AiSearch(board,depth,budget,randomPermille,seed*997+board.MoveCount*31,oversightPermille:oversight))
            {
                while(!search.Complete)
                {
                    int before=search.NodesVisited;search.Step(16);
                    if(search.NodesVisited-before>16)throw new Exception("Step quota exceeded");
                }
                watch.Stop();report.maxSearchMilliseconds=Math.Max(report.maxSearchMilliseconds,watch.Elapsed.TotalMilliseconds);
                report.maxNodes=Math.Max(report.maxNodes,search.NodesVisited);if(search.NodesVisited>budget)throw new Exception("Node budget exceeded");
                report.aiMoves++;var next=board.Clone();if(!next.TryDrop(search.ChosenColumn))throw new Exception("Illegal AI reply");
                Visit(next,seed,depth,budget,randomPermille,oversight,moves+search.ChosenColumn+",",report);
            }
        }
    }
}
