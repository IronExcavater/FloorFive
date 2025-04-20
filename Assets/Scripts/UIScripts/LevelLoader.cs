using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    // Level 1~5 Scene should be added to the Build Settings.
    // File > Build Settings Scene List > Add Open Scenes

    public void LoadLevel(int levelNumber)
    {
        // 씬 인덱스 = 빌드 설정에서의 씬 순서 (0부터 시작)
        // SceneIndex (Starting from 0) = Level 
        int sceneIndex = levelNumber;
        SceneManager.LoadScene(sceneIndex);

        // 또는 씬 이름으로 로드 (권장): 
        // SceneManager.LoadScene("Level" + levelNumber);
    }

    // (옵션) 씬 이름으로도 로드 가능한 버전
    public void LoadLevelByName(string sceneName)
    {
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning($"Missing '{sceneName}'Scene!");
        }
    }
}
