using System.Collections.Generic;
using UnityEngine;

namespace Subtitle
{
    [CreateAssetMenu(fileName = "SubtitleSequence", menuName = "Subtitles/Subtitle Sequence")]
    public class SubtitleSequence : ScriptableObject
    {
        public List<SubtitleEntry> entries;
    }
}