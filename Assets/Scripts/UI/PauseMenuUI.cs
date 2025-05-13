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
        
        private bool _isPaused;

        public bool IsPaused
        {
            get => _isPaused;
            set
            {
                _isPaused = value;
                
                _canvasGroup.alpha = _isPaused ? 1 : 0;
                _canvasGroup.blocksRaycasts = _isPaused;
                _canvasGroup.interactable = _isPaused;
                Time.timeScale = _isPaused ? 1 : 0;
                
                Cursor.lockState = _isPaused ? CursorLockMode.None : CursorLockMode.Locked;
                Cursor.visible = _isPaused;

                if (_playerInput != null)
                {
                    _playerInput.SwitchCurrentActionMap(_isPaused ? "UI" : "Player");
                }
            }
        }

        private void Awake()
        {
            _canvasGroup = GetComponentInChildren<CanvasGroup>();
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
            IsPaused = false;
            LoadManager.LoadScene(LoadManager.MainMenuSceneIndex, LoadSceneMode.Single);
        }
    }
}