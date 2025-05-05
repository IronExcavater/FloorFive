using System.Collections;
using Animation;
using UnityEngine;

namespace Anomaly
{
    public class AnomalyBase : MonoBehaviour
    {
        public Vector3 anomalousPosition = Vector3.zero;
        public Vector3 anomalousRotation = Vector3.zero;
    
        private Vector3 _startPos;
        private Quaternion _startRot;
    
        private bool _active = true;
        public bool Active
        {
            get => _active;
            private set
            {
                if (_active == value) return;
                _active = value;
                
                if (_active)
                {
                    gameObject.transform.position = anomalousPosition;
                    gameObject.transform.eulerAngles = anomalousRotation;
                    StartCoroutine(Alive());
                }
                else
                {
                    gameObject.transform.position = _startPos;
                    gameObject.transform.rotation = _startRot;
                }
            }
        }

        private void Awake()
        {
            _startPos = gameObject.transform.position;
            _startRot = gameObject.transform.rotation;
        }

        private IEnumerator Alive()
        {
            yield return new WaitWhile(() => Vector3.Distance(gameObject.transform.position, _startPos) > 0.3f);
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