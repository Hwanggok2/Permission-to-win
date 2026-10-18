using System;
using System.Collections.Generic;

namespace StageGame
{
    // Rows are indexed from the bottom. The model never depends on rendering or Unity.
    public sealed class ConnectFourBoard
    {
        public const int Width = 7;
        public const int Height = 6;
        readonly int[] cells = new int[Width * Height];
        readonly int[] heights = new int[Width];
        readonly List<int> winningCells = new List<int>();
        readonly int[] lastMoves = { -1, -1 }, lastMoveNumbers = { 0, 0 };
        public int LastMoveFor(int player) => lastMoves[player == 1 ? 0 : 1];
        public int LastMoveNumberFor(int player) => lastMoveNumbers[player == 1 ? 0 : 1];
        public int CurrentPlayer { get; private set; }
        public int Winner { get; private set; }
        public int MoveCount { get; private set; }
        public int LastMove { get; private set; } = -1;
        public bool IsDraw => Winner == 0 && MoveCount == Width * Height;
        public bool IsFinished => Winner != 0 || IsDraw;
        public IReadOnlyList<int> WinningCells => winningCells;

        public ConnectFourBoard(int starter = 1)
        {
            if (starter != 1 && starter != -1) throw new ArgumentOutOfRangeException(nameof(starter));
            CurrentPlayer = starter;
        }

        public int this[int column, int row] => cells[row * Width + column];
        public bool CanDrop(int column) => !IsFinished && column >= 0 && column < Width && heights[column] < Height;
        public int LandingRow(int column) => CanDrop(column) ? heights[column] : -1;

        public bool TryDrop(int column)
        {
            if (!CanDrop(column)) return false;
            int row = heights[column]++;
            int player = CurrentPlayer;
            cells[row * Width + column] = player;
            LastMove = row * Width + column;
            MoveCount++;
            lastMoves[player == 1 ? 0 : 1] = LastMove;
            lastMoveNumbers[player == 1 ? 0 : 1] = MoveCount;
            CheckLine(column, row, 1, 0, player);
            CheckLine(column, row, 0, 1, player);
            CheckLine(column, row, 1, 1, player);
            CheckLine(column, row, 1, -1, player);
            CurrentPlayer = -player;
            return true;
        }

        void CheckLine(int x, int y, int dx, int dy, int player)
        {
            int count = 1;
            for (int sign = -1; sign <= 1; sign += 2)
                for (int n = 1; n < Math.Max(Width, Height); n++)
                {
                    int cx = x + dx * n * sign, cy = y + dy * n * sign;
                    if (cx < 0 || cx >= Width || cy < 0 || cy >= Height || this[cx, cy] != player) break;
                    count++;
                }
            if (count < 4) return;
            Winner = player;
            if (!winningCells.Contains(y * Width + x)) winningCells.Add(y * Width + x);
            for (int sign = -1; sign <= 1; sign += 2)
                for (int n = 1; n < Math.Max(Width, Height); n++)
                {
                    int cx = x + dx * n * sign, cy = y + dy * n * sign;
                    if (cx < 0 || cx >= Width || cy < 0 || cy >= Height || this[cx, cy] != player) break;
                    int cell = cy * Width + cx;
                    if (!winningCells.Contains(cell)) winningCells.Add(cell);
                }
        }

        public ConnectFourBoard Clone()
        {
            var copy = new ConnectFourBoard(CurrentPlayer);
            Array.Copy(cells, copy.cells, cells.Length);
            Array.Copy(heights, copy.heights, heights.Length);
            copy.Winner = Winner;
            copy.MoveCount = MoveCount;
            copy.LastMove = LastMove;
            Array.Copy(lastMoves, copy.lastMoves, 2);
            Array.Copy(lastMoveNumbers, copy.lastMoveNumbers, 2);
            copy.winningCells.AddRange(winningCells);
            return copy;
        }
    }
}
