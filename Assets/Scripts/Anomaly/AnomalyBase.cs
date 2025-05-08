using System.Collections;
using Animation;
using UnityEngine;

namespace Anomaly
{
    [RequireComponent(typeof(Rigidbody))]
    public class AnomalyBase : MonoBehaviour
    {
        public Vector3 anomalousPosition;
        public Vector3 anomalousRotation;
    
        private Vector3 _startPos;
        private Quaternion _startRot;
    
        private Rigidbody _rigidbody;
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
                    gameObject.transform.localPosition = anomalousPosition;
                    gameObject.transform.localEulerAngles = anomalousRotation;
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

        public float activeTime;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.isKinematic = !Active;
            
            Vector3 center = Utils.Utils.GetLocalBounds(gameObject).center;
            _startPos = center;
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