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

        private Vector3 _localCenter;
        
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

        protected virtual void Activate(bool active)
        {
            activeTime = 0;
            _rigidbody.isKinematic = !active;
            AnimationManager.RemoveTweens(this);
                
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
                yield return null;
            }
            Active = false;
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
    }
}