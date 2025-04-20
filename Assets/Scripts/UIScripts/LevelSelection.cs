using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField] private CanvasGroup levelSelectCanvasGroup;
    [SerializeField] private CanvasGroup settingsCanvasGroup;
    [SerializeField] private CanvasGroup creditsCanvasGroup;

    [SerializeField] private float fadeDuration = 0.5f;

    private bool isLevelSelectVisible = false;
    private bool isSettingsVisible = false;
    private bool isCreditsVisible = false;

    private Coroutine currentCoroutine;

    // Toggle Level Selection UI
    public void ToggleLevelSelection()
    {
        if (isLevelSelectVisible)
        {
            FadeOut(levelSelectCanvasGroup);
        }
        else
        {
            FadeIn(levelSelectCanvasGroup);
        }
    }

    // Toggle Settings UI
    public void ToggleSettings()
    {
        if (isSettingsVisible)
        {
            FadeOut(settingsCanvasGroup);
        }
        else
        {
            FadeIn(settingsCanvasGroup);
        }
    }

    // Toggle Credits UI
    public void ToggleCredits()
    {
        if (isCreditsVisible)
        {
            FadeOut(creditsCanvasGroup);
        }
        else
        {
            FadeIn(creditsCanvasGroup);
        }
    }

    // Fade in a CanvasGroup
    private void FadeIn(CanvasGroup canvasGroup)
    {
        if (canvasGroup == null) return;
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);

        canvasGroup.gameObject.SetActive(true);
        currentCoroutine = StartCoroutine(FadeCoroutine(canvasGroup, 0f, 1f));
    }

    // Fade out a CanvasGroup
    private void FadeOut(CanvasGroup canvasGroup)
    {
        if (canvasGroup == null) return;
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);

        currentCoroutine = StartCoroutine(FadeCoroutine(canvasGroup, 1f, 0f));
    }

    // Coroutine to handle fade effect
    private IEnumerator FadeCoroutine(CanvasGroup canvasGroup, float startAlpha, float endAlpha)
    {
        float elapsedTime = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = endAlpha;
        canvasGroup.interactable = (endAlpha == 1f);
        canvasGroup.blocksRaycasts = (endAlpha == 1f);

        if (endAlpha == 0f)
        {
            canvasGroup.gameObject.SetActive(false);
        }

        currentCoroutine = null;
    }

    // Optionally, add any additional functionality here for other UI interactions
}
