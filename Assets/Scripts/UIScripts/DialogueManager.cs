using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private Text dialogueText;

    [Header("Player Reference")]
    [SerializeField] private PlayerController playerController;

    private List<string> dialogueLines = new List<string>();
    private int currentLine = 0;
    private bool isDialogueActive = false;

    public static DialogueManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;

        dialoguePanel.SetActive(false);
    }

    private void Update()
    {
        if (isDialogueActive && Input.GetKeyDown(KeyCode.E))
        {
            ShowNextLine();
        }
    }

    public void StartDialogue(TextAsset dialogueFile)
    {
        if (dialogueFile == null) return;

        LoadDialogueFromFile(dialogueFile);

        if (dialogueLines.Count == 0) return;

        isDialogueActive = true;
        currentLine = 0;
        dialoguePanel.SetActive(true);
        dialogueText.text = dialogueLines[currentLine];

        if (playerController != null)
            playerController.SetMovement(false);
    }

    private void LoadDialogueFromFile(TextAsset file)
    {
        dialogueLines.Clear();

        string[] lines = file.text.Split('\n');
        foreach (string line in lines)
        {
            if (!string.IsNullOrWhiteSpace(line))
                dialogueLines.Add(line.Trim());
        }
    }

    private void ShowNextLine()
    {
        currentLine++;

        if (currentLine < dialogueLines.Count)
        {
            dialogueText.text = dialogueLines[currentLine];
        }
        else
        {
            EndDialogue();
        }
    }

    private void EndDialogue()
    {
        isDialogueActive = false;
        dialoguePanel.SetActive(false);

        //if (playerController != null)
        //    playerController.SetMovement(true);
    }
}
