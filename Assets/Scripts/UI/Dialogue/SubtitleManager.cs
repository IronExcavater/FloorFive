using UnityEngine;
using TMPro;
using System.Collections;

public class SubtitleManager : MonoBehaviour
{
    public static SubtitleManager Instance;

    [SerializeField] private TMP_Text subtitleText;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float fallbackDuration = 2.5f;

    private Coroutine currentRoutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        subtitleText.text = "";
        subtitleText.gameObject.SetActive(false);
    }

    public void PlaySubtitleLine(SubtitleLine line)
    {
        if (line == null) return;

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        float duration = line.audioClip != null ? line.audioClip.length : fallbackDuration;
        currentRoutine = StartCoroutine(PlaySubtitleRoutine(line, duration));
    }

    private IEnumerator PlaySubtitleRoutine(SubtitleLine line, float duration)
    {
        if (line.audioClip != null)
            audioSource.PlayOneShot(line.audioClip);

        subtitleText.text = line.subtitleText;
        subtitleText.gameObject.SetActive(true);

        yield return new WaitForSeconds(duration);

        subtitleText.text = "";
        subtitleText.gameObject.SetActive(false);
    }
}
