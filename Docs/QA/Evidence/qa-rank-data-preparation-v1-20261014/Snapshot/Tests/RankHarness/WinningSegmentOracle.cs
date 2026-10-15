using System;

namespace StageGameIndependentQa
{
    // Prepared independent expectation only; not bound to a product test runner yet.
    // Row0 is the bottom. A cell index is row*7+column.
    internal static class WinningSegmentOracle
    {
        internal static bool IsValid(int[] cells, int first, int last, int winningMove, int winner)
        {
            if (cells == null || cells.Length != 42 || (winner != 1 && winner != -1) ||
                first < 0 || first >= 42 || last < 0 || last >= 42 || winningMove < 0 || winningMove >= 42)
                return false;
            int dx = last % 7 - first % 7;
            int dy = last / 7 - first / 7;
            int length = Math.Max(Math.Abs(dx), Math.Abs(dy));
            if (length < 3 || (dx != 0 && dy != 0 && Math.Abs(dx) != Math.Abs(dy))) return false;
            int sx = Math.Sign(dx), sy = Math.Sign(dy);
            bool includesWinningMove = false;
            for (int offset = 0; offset <= length; offset++)
            {
                int cell = (first / 7 + offset * sy) * 7 + first % 7 + offset * sx;
                if (cells[cell] != winner) return false;
                if (cell == winningMove) includesWinningMove = true;
            }
            return includesWinningMove;
        }
    }
}
