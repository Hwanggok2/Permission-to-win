using UnityEngine;

namespace StageGame
{
    [System.Serializable]
    public sealed class CharacterPortrait
    {
        public string characterId;
        public Sprite intro, thinking, defeat;
    }

    public sealed class RankTheme : ScriptableObject
    {
        public Sprite panel, primary, currentCard, lockedCard;
        public Sprite blackStone, whiteStone, blackWin, whiteWin, preview;
        public Sprite aiAvatar, playerAvatar;
        public Sprite close, pause, help, settings, left, right, check, locked, star, undo, retry;
        public Sprite roundPrimary;
        public Sprite[] rankHats;
        public Sprite barracks, woodenBoard;
        public CharacterPortrait[] portraits;
        public CharacterPortrait finalRecruit;

        public CharacterPortrait Portrait(string characterId, bool tournament = false)
        {
            if (tournament && characterId == "C01" && finalRecruit != null && finalRecruit.intro != null)
                return finalRecruit;
            if (portraits != null)
                foreach (var portrait in portraits)
                    if (portrait.characterId == characterId) return portrait;
            return null;
        }
    }
}
