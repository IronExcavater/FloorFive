using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private string sceneName;
    public int EsceneIndex;
    private void Awake()
    {
        // Prevent this object from being destroyed between scenes
        DontDestroyOnLoad(this.gameObject);
    }

    public void LoadLevel1()
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        EsceneIndex = 1;
    }
    public void LoadLevel2()
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        EsceneIndex = 2;
    }
    public void LoadLevel3()
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        EsceneIndex = 3;
    }
    public void LoadLevel4()
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        EsceneIndex = 4;
    }
    public void LoadLevel5()
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        EsceneIndex = 5;
    }
}