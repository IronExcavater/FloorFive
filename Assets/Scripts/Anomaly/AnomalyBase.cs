using System.Collections;
using Animation;
using Audio;
using UnityEngine;
using Utils;

namespace Anomaly
{
    public class AnomalyBase : Movable
    {
        [Header("Anomaly")]
        public Vector3 anomalousPosition;
        public Vector3 anomalousRotation;
    
        private Vector3 _startPos;
        private Quaternion _startRot;
        
        private bool _active;
        public bool Active
        {
            get => _active;
            set
            {
                if (_active == value) return;
                _active = value;
                
                activeTime = 0;
                _rigidbody.isKinematic = !value;
                
                if (_active)
                {
                    transform.localPosition = anomalousPosition;
                    transform.localEulerAngles = anomalousRotation;
                    _audioSource.pitch = Random.Range(0.9f, 1.1f);
                    _audioSource.PlayOneShot(AudioManager.AudioGroupDictionary.GetValue("anomalyTrigger").GetRandomClip());
                    StartCoroutine(Alive());
                }
                else
                {
                    AnimationManager.CreateTween(this, position => transform.localPosition = position,
                        transform.localPosition, _startPos, 0.3f, Easing.EaseInOutCubic);
                    AnimationManager.CreateTween(this, rotation => transform.localRotation = rotation,
                        transform.localRotation, _startRot, 0.3f, Easing.EaseInOutCubic);
                }
            }
        }

        [HideInInspector] public float activeTime;

        protected override void Awake()
        {
            base.Awake();
            _rigidbody.isKinematic = !Active;
            
            Vector3 center = Utils.Utils.GetLocalBounds(gameObject).center;
            _startPos = transform.localPosition + center;
            _startRot = transform.localRotation;
        }
        
        private void Reset()
        {
            Vector3 center = Utils.Utils.GetLocalBounds(gameObject).center;
            anomalousPosition = transform.localPosition + center;
            anomalousRotation = transform.localEulerAngles;
        }

        private IEnumerator Alive()
        {
            while (Vector3.Distance(gameObject.transform.localPosition, _startPos) > 0.5f)
            {
                activeTime += Time.deltaTime;
                yield return null;
            }
            Active = false;
        }

        /*private bool SeenByPlayer()
        {
            var cam = Camera.main;
            if (!cam) return false;

            var toObject = target.transform.position - cam.transform.position;
            if (toObject.magnitude > seenDistance) return false;

            var dot = Vector3.Dot(cam.transform.forward, toObject.normalized);
            return dot > Mathf.Cos(seenAngle * Mathf.Deg2Rad);
        }*/
    }
}