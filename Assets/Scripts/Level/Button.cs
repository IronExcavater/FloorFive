using Audio;
using Player;
using UnityEngine;
using Utils;

namespace Level
{
    [RequireComponent(typeof(AudioSource))]
    public class Button : Interactable
    {
        private AudioSource _audioSource;

        protected override void Awake()
        {
            base.Awake();
            _audioSource = GetComponent<AudioSource>();
        }
        
        protected override void Interact(PlayerController player)
        {
            _audioSource.PlayOneShot(AudioManager.AudioGroupDictionary.GetValue("buttonPress").GetRandomClip());
        }
    }
}