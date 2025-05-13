using System.Collections;
using Load;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace UI
{
    public class PauseMenuUI : MonoBehaviour
    {
        private CanvasGroup _canvasGroup;
        private PlayerInput _playerInput;
        
        [SerializeField] private float fadeDuration = 1f;
        private Coroutine _fadeCoroutine;
        
        private bool _isPaused;

        public bool IsPaused
        {
            get => _isPaused;
            set
            {
                _isPaused = value;
                
                Cursor.lockState = _isPaused ? CursorLockMode.None : CursorLockMode.Locked;
                Cursor.visible = _isPaused;

                if (_playerInput != null)
                {
                    _playerInput.SwitchCurrentActionMap(_isPaused ? "UI" : "Player");
                }
                
                if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = StartCoroutine(FadeCoroutine(_canvasGroup, _isPaused ? 1f : 0f));
            }
        }

        private void Awake()
        {
            _canvasGroup = GetComponentInChildren<CanvasGroup>();
            _canvasGroup.alpha = 0;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
            
            _playerInput = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerInput>();
            IsPaused = false;
        }

        private void Update()
        {
            if (_playerInput.actions["Pause"].WasPressedThisFrame()) TogglePause();
        }

        public void TogglePause()
        {
            IsPaused = !IsPaused;
        }

        public void OnResumeButton()
        {
            IsPaused = false;
        }

        public void OnPauseButton()
        {
            IsPaused = true;
        }

        public void OnExitButton()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            LoadManager.LoadScene(LoadManager.MainMenuSceneIndex, LoadSceneMode.Single);
        }
        
        // Coroutine to handle fade effect
        private IEnumerator FadeCoroutine(CanvasGroup canvasGroup, float targetAlpha)
        {
            float elapsedTime = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                var t = Mathf.Lerp(canvasGroup.alpha, targetAlpha, elapsedTime / fadeDuration);
                canvasGroup.alpha = t;
                Time.timeScale = -t + 1;
                yield return null;
            }

            canvasGroup.alpha = targetAlpha;
            canvasGroup.interactable = targetAlpha == 1f;
            canvasGroup.blocksRaycasts = targetAlpha == 1f;
            _fadeCoroutine = null;
        }
    }
}