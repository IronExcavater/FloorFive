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

        private Collider _collider;
        private bool _revealed = false;
        private Room _room;

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
            // Room 오브젝트 찾기 (Room 태그가 있다고 가정)
            GameObject roomObj = GameObject.FindGameObjectWithTag("Room");
            if (roomObj != null)
                _room = roomObj.GetComponent<Room>();

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

            Active = true;
        }

        private void SetHiddenState()
        {
            // Room이 활성화되어 있을 때만 HiddenAnomaly 레이어로 전환
            if (_room != null && _room.gameObject.activeInHierarchy)
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
            else
            {
                Debug.Log($"Room is not active. CameraAnomaly will not switch to HiddenAnomaly layer.");
            }
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
