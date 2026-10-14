using System;
using System.Collections.Generic;

namespace StageGame
{
    // Iterative deepening negamax. Step() yields at every explored node, including on WebGL.
    public sealed class AiSearch : IDisposable
    {
        static readonly int[] Order = { 3, 2, 4, 1, 5, 0, 6 };
        const int Win = 100000;
        readonly ConnectFourBoard root;
        readonly int maxDepth, budget, randomPermille;
        readonly Random random;
        readonly IEnumerator<int> iterator;
        bool exhausted;
        public int ChosenColumn { get; private set; } = -1;
        public int NodesVisited { get; private set; }
        public int CompletedDepth { get; private set; }
        public bool Complete { get; private set; }

        public AiSearch(ConnectFourBoard board, int depth, int nodeBudget, int mistakePermille, int seed)
        {
            if (depth < 1 || depth > 6 || nodeBudget < 1 || mistakePermille < 0 || mistakePermille > 1000)
                throw new ArgumentOutOfRangeException(nameof(depth));
            root = board.Clone();
            maxDepth = depth;
            budget = nodeBudget;
            randomPermille = mistakePermille;
            random = new Random(seed);
            iterator = Search().GetEnumerator();
        }

        public void Step(int nodeQuota = 128)
        {
            if (nodeQuota < 1) throw new ArgumentOutOfRangeException(nameof(nodeQuota));
            for (int i = 0; i < nodeQuota && !Complete; i++)
                if (!iterator.MoveNext()) Complete = true;
        }

        public void Dispose() { iterator.Dispose(); Complete = true; }

        IEnumerable<int> Search()
        {
            if (root.IsFinished) yield break;
            var legal = new List<int>();
            var safe = new List<int>();
            foreach (int column in Order)
            {
                if (!root.CanDrop(column)) continue;
                legal.Add(column);
                var after = root.Clone();
                after.TryDrop(column);
                if (after.Winner == root.CurrentPlayer) { ChosenColumn = column; yield break; }
                bool opponentWins = false;
                foreach (int reply in Order)
                {
                    if (!after.CanDrop(reply)) continue;
                    var response = after.Clone();
                    response.TryDrop(reply);
                    if (response.Winner == -root.CurrentPlayer) { opponentWins = true; break; }
                }
                if (!opponentWins) safe.Add(column);
            }
            var candidates = safe.Count > 0 ? safe : legal;
            if (candidates.Count == 0) yield break;
            ChosenColumn = candidates[0];
            // Random mistakes cannot ignore an immediate win or an avoidable immediate loss.
            if (random.Next(1000) < randomPermille)
            {
                ChosenColumn = candidates[random.Next(candidates.Count)];
                yield break;
            }
            for (int depth = 1; depth <= maxDepth; depth++)
            {
                int best = -Win * 2, bestMove = ChosenColumn;
                foreach (int column in candidates)
                {
                    var next = root.Clone();
                    next.TryDrop(column);
                    var result = new Score();
                    foreach (int tick in Negamax(next, depth - 1, -Win * 2, Win * 2, result)) yield return tick;
                    if (exhausted) yield break;
                    int score = -result.Value;
                    if (score > best) { best = score; bestMove = column; }
                }
                ChosenColumn = bestMove;
                CompletedDepth = depth;
            }
        }

        sealed class Score { public int Value; }
        IEnumerable<int> Negamax(ConnectFourBoard board, int depth, int alpha, int beta, Score result)
        {
            if (NodesVisited >= budget) { exhausted = true; yield break; }
            NodesVisited++;
            yield return 0;
            if (board.Winner != 0) { result.Value = -Win - depth; yield break; }
            if (board.IsDraw) { result.Value = 0; yield break; }
            if (depth == 0) { result.Value = Evaluate(board); yield break; }
            int best = -Win * 2;
            foreach (int column in Order)
            {
                if (!board.CanDrop(column)) continue;
                var child = board.Clone();
                child.TryDrop(column);
                var score = new Score();
                foreach (int tick in Negamax(child, depth - 1, -beta, -alpha, score)) yield return tick;
                if (exhausted) yield break;
                best = Math.Max(best, -score.Value);
                alpha = Math.Max(alpha, best);
                if (alpha >= beta) break;
            }
            result.Value = best;
        }

        static int Evaluate(ConnectFourBoard board)
        {
            int value = 0;
            for (int row = 0; row < ConnectFourBoard.Height; row++) value += board[3, row] * board.CurrentPlayer * 5;
            for (int y = 0; y < ConnectFourBoard.Height; y++)
                for (int x = 0; x < ConnectFourBoard.Width; x++)
                {
                    if (x <= 3) value += Window(board, x, y, 1, 0);
                    if (y <= 2) value += Window(board, x, y, 0, 1);
                    if (x <= 3 && y <= 2) value += Window(board, x, y, 1, 1);
                    if (x <= 3 && y >= 3) value += Window(board, x, y, 1, -1);
                }
            return value;
        }

        static int Window(ConnectFourBoard board, int x, int y, int dx, int dy)
        {
            int own = 0, enemy = 0;
            for (int n = 0; n < 4; n++)
            {
                int cell = board[x + dx * n, y + dy * n] * board.CurrentPlayer;
                if (cell == 1) own++;
                if (cell == -1) enemy++;
            }
            if (own > 0 && enemy > 0) return 0;
            int count = Math.Max(own, enemy);
            int weight = count == 3 ? 60 : count == 2 ? 8 : count;
            return enemy > 0 ? -weight : weight;
        }
    }
}
