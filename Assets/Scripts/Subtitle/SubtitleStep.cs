using System;
using UnityEngine;

namespace Subtitle
{
    [Serializable]
    public class SubtitleStep
    {
        public SubtitleEvent subtitleEvent;
        public string text;
        public AudioClip audioClip;
        public float duration = 5f;
    }
}