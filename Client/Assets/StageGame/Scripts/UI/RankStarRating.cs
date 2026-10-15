using UnityEngine;
using UnityEngine.UI;

namespace StageGame
{
    // Eight saved sprite pairs. A half star uses the left half of the foreground sprite.
    public sealed class RankStarRating : MonoBehaviour
    {
        public Image[] filledStars;
        public float Value { get; private set; }
        public void SetValue(float value)
        {
            Value=value;
            for(int i=0;i<filledStars.Length;i++)filledStars[i].fillAmount=Mathf.Clamp01(value-i);
        }
    }
}
