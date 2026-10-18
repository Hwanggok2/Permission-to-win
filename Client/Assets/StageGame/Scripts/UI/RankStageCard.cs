using UnityEngine;
using UnityEngine.UI;

namespace StageGame
{
    public sealed class RankStageCard : MonoBehaviour
    {
        public Button button;
        public Text rank, number, stars, rating;
        public Image state;
        public Image header, accent;
        public Sprite paper, currentPaper, lockedPaper;
        public Sprite lockedMark, clearedMark, currentMark;
        public void SetAppearance(bool locked, bool current, bool cleared=false)
        {
            button.GetComponent<Image>().sprite=current?currentPaper:locked?lockedPaper:paper;
            header.color=current?new Color32(102,76,37,255):locked?new Color32(74,81,61,255):new Color32(48,67,45,255);
            accent.color=current?new Color32(194,151,63,255):new Color32(130,140,106,255);
            state.sprite=locked?lockedMark:cleared?clearedMark:currentMark;state.gameObject.SetActive(true);
        }
    }
}
