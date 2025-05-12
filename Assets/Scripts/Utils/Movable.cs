using System;
using Audio;
using Level;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Utils
{
    public enum MovableType
    {
        Default,
        BouncyBall
    }
    
    [RequireComponent(typeof(Rigidbody))]
    public class Movable : MonoBehaviour
    {
        [Header("Impact")]
        [SerializeField] private float maxImpactVelocity;
        [SerializeField] private float minImpactVolume;
        [SerializeField] private float maxImpactVolume;
        
        public MovableType movableType = MovableType.Default;

        public event Action OnGrabbed;
        public event Action OnImpacted;
        
        protected Rigidbody _rigidbody;
        protected AudioSource _audioSource;

        protected virtual void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _audioSource = GetComponentInChildren<AudioSource>();

            if (_audioSource.gameObject != gameObject)
            {
                _audioSource.gameObject.transform.localPosition = Utils.GetLocalBounds(gameObject).center;
            }
            
            Utils.SetLayerRecursive(gameObject, LayerMask.NameToLayer("Movable"));
        }
        
        private void OnCollisionEnter(Collision collision)
        {
            float velocity = collision.relativeVelocity.magnitude;

            if (_rigidbody.linearVelocity.magnitude < 0.5f || velocity < 0.5f ||
                _audioSource == null) return; // Ignore tiny taps

            float t = Mathf.Clamp01(velocity / maxImpactVelocity);
            float volume = Mathf.Lerp(minImpactVolume, maxImpactVolume, t);

            string sfxKey = "carpetDrop";
            switch (movableType)
            {
                case MovableType.Default:
                    if (collision.gameObject.TryGetComponent<Surface>(out var surface)) 
                        sfxKey = surface.surfaceType.ToString().ToLower() + "Drop";
                    break;
                case MovableType.BouncyBall:
                    sfxKey = "ballBounce";
                    break;
            }
            
            AudioClip clip = AudioManager.AudioGroupDictionary.GetValue(sfxKey).GetRandomClip();
            _audioSource.pitch = Random.Range(0.9f, 1.1f);
            _audioSource.PlayOneShot(clip, volume);
            OnImpacted?.Invoke();
        }

        public void OnGrab()
        {
            OnGrabbed?.Invoke();
        }
        
        protected virtual void Grab() {}
    }
}