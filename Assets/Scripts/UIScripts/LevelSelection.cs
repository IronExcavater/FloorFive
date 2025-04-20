using UnityEngine;
using System.Collections;

public class LevelSelection : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 0.5f;

    private Coroutine currentCoroutine;
    private bool isVisible = false; // save the visibility state

    // toggle the fade in/out

    public void test()
    {
        Debug.Log("test");
    }
   
    public void ToggleFade()
    {
        if (isVisible)
        {
            FadeOut();
        }
        else
        {
            FadeIn();
        }
    }

    public void FadeIn()
    {
        if (canvasGroup == null) return;
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);

        gameObject.SetActive(true);
        currentCoroutine = StartCoroutine(FadeCoroutine(0f, 1f));
        isVisible = true;
    }

    public void FadeOut()
    {
        if (canvasGroup == null) return;
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);

        currentCoroutine = StartCoroutine(FadeCoroutine(1f, 0f));
        isVisible = false;
    }

    private IEnumerator FadeCoroutine(float startAlpha, float endAlpha)
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
            gameObject.SetActive(false);
        }

        currentCoroutine = null;
    }
}
