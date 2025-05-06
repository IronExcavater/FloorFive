using TMPro;
using UnityEngine;
using Utils;

namespace UI
{
    public class InteractionUI : Singleton<InteractionUI>
    {
        private Canvas _canvas;
        private TMP_Text _promptText;

        protected override void Awake()
        {
            base.Awake();
            _canvas = GetComponent<Canvas>();
            _promptText = GetComponentInChildren<TMP_Text>();
        }
        
        public static void ShowPrompt(Transform target, string promptText)
        {
            Instance._promptText.text = promptText;
            Instance.transform.position = target.position + Vector3.up * 0.2f;
            Instance._canvas.enabled = true;
        }

        public static void HidePrompt()
        {
            Instance._canvas.enabled = false;
        }

        private void LateUpdate()
        {
            transform.forward = Camera.main.transform.forward;
        }
    }
}