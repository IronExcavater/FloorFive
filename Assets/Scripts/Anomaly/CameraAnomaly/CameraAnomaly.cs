using UnityEngine;

namespace Anomaly
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public class CameraAnomaly : AnomalyBase
    {
        [Header("Anomaly Positioning (씬에서 조정 가능)")]
        public Vector3 anomalousPosition;
        public Vector3 anomalousRotation;

        [Header("Layer Settings")]
        [Tooltip("숨겨진 상태에서 사용할 레이어 이름 (예: HiddenAnomaly)")]
        public string anomalyLayerName = "HiddenAnomaly";

        [Tooltip("현실에 드러날 때 사용할 레이어 이름 (예: Default 또는 AnomalyRevealed)")]
        public string revealLayerName = "Default";

        [Header("Reveal Settings")]
        [Tooltip("현실화 시 콜라이더 활성화 여부")]
        public bool colliderEnableOnReveal = true;

        [Tooltip("현실화 시 재생할 이펙트 프리팹 (선택)")]
        public GameObject revealEffectPrefab;

        private Collider _collider;
        private Rigidbody _rigidbody;
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

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            SetHiddenState();
        }

        public void Reveal()
        {
            if (_revealed) return;
            _revealed = true;

            // 레이어 변경 (자식 포함)
            int revealLayer = LayerMask.NameToLayer(revealLayerName);
            if (revealLayer < 0)
            {
                Debug.LogWarning($"Layer '{revealLayerName}' not found. Object will remain in current layer.");
            }
            else
            {
                SetLayerRecursively(gameObject, revealLayer);
            }

            // 위치 및 회전 적용
            transform.position = anomalousPosition;
            transform.eulerAngles = anomalousRotation;

            // 콜라이더 활성화
            if (_collider && colliderEnableOnReveal)
                _collider.enabled = true;

            // 이펙트 재생
            if (revealEffectPrefab)
                Instantiate(revealEffectPrefab, transform.position, Quaternion.identity);

            // 상태 활성화
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
        /// 자식까지 포함한 레이어 변경
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
