using UnityEngine;

namespace Anomaly
{
    public class CameraAnomaly : AnomalyBase
    {
        [Header("Layer Settings")]
        [Tooltip("������ ���¿��� ����� ���̾� �̸� (��: HiddenAnomaly)")]
        public string anomalyLayerName = "HiddenAnomaly";

        [Tooltip("���ǿ� �巯�� �� ����� ���̾� �̸� (��: Default �Ǵ� AnomalyRevealed)")]
        public string revealLayerName = "Default";

        [Header("Reveal Settings")]
        [Tooltip("����ȭ �� �ݶ��̴� Ȱ��ȭ ����")]
        public bool colliderEnableOnReveal = true;

        //[Tooltip("����ȭ �� ����� ����Ʈ ������ (����)")]
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

            // ���̾� ����
            int revealLayer = LayerMask.NameToLayer(revealLayerName);
            if (revealLayer < 0)
            {
                Debug.LogWarning($"Layer '{revealLayerName}' not found. Object will remain in current layer.");
            }
            else
            {
                SetLayerRecursively(gameObject, revealLayer);
            }

            // ��ġ �� ȸ�� ����
            transform.position = anomalousPosition;
            transform.eulerAngles = anomalousRotation;

            // �ݶ��̴� Ȱ��ȭ
            if (_collider && colliderEnableOnReveal)
                _collider.enabled = true;

            //// ����Ʈ ���
            //if (revealEffectPrefab)
            //    Instantiate(revealEffectPrefab, transform.position, Quaternion.identity);

            // ���� Ȱ��ȭ
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
        /// �ڽı��� ������ ���̾� ����
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
