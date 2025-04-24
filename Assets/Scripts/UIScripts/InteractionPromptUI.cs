using UnityEngine;
using TMPro;

public class InteractionPromptUI : MonoBehaviour
{
    public static InteractionPromptUI Instance { get; private set; }

    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text promptText;

    private void Awake()
    {
        // Singleton pattern with safety check
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple instances of InteractionPromptUI detected. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (panel == null)
            Debug.LogWarning("Panel is not assigned in the inspector.");
        if (promptText == null)
            Debug.LogWarning("PromptText is not assigned in the inspector.");

        panel?.SetActive(false);
    }

    public void ShowPrompt(string text)
    {
        if (promptText == null || panel == null) return;

        // Only update if text has changed
        if (promptText.text != text)
        {
            promptText.text = text;
        }

        if (!panel.activeSelf)
            panel.SetActive(true);
    }

    public void HidePrompt()
    {
        if (panel != null && panel.activeSelf)
        {
            panel.SetActive(false);
        }
    }
}
