using UnityEngine;
using System.Collections;
using Load;
using UnityEngine.SceneManagement;

namespace UI
{
    public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup levelSelectCanvasGroup;
    [SerializeField] private CanvasGroup settingsCanvasGroup;
    [SerializeField] private CanvasGroup creditsCanvasGroup;

    [SerializeField] private float fadeDuration = 0.5f;

    private CanvasGroup _currentCanvasGroup;
    
    public CanvasGroup CurrentCanvasGroup
    {
        get => _currentCanvasGroup;
        set
        {
            if (_currentCanvasGroup == value) return;
            
            FadeOut(_currentCanvasGroup);
            _currentCanvasGroup = value;
            FadeIn(_currentCanvasGroup);
        }
    }

    private void Awake()
    {
        if (levelSelectCanvasGroup != null)
        {
            levelSelectCanvasGroup.alpha = 0;
            levelSelectCanvasGroup.interactable = false;
            levelSelectCanvasGroup.blocksRaycasts = false;
        }
        
        if (settingsCanvasGroup != null)
        {
            settingsCanvasGroup.alpha = 0;
            settingsCanvasGroup.interactable = false;
            settingsCanvasGroup.blocksRaycasts = false;
        }

        if (creditsCanvasGroup != null)
        {
            creditsCanvasGroup.alpha = 0;
            creditsCanvasGroup.interactable = false;
            creditsCanvasGroup.blocksRaycasts = false;
        }
    }

    public void OnLevelSelect()
    {
        CurrentCanvasGroup = levelSelectCanvasGroup;
    }

    public void OnCredits()
    {
        CurrentCanvasGroup = creditsCanvasGroup;
    }

    public void OnExit()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #elif UNITY_STANDALONE
        Application.Quit();
        #elif UNITY_WEBGL
        Application.ExternalEval("window.close();"); // May not work due to browser restrictions
        #endif
    }

    public void OnStart()
    {
        LoadManager.UILevelIndexPressed = 2;
        LoadElevator();
    }

    public void OnTutorial()
    {
        LoadManager.UILevelIndexPressed = 2;
        LoadElevator();
    }

    public void OnLevel1()
    {
        LoadManager.UILevelIndexPressed = 3;
        LoadElevator();
    }

    public void OnLevel2()
    {
        LoadManager.UILevelIndexPressed = 4;
        LoadElevator();
    }

    public void OnLevel3()
    {
        LoadManager.UILevelIndexPressed = 5;
        LoadElevator();
    }

    public void OnLevel4()
    {
        LoadManager.UILevelIndexPressed = 6;
        LoadElevator();
    }

    private void LoadElevator()
    {
        LoadManager.LoadScene(1, LoadSceneMode.Single);
    }

    // Fade in a CanvasGroup
    private void FadeIn(CanvasGroup canvasGroup)
    {
        if (canvasGroup == null) return;
        StartCoroutine(FadeCoroutine(canvasGroup, 1));
    }

    // Fade out a CanvasGroup
    private void FadeOut(CanvasGroup canvasGroup)
    {
        if (canvasGroup == null) return;
        StartCoroutine(FadeCoroutine(canvasGroup, 0));
    }

    // Coroutine to handle fade effect
    private IEnumerator FadeCoroutine(CanvasGroup canvasGroup, float targetAlpha)
    {
        float elapsedTime = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        
        canvasGroup.gameObject.SetActive(true);

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, elapsedTime / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        canvasGroup.interactable = targetAlpha == 1f;
        canvasGroup.blocksRaycasts = targetAlpha == 1f;

        if (targetAlpha == 0f)
        {
            canvasGroup.gameObject.SetActive(false); // Deactivate the GameObject when completely faded out
        }
    }

    // Optionally, add any additional functionality here for other UI interactions
}
}
