using TMPro;
using UnityEngine;
using Utils;

namespace UI
{
    [DoNotDestroySingleton]
    public class InteractionUI : Singleton<InteractionUI>
    {
        private Interactable _target;
        private TMP_Text _promptText;
        private float _targetAlpha;
        
        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;
        private Camera _mainCamera;

        protected override void Awake()
        {
            base.Awake();
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _promptText = GetComponentInChildren<TMP_Text>();
            _mainCamera = Camera.main;
            
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.alpha = 0f;
        }
        
        public static void ShowPrompt(Interactable target, string promptText)
        {
            Instance._target = target;
            Instance._promptText.text = promptText;
            Instance._targetAlpha = 1;
        }

        public static void HidePrompt()
        {
            Instance._targetAlpha = 0;
        }

        private void Update()
        {
            if (_mainCamera == null) _mainCamera = Camera.main;
            
            if (_mainCamera == null || Instance._target == null) return;
            
            _canvasGroup.alpha = Mathf.MoveTowards(_canvasGroup.alpha, _targetAlpha, Time.deltaTime * 2f);
            Vector3 direction = Instance._target.transform.position - _mainCamera.transform.position;
            _rectTransform.position = Instance._target.transform.position
                + Instance._target.transform.TransformDirection(Instance._target.promptOffset)
                + Vector3.up * 0.2f - direction.normalized * 0.2f;
            _rectTransform.forward = _mainCamera.transform.forward;
        }
    }
}