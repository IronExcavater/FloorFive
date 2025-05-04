using UnityEngine;
using UnityEngine.Audio;
using Utils;

namespace Audio
{
    [DoNotDestroySingleton]
    public class AudioManager : Singleton<AudioManager>
    {
        public AudioMixer audioMixer;

        [Header("Audio Clips:")]
        public AudioGroupDictionary audioGroupDictionary;

        public static AudioGroupDictionary AudioGroupDictionary => Instance.audioGroupDictionary;
    }

}