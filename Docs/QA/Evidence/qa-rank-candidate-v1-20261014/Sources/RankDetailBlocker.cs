using UnityEngine;
using UnityEngine.UI;

namespace StageGame
{
    // Stationary serialized hit area, active even while the detail child slides off its edge.
    public sealed class RankDetailBlocker : MonoBehaviour
    {
        public Image blocker;
        void OnEnable() { if (blocker != null) blocker.enabled = true; }
        void OnDisable() { if (blocker != null) blocker.enabled = false; }
    }
}
