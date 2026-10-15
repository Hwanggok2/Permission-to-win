using System;
using System.Collections.Generic;
using StageGame;

public static class OpeningAudit
{
    public sealed class Report { public int starter, playerBranches, aiMoves, earlyPlayerWins, aiWins, maxNodes; public string opening, firstLoss; }
    public static Report Run(int starter, string opening = "")
    {
        var report=new Report{starter=starter,opening=opening};
        var board=new ConnectFourBoard(starter);
        foreach(var token in opening.Split(new[]{','},StringSplitOptions.RemoveEmptyEntries))
            if(!board.TryDrop(int.Parse(token)))throw new Exception("Invalid opening: "+opening);
        Visit(board,0,"",report);
        return report;
    }
    static void Visit(ConnectFourBoard board,int aiMoves,string line,Report report)
    {
        if(board.IsFinished)
        {
            if(board.Winner==1){report.earlyPlayerWins++;if(report.firstLoss==null)report.firstLoss=line;}
            if(board.Winner==-1)report.aiWins++;
            return;
        }
        if(board.CurrentPlayer==-1)
        {
            if(aiMoves>=5)return;
            using(var search=new AiSearch(board,1,500,850,board.MoveCount*31,true))
            {
                while(!search.Complete)search.Step(256);
                report.maxNodes=Math.Max(report.maxNodes,search.NodesVisited);
                report.aiMoves++;
                var next=board.Clone();next.TryDrop(search.ChosenColumn);
                Visit(next,aiMoves+1,line+search.ChosenColumn+",",report);
            }
        }
        else
        {
            // Check every legal player continuation through the fifth AI reply.
            if(aiMoves>=5)return;
            for(int col=0;col<7;col++)if(board.CanDrop(col))
            {
                report.playerBranches++;
                var next=board.Clone();next.TryDrop(col);
                Visit(next,aiMoves,line+col+",",report);
            }
        }
    }
}
