using TMPro;
using UnityEngine;
using Utils;

namespace UI
{
    public class InteractionUI : Singleton<InteractionUI>
    {
        private Transform _target;
        private Canvas _canvas;
        private TMP_Text _promptText;

        protected override void Awake()
        {
            base.Awake();
            _canvas = GetComponent<Canvas>();
            _promptText = GetComponentInChildren<TMP_Text>();
            _canvas.enabled = false;
        }
        
        public static void ShowPrompt(Transform target, string promptText)
        {
            Instance._target = target;
            Instance._promptText.text = promptText;
            Instance._canvas.enabled = true;
        }

        public static void HidePrompt()
        {
            Instance._canvas.enabled = false;
        }

        private void LateUpdate()
        {
            Camera camera = Camera.main;
            if (camera == null || Instance._target == null) return;

            Vector3 direction = Instance._target.position - camera.transform.position; // TODO: Not working
            transform.position = Instance._target.position + Vector3.up * 0.2f + direction.normalized * 0.2f;
            transform.forward = -camera.transform.forward;
        }
    }
}