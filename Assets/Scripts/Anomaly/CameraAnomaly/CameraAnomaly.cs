using System.Collections;
using UnityEngine;

namespace Anomaly
{
    [RequireComponent(typeof(Collider))]
    public class CameraAnomaly : AnomalyBase
    {
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
        private bool _revealed = false;

        private void Start()
        {
            _collider = GetComponent<Collider>();
            SetHiddenState();
        }

        /// <summary>
        /// 사진에 anomaly가 찍히면 호출됨. 현실에 드러나는 처리.
        /// </summary>
        public void Reveal()
        {
            if (_revealed) return;
            _revealed = true;

            // 현실 레이어로 변경
            int revealLayer = LayerMask.NameToLayer(revealLayerName);
            if (revealLayer >= 0)
                gameObject.layer = revealLayer;

            // 콜라이더 활성화
            if (_collider && colliderEnableOnReveal)
                _collider.enabled = true;

            // 연출 이펙트(선택)
            if (revealEffectPrefab)
                Instantiate(revealEffectPrefab, transform.position, Quaternion.identity);

            // 상태 활성화 (필요시)
            Active = true;
        }

        /// <summary>
        /// 시작 시 숨김 상태로 세팅 (카메라에만 보이고, 현실에서는 안 보임)
        /// </summary>
        private void SetHiddenState()
        {
            // 숨김 레이어로 변경
            int anomalyLayer = LayerMask.NameToLayer(anomalyLayerName);
            if (anomalyLayer >= 0)
                gameObject.layer = anomalyLayer;

            // 콜라이더 비활성화
            if (_collider)
                _collider.enabled = false;

            // 상태 비활성화
            Active = false;
            _revealed = false;
        }
    }
}
