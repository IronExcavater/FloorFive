using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Audio
{
    [Serializable]
    public class AudioGroup
    {
        public AudioClip[] clips;
        
        public AudioClip GetRandomClip()
        {
            if (clips == null || clips.Length == 0) return null;
            return clips[Random.Range(0, clips.Length)];
        }

        public AudioClip GetFirstClip()
        {
            if (clips == null || clips.Length == 0) return null;
            return clips[0];
        }
    }
}