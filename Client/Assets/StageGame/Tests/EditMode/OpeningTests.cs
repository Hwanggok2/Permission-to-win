using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace StageGame.Tests
{
    public sealed class OpeningSearchTests
    {
        [TestCase(1)] [TestCase(-1)]
        public void GuardedOpeningUsesCenterWithoutRandomMistakesAndKeepsStepBudget(int starter)
        {
            var board=new ConnectFourBoard(starter);
            for(int seed=0;seed<8;seed++)
                using(var search=new AiSearch(board,1,500,1000,seed,true))
                {
                    while(!search.Complete)
                    {
                        int before=search.NodesVisited;search.Step(8);
                        Assert.LessOrEqual(search.NodesVisited-before,8);
                    }
                    Assert.AreEqual(3,search.ChosenColumn);
                    Assert.GreaterOrEqual(search.CompletedDepth,4);
                    Assert.LessOrEqual(search.NodesVisited,3500);
                    Assert.AreEqual(0,board.MoveCount);
                }
        }
        [Test] public void GuardedOpeningAvoidsTheObservedEarlyDoubleThreat()
        {
            var board=new ConnectFourBoard();foreach(int col in new[]{2,2,3})board.TryDrop(col);
            using(var search=new AiSearch(board,1,500,850,93,true))
            {
                while(!search.Complete)search.Step();
                board.TryDrop(search.ChosenColumn);
                // The old shallow reply at column3 let player column1 create two wins.
                for(int col=0;col<7;col++)
                {
                    var next=board.Clone();if(!next.TryDrop(col))continue;
                    int threats=0;
                    for(int win=0;win<7;win++)
                    {
                        // Evaluate player threats after allowing AI any legal defense.
                        var reply=next.Clone();if(!reply.TryDrop(win))continue;
                        bool humanCanWin=false;
                        for(int c=0;c<7;c++){var last=reply.Clone();if(last.TryDrop(c)&&last.Winner==1)humanCanWin=true;}
                        if(humanCanWin)threats++;
                    }
                    Assert.Less(threats,7,"Opening reply must leave a defense against the observed fork.");
                }
            }
        }
    }
    public sealed partial class RankFlowTests
    {
        AiSearch MatchSearch(bool hint)=>(AiSearch)typeof(RankGameApp).GetMethod("CreateSearch",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(app,new object[]{hint});
        int SearchDepth(bool hint=false)
        {
            using(var search=MatchSearch(hint)){while(!search.Complete)search.Step();return search.CompletedDepth;}
        }
        [UnityTest] public IEnumerator RatedMatchesUseTheirAuthoredStrengthFromTheFirstMove()
        {
            Progress.Data.unlockedStage=30;
            foreach(int stage in new[]{0,1,4,6,12,24,29})
            {
                app.StartStage(stage);Call("CancelSearch");
                var original=Board.Clone();Assert.AreEqual(original.MoveCount,Field<int>("openingMoveCount"));
                // A nonterminal fixture isolates the turn boundary for both starter colors.
                var fixture=new ConnectFourBoard(Data.Stages[stage].starter);
                var moves=new[]{0,1,0,1,2,3,2,3,4,5,4,5,6,6};
                for(int i=0;i<original.MoveCount+8;i++)Assert.IsTrue(fixture.TryDrop(moves[i]));
                Set("board",fixture);
                Assert.LessOrEqual(SearchDepth(),Data.Stages[stage].aiDepth,"No hidden opening strength override.");
                using(var hint=MatchSearch(true))
                {
                    while(!hint.Complete)hint.Step();
                    Assert.Greater(hint.NodesVisited,0,"Hints must not use the opponent's random oversight.");
                }
                Board.TryDrop(moves[original.MoveCount+8]);Board.TryDrop(moves[original.MoveCount+9]);
                Assert.LessOrEqual(SearchDepth(),Data.Stages[stage].aiDepth);
                app.StartStage(stage);Call("CancelSearch");Assert.LessOrEqual(SearchDepth(),Data.Stages[stage].aiDepth,"Retry keeps the authored rating.");
            }
            yield return null;
        }
        [UnityTest] public IEnumerator ActualAiStarterPlacesOnceAndUndoRestoresOpeningPhase()
        {
            Progress.Data.unlockedStage=30;app.StartStage(4);
            Assert.AreEqual(-1,Board.CurrentPlayer);Assert.IsTrue(Field<bool>("busy"));Assert.AreEqual(0,Board.MoveCount);
            yield return AiReady();Assert.AreEqual(1,Board.MoveCount);Assert.AreEqual(1,Board.CurrentPlayer);Assert.AreEqual(-1,Board[Board.LastMove%7,0]);
            var before=Board.Clone();app.SelectColumn(2);app.PlaceSelected();yield return AiReady();
            Assert.AreEqual(3,Board.MoveCount);app.UndoTurn();SameBoard(before,Board);
            Assert.LessOrEqual(SearchDepth(),Data.Stages[4].aiDepth);
        }
        [UnityTest] public IEnumerator HighestRatingFindsFivePlyAttackWithinTheRealTurnDeadline()
        {
            Progress.Data.unlockedStage=30;app.StartStage(29);
            foreach(var moves in new[]{new[]{3,4,1,0,0,5,4,6,2,1,1,3,4,6},new[]{4,5,5,3,1,0,0,0,2,5,0,2,3,6}})
            {
                Inject(Replay(-1,moves));Call("BeginThink",false);yield return AiReady();
                Assert.AreEqual(moves.Length+1,Board.MoveCount);
                Assert.AreEqual(4,Board.LastMove%7,"The live time-sliced search must find the forced attack, not stop at a shallow fallback.");
                Assert.AreEqual(1,Board.CurrentPlayer);
            }
        }
    }
}
