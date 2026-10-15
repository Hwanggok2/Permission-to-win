using UnityEngine;

namespace StageGame
{
    public sealed class RankTheme : ScriptableObject
    {
        public Sprite panel, primary, currentCard, lockedCard;
        public Sprite blackStone, whiteStone, blackWin, whiteWin, preview;
        public Sprite aiAvatar, playerAvatar;
        public Sprite close, pause, help, settings, left, right, check, locked, star, undo, retry;
    }
}
