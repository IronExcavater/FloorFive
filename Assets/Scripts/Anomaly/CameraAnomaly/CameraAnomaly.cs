using System.Collections;
using UnityEngine;

namespace Anomaly
{
    [RequireComponent(typeof(Collider))]
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

        [Tooltip("����ȭ �� ����� ����Ʈ ������ (����)")]
        public GameObject revealEffectPrefab;

        private Collider _collider;
        private bool _revealed = false;

        private void Start()
        {
            _collider = GetComponent<Collider>();
            SetHiddenState();
        }

        /// <summary>
        /// ������ anomaly�� ������ ȣ���. ���ǿ� �巯���� ó��.
        /// </summary>
        public void Reveal()
        {
            if (_revealed) return;
            _revealed = true;

            // ���� ���̾�� ����
            int revealLayer = LayerMask.NameToLayer(revealLayerName);
            if (revealLayer >= 0)
                gameObject.layer = revealLayer;

            // �ݶ��̴� Ȱ��ȭ
            if (_collider && colliderEnableOnReveal)
                _collider.enabled = true;

            // ���� ����Ʈ(����)
            if (revealEffectPrefab)
                Instantiate(revealEffectPrefab, transform.position, Quaternion.identity);

            // ���� Ȱ��ȭ (�ʿ��)
            Active = true;
        }

        /// <summary>
        /// ���� �� ���� ���·� ���� (ī�޶󿡸� ���̰�, ���ǿ����� �� ����)
        /// </summary>
        private void SetHiddenState()
        {
            // ���� ���̾�� ����
            int anomalyLayer = LayerMask.NameToLayer(anomalyLayerName);
            if (anomalyLayer >= 0)
                gameObject.layer = anomalyLayer;

            // �ݶ��̴� ��Ȱ��ȭ
            if (_collider)
                _collider.enabled = false;

            // ���� ��Ȱ��ȭ
            Active = false;
            _revealed = false;
        }
    }
}
