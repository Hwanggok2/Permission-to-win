using UnityEngine;

namespace StageGame
{
    // Authored sprites are bound by the saved AppRoot prefab; no runtime UI construction.
    public sealed class TabletopTheme : ScriptableObject
    {
        public Sprite panel, primary, secondary, board, hole;
        public Sprite player, opponent, playerWin, opponentWin;
        public Sprite nodeCurrent, nodeCleared, nodeLocked, nodeSelected;
        public Sprite pause, help, settings, chevron, locked, check, play, close;
    }
}
