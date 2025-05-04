using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    // Level 1~5 Scene should be added to the Build Settings.
    // File > Build Settings Scene List > Add Open Scenes

    public void LoadLevel(int levelNumber)
    {
        // �� �ε��� = ���� ���������� �� ���� (0���� ����)
        // SceneIndex (Starting from 0) = Level 
        int sceneIndex = levelNumber;
        SceneManager.LoadScene(sceneIndex);

        // �Ǵ� �� �̸����� �ε� (����): 
        // SceneManager.LoadScene("Level" + levelNumber);
    }

    // (�ɼ�) �� �̸����ε� �ε� ������ ����
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
