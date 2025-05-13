using System.Collections;
using Animation;
using Audio;
using Level;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

namespace Anomaly
{
    public class AnomalyBase : Movable
    {
        [Header("Anomaly")]
        public Vector3 anomalousPosition;
        public Vector3 anomalousRotation;
    
        public Vector3 _startPos;
        public Quaternion _startRot;

        private Vector3 _localCenter;
        
        protected Room _room;
        
        private bool _active;
        public bool Active
        {
            get => _active;
            set
            {
                if (_active == value) return;
                _active = value;
                
                Activate(_active);
            }
        }

        [HideInInspector] public float activeTime;

        private AudioSource _humAudioSource;
        private bool _isFading;
        [SerializeField] private float minHumVolume = 0.1f;
        [SerializeField] private float maxHumVolume = 0.5f;
        [SerializeField] private float maxHumActiveTime = 10f;

        protected virtual void Activate(bool active)
        {
            activeTime = 0;
            _rigidbody.isKinematic = !active;
            AnimationManager.RemoveTweens(this);
                
            if (_active)
            {
                transform.localPosition = anomalousPosition - _localCenter;
                transform.localEulerAngles = anomalousRotation;
                _audioSource.pitch = Random.Range(0.9f, 1.1f);
                _audioSource.PlayOneShot(AudioManager.AudioGroupDictionary.GetValue("anomalyTrigger").GetRandomClip());
                StartCoroutine(Alive());
                StartCoroutine(FadeHum(1, 1));
            }
            else
            {
                StartCoroutine(FadeHum(0, 1));
                
                AnimationManager.CreateTween(this, position => transform.localPosition = position,
                    transform.localPosition, _startPos - _localCenter, 0.3f, Easing.EaseInOutCubic);
                AnimationManager.CreateTween(this, rotation => transform.localRotation = rotation,
                    transform.localRotation, _startRot, 0.3f, Easing.EaseInOutCubic);
            }
        }

        protected override void Awake()
        {
            base.Awake();
            _rigidbody.isKinematic = !Active;
            
            _localCenter = Utils.Utils.GetLocalBounds(gameObject).center;
            _startPos = transform.localPosition + _localCenter;
            _startRot = transform.localRotation;
        }

        private void Start()
        {
            _room = GameObject.FindGameObjectWithTag("Room").GetComponent<Room>();
            _humAudioSource = gameObject.AddComponent<AudioSource>();
            _humAudioSource.clip = AudioManager.AudioGroupDictionary.GetValue("energyHum").GetFirstClip();
            _humAudioSource.loop = true;
            _humAudioSource.playOnAwake = false;
            _humAudioSource.volume = minHumVolume;
            _humAudioSource.spatialBlend = 1;
        }

        private void Reset()
        {
            Vector3 center = Utils.Utils.GetLocalBounds(gameObject).center;
            anomalousPosition = transform.localPosition + center;
            anomalousRotation = transform.localEulerAngles;
        }

        private IEnumerator Alive()
        {
            while (Vector3.Distance(gameObject.transform.localPosition + _localCenter, _startPos) > 0.5f)
            {
                activeTime += Time.deltaTime;
                
                if (!_isFading)
                {
                    _humAudioSource.volume = Mathf.MoveTowards(
                        _humAudioSource.volume,
                        Mathf.Lerp(minHumVolume, maxHumVolume, activeTime / maxHumActiveTime),
                        Time.deltaTime);
                }
                yield return null;
            }
            Active = false;
            _room.AnomalyCompleted(this);
        }

        public Bounds GetNormalBounds()
        {
            Bounds bounds = Utils.Utils.GetLocalBounds(gameObject);
            bounds.center = transform.position + bounds.center;
            return bounds;
        }

        public Bounds GetAnomalousBounds()
        {
            Bounds bounds = Utils.Utils.GetLocalBounds(gameObject);
            bounds.center = anomalousPosition;
            return bounds;
        }

        private IEnumerator FadeHum(float targetVolume, float duration)
        {
            if (!_humAudioSource.isPlaying) _humAudioSource.Play();
            _isFading = true;
            
            AnimationManager.RemoveTweens(_humAudioSource);
            var fade = AnimationManager.CreateTween(_humAudioSource, volume => _humAudioSource.volume = volume,
                _humAudioSource.volume, targetVolume, duration, Easing.EaseInOutCubic);
            
            yield return new WaitUntil(() => !AnimationManager.HasTween(fade));
            
            _isFading = false;
            if (targetVolume <= 0) _humAudioSource.Stop();
        }
    }
}