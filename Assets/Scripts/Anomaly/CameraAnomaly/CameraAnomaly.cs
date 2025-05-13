using UnityEngine;

namespace Anomaly
{
    public class CameraAnomaly : AnomalyBase
    {
        [Header("Layer Settings")]
        [Tooltip("Layer name used when the object is hidden (e.g., HiddenAnomaly)")]
        public string anomalyLayerName = "HiddenAnomaly";

        [Tooltip("Layer name used when the object is revealed (e.g., Default or AnomalyRevealed)")]
        public string revealLayerName = "Default";

        [Header("Reveal Settings")]
        [Tooltip("Enable collider when the object is revealed")]
        public bool colliderEnableOnReveal = true;

        //[Tooltip("Effect prefab to instantiate when the object is revealed (optional)")]
        //public GameObject revealEffectPrefab;

        private Collider _collider;
        private bool _revealed = false;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                _rigidbody = GetComponent<Rigidbody>();
                if (_rigidbody) _rigidbody.isKinematic = false;
            }
        }
#endif

        protected override void Awake()
        {
            base.Awake();
            _collider = GetComponentInChildren<Collider>();
        }

        private void Start()
        {
            SetHiddenState();
        }

        public void Reveal()
        {
            if (_revealed) return;
            _revealed = true;

            // Set layer to reveal layer
            int revealLayer = LayerMask.NameToLayer(revealLayerName);
            if (revealLayer < 0)
            {
                Debug.LogWarning($"Layer '{revealLayerName}' not found. Object will remain in current layer.");
            }
            else
            {
                SetLayerRecursively(gameObject, revealLayer);
            }

            // Apply position and rotation
            transform.position = anomalousPosition;
            transform.eulerAngles = anomalousRotation;

            // Enable collider
            if (_collider && colliderEnableOnReveal)
                _collider.enabled = true;

            //// Instantiate reveal effect
            //if (revealEffectPrefab)
            //    Instantiate(revealEffectPrefab, transform.position, Quaternion.identity);

            // Activate object
            Active = true;
        }

        private void SetHiddenState()
        {
            int anomalyLayer = LayerMask.NameToLayer(anomalyLayerName);
            if (anomalyLayer < 0)
            {
                Debug.LogWarning($"Layer '{anomalyLayerName}' not found. Object will remain in current layer.");
            }
            else
            {
                SetLayerRecursively(gameObject, anomalyLayer);
            }

            if (_collider)
                _collider.enabled = false;

            Active = false;
            _revealed = false;
        }

        /// <summary>
        /// Set the layer for this object and all its children
        /// </summary>
        private void SetLayerRecursively(GameObject obj, int newLayer)
        {
            obj.layer = newLayer;
            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, newLayer);
            }
        }
    }
}
