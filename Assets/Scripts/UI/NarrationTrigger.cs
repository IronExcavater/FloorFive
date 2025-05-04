using UnityEngine;

public class NarrationTrigger : MonoBehaviour
{
    [SerializeField] private SubtitleLine subtitleLine;

    private bool hasPlayed = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!hasPlayed && other.CompareTag("Player"))
        {
            SubtitleManager.Instance.PlaySubtitleLine(subtitleLine);
            hasPlayed = true;
        }
    }
}
