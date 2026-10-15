using System;
using System.Diagnostics;
using StageGame;

// Fixed sparring partner, paired colors and openings; these scores are not human win rates or Elo.
public static class RatingMatchAudit
{
    static readonly string[] Openings = { "", "3", "2,4", "3,2,4", "1,4,2,3", "3,2,2,3,4,4", "0,6", "2,3,4,2" };
    public sealed class Result
    {
        public int games,wins,losses,draws,maxNodes,searches;
        public double scorePercent,meanPlies,maxMilliseconds;
    }
    public static Result Run(int depth,int budget,int randomPermille,int oversight,int pairsPerOpening)
    {
        var result=new Result();long totalPlies=0;
        for(int opening=0;opening<Openings.Length;opening++)
            for(int pair=0;pair<pairsPerOpening;pair++)
                foreach(int ratedColor in new[]{1,-1})
                {
                    var board=new ConnectFourBoard();
                    foreach(string move in Openings[opening].Split(','))
                        if(move.Length>0&&!board.TryDrop(int.Parse(move)))throw new Exception("Invalid opening");
                    while(!board.IsFinished)
                    {
                        bool rated=board.CurrentPlayer==ratedColor;
                        int seed=20261016+opening*7919+pair*997+board.MoveCount*31;
                        var watch=Stopwatch.StartNew();
                        using(var search=new AiSearch(board,rated?depth:4,rated?budget:10000,rated?randomPermille:250,seed,oversightPermille:rated?oversight:0))
                        {
                            while(!search.Complete)
                            {
                                int previous=search.NodesVisited;search.Step(16);
                                if(search.NodesVisited-previous>16)throw new Exception("Step quota exceeded");
                            }
                            watch.Stop();
                            if(search.NodesVisited>(rated?budget:10000))throw new Exception("Node budget exceeded");
                            if(!board.TryDrop(search.ChosenColumn))throw new Exception("Illegal AI move");
                            if(rated)
                            {
                                result.searches++;result.maxNodes=Math.Max(result.maxNodes,search.NodesVisited);
                                result.maxMilliseconds=Math.Max(result.maxMilliseconds,watch.Elapsed.TotalMilliseconds);
                            }
                        }
                    }
                    result.games++;totalPlies+=board.MoveCount;
                    if(board.Winner==ratedColor)result.wins++;
                    else if(board.IsDraw)result.draws++;
                    else result.losses++;
                }
        result.scorePercent=100.0*(result.wins+.5*result.draws)/result.games;
        result.meanPlies=(double)totalPlies/result.games;return result;
    }
}
