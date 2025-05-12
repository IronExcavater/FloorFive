using UnityEngine;

namespace UI
{
    [CreateAssetMenu(fileName = "SubtitleSequence", menuName = "Subtitles/Subtitle Sequence")]
    public class SubtitleSequence : ScriptableObject
    {
        public SubtitleStep[] Steps;
    }
}