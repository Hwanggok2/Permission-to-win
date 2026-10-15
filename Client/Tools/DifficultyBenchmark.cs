using System;
using StageGame;

// Compares authored data through the production AI. This is a bot proxy, not human win-rate evidence.
public static class DifficultyBenchmark
{
    public sealed class Result
    {
        public int games, playerWins, aiWins, draws;
    }

    public static Result Run(int starter,string opening,int depth,int budget,int randomPermille,int games,bool protectOpening=false,int oversightPermille=0)
    {
        var result=new Result{games=games};
        for(int seed=0;seed<games;seed++)
        {
            var board=new ConnectFourBoard(starter);
            foreach(string move in opening.Split(','))
                if(move.Length>0&&!board.TryDrop(int.Parse(move)))throw new Exception("Invalid opening");
            int openingCount=board.MoveCount;
            while(!board.IsFinished)
            {
                // Proxy player only recognizes immediate wins/losses; otherwise chooses a safe move at random.
                bool player=board.CurrentPlayer==1;
                using(var search=new AiSearch(board,player?1:depth,player?100:budget,player?1000:randomPermille,seed*997+board.MoveCount*31,
                    protectOpening&&!player&&(board.MoveCount-openingCount)/2<5,player?0:oversightPermille))
                {
                    while(!search.Complete)search.Step(256);
                    if(!board.TryDrop(search.ChosenColumn))throw new Exception("Illegal AI move");
                }
            }
            if(board.Winner==1)result.playerWins++;
            else if(board.Winner==-1)result.aiWins++;
            else result.draws++;
        }
        return result;
    }
}
