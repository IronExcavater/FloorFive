using System;
using UnityEngine;

namespace Subtitle
{
    [Serializable]
    public class SubtitleStep
    {
        public SubtitleEvent subtitleEvent;
        [Tooltip("If > 0, the step will auto-trigger after this delay from the previous step")]
        public float autoTriggerDelay = -1f;
        
        public string text;
        public AudioClip audioClip;
        public float duration = 5f;
    }
}