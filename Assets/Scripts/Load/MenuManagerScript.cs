using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private string mainSceneName;    // your gameplay Scene
    [SerializeField] private string pauseSceneName;   // e.g. "PauseMenu"

    public int EsceneIndex { get; private set; }

    private bool isPaused = false;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            TogglePause();
    }

    public void TogglePause()
    {
        if (isPaused)
            Unpause();
        else
            Pause();
    }

    private void Pause()
    {
        // Load the pause Scene additively
        SceneManager.LoadSceneAsync(pauseSceneName, LoadSceneMode.Additive);
        Time.timeScale = 0;
        isPaused = true;
    }

    private void Unpause()
    {
        // Unload the pause Scene
        SceneManager.UnloadSceneAsync(pauseSceneName);
        Time.timeScale = 1;
        isPaused = false;
    }

    public void LoadLevel(int index, string sceneToLoad)
    {
        // Unpause in case you came from a paused state
        if (isPaused) Unpause();

        EsceneIndex = index;
        // Load the chosen gameplay Scene (single mode)
        SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Single);
    }

    // Convenience methods for your buttons
    public void LoadLevel1() => LoadLevel(1, mainSceneName);
    public void LoadLevel2() => LoadLevel(2, mainSceneName);
    public void LoadLevel3() => LoadLevel(3, mainSceneName);
    public void LoadLevel4() => LoadLevel(4, mainSceneName);
    public void LoadLevel5() => LoadLevel(5, mainSceneName);
}