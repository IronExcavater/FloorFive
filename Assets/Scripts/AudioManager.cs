using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Audio { get; private set; }
    
    public AudioMixer audioMixer;
    
    [Header("SFX Clips:")]
    public AudioClip[] step;
    
    private void Awake()
    {
        if (Audio == null)
        {
            Audio = this;
            DontDestroyOnLoad(Audio);
        }
        else Destroy(gameObject);
    }

    public static AudioClip GetRandomClip(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return null;
        return clips[Random.Range(0, clips.Length)];
    }
}
