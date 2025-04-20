using UnityEngine;
using System.Collections;

public class UIController : MonoBehaviour
{
    [SerializeField] private CanvasGroup levelSelectCanvasGroup;
    [SerializeField] private CanvasGroup settingsCanvasGroup;
    [SerializeField] private CanvasGroup creditsCanvasGroup;

    [SerializeField] private float fadeDuration = 0.5f;

    private bool isLevelSelectVisible = false; // level selection visibility state
    private bool isSettingsVisible = false;
    private bool isCreditsVisible = false;

    private Coroutine currentCoroutine;

    // Toggle Level Selection UI
    public void ToggleLevelSelection()
    {
        if (isLevelSelectVisible)
        {
            FadeOut(levelSelectCanvasGroup); // Hide Level Select UI
        }
        else
        {
            FadeIn(levelSelectCanvasGroup); // Show Level Select UI
        }
    }

    // Toggle Settings UI
    public void ToggleSettings()
    {
        if (isSettingsVisible)
        {
            FadeOut(settingsCanvasGroup); // Hide Settings UI
        }
        else
        {
            FadeIn(settingsCanvasGroup); // Show Settings UI
        }
    }

    // Toggle Credits UI
    public void ToggleCredits()
    {
        if (isCreditsVisible)
        {
            FadeOut(creditsCanvasGroup); // Hide Credits UI
        }
        else
        {
            FadeIn(creditsCanvasGroup); // Show Credits UI
        }
    }

    // Fade in a CanvasGroup
    private void FadeIn(CanvasGroup canvasGroup)
    {
        if (canvasGroup == null) return;
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);

        canvasGroup.gameObject.SetActive(true); // Activate the GameObject first
        currentCoroutine = StartCoroutine(FadeCoroutine(canvasGroup, 0f, 1f)); // Start fade in
    }

    // Fade out a CanvasGroup
    private void FadeOut(CanvasGroup canvasGroup)
    {
        if (canvasGroup == null) return;
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);

        currentCoroutine = StartCoroutine(FadeCoroutine(canvasGroup, 1f, 0f)); // Start fade out
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
            canvasGroup.gameObject.SetActive(false); // Deactivate the GameObject when completely faded out
        }

        // Update visibility state based on fade direction
        if (canvasGroup == levelSelectCanvasGroup)
        {
            isLevelSelectVisible = (endAlpha == 1f); // Update the visibility flag
        }
        else if (canvasGroup == settingsCanvasGroup)
        {
            isSettingsVisible = (endAlpha == 1f);
        }
        else if (canvasGroup == creditsCanvasGroup)
        {
            isCreditsVisible = (endAlpha == 1f);
        }

        currentCoroutine = null;
    }

    // Optionally, add any additional functionality here for other UI interactions
}
