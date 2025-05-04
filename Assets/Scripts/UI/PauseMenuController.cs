using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;
using UnityEngine.EventSystems;

public class PauseMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject helpPanel;
    [SerializeField] private Text levelInfoText;
    [SerializeField] private Text narratorTipText;

    [Header("UI Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button helpButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button pauseToggleButton; // 햄버거/ESC 역할

    [Header("Narrator Tips")]
    [SerializeField]
    private string[] narratorTips = {
        "Tip: Take your time and observe the world closely!",
        "Tip: Not everything is what it seems.",
        "Tip: Use the shadows to your advantage.",
        "Tip: Don’t forget to explore hidden areas.",
        "Tip: Sometimes waiting is the best action."
    };

    private bool isPaused = false;

    private void Start()
    {
        // 패널 비활성화
        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);
        helpPanel.SetActive(false);

        // 버튼 이벤트 연결
        resumeButton.onClick.AddListener(ResumeGame);
        saveButton.onClick.AddListener(SaveGame);
        mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        settingsButton.onClick.AddListener(OpenSettings);
        helpButton.onClick.AddListener(OpenHelp);
        quitButton.onClick.AddListener(QuitGame);
        if (pauseToggleButton != null)
            pauseToggleButton.onClick.AddListener(TogglePause);

        // 레벨 정보 표시
        if (levelInfoText != null)
        {
            string sceneName = SceneManager.GetActiveScene().name;
            string levelNumber = Regex.Match(sceneName, @"\d+").Value;
            levelInfoText.text = $"📍 Current Stage: Level {levelNumber}";
        }

        // 랜덤 내레이터 팁 표시
        if (narratorTipText != null && narratorTips.Length > 0)
        {
            int randomIndex = Random.Range(0, narratorTips.Length);
            narratorTipText.text = narratorTips[randomIndex];
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 순차적으로 닫기: help → settings → pause
            if (helpPanel.activeSelf)
            {
                helpPanel.SetActive(false);
            }
            else if (settingsPanel.activeSelf)
            {
                settingsPanel.SetActive(false);
            }
            else if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void TogglePause()
    {
        if (isPaused) ResumeGame();
        else PauseGame();
    }

    public void PauseGame()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        AudioListener.pause = true;

        // UI 네비게이션: 첫 버튼 선택
        if (resumeButton != null)
            EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
    }

    public void ResumeGame()
    {
        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);
        helpPanel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        AudioListener.pause = false;
    }

    public void SaveGame()
    {
        // 예시: 현재 씬 이름만 저장 (확장 가능)
        PlayerPrefs.SetString("SavedLevel", SceneManager.GetActiveScene().name);
        PlayerPrefs.Save();
        Debug.Log("Game saved successfully!");
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
        helpPanel.SetActive(false);
    }

    public void OpenHelp()
    {
        helpPanel.SetActive(true);
        settingsPanel.SetActive(false);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        Debug.Log("QuitGame called (Editor)");
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
