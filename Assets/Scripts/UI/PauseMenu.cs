using UnityEngine;
using Utils;

namespace UI
{
    [DoNotDestroySingleton]
    public class PauseMenu : Singleton<PauseMenu>
    {
        [Header("UI")]
        [SerializeField] private GameObject pauseCanvas;  // Assign your Canvas/GameObject here

        private bool isPaused = false;

        // Track cursor state so we can restore it
        private CursorLockMode prevLockState;
        private bool prevCursorVisible;

        private void Start()
        {
            if (pauseCanvas != null)
                pauseCanvas.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
                TogglePause();
        }

        public void TogglePause()
        {
            if (isPaused) Unpause();
            else Pause();
        }

        private void Pause()
        {
            // Save current cursor state
            prevLockState = Cursor.lockState;
            prevCursorVisible = Cursor.visible;

            // Show pause UI
            if (pauseCanvas != null)
                pauseCanvas.SetActive(true);

            // Free and show cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Freeze game time
            Time.timeScale = 0f;
            isPaused = true;
        }

        private void Unpause()
        {
            // Hide pause UI
            if (pauseCanvas != null)
                pauseCanvas.SetActive(false);

            // Restore cursor
            Cursor.lockState = prevLockState;
            Cursor.visible = prevCursorVisible;

            // Resume game time
            Time.timeScale = 1f;
            isPaused = false;
        }

        /// <summary>
        /// Call this from your level‐load buttons if needed.
        /// Ensures we unpause before loading a new scene.
        /// </summary>
        public void LoadLevel(int index, string sceneToLoad)
        {
            if (isPaused) Unpause();
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoad);
        }
    }
}