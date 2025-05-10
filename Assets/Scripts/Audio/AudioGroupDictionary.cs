using System;
using UnityEngine;
using Utils;

namespace Audio
{
    [CreateAssetMenu(menuName = "Audio/AudioGroup Dictionary")]
    public class AudioGroupDictionary : SerializedDictionary<AudioGroupPair, string, AudioGroup>
    {
        public override AudioGroup GetValue(string key) => Dictionary[key];
    }

    [Serializable]
    public class AudioGroupPair : KeyValuePair<string, AudioGroup> {}
}