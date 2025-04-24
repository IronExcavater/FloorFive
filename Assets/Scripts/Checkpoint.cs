using System;
using TMPro;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private TextMeshPro countText;

    private void Awake()
    {
        // Initialize the score text using GameManager's Score
        countText.text = GameManager.Score.ToString();

        // Subscribe to the OnScoreChange event from GameManager
        GameManager.OnScoreChange += UpdateScoreText;
    }

    private void OnEnable()
    {
        // Optional: If GameController.OnScoreChanged needs to be subscribed to as well
        GameController.OnScoreChanged += UpdateScoreText;
    }

    private void OnDisable()
    {
        GameController.OnScoreChanged -= UpdateScoreText;
        GameManager.OnScoreChange -= UpdateScoreText; // Unsubscribe from GameManager's event
    }

    private void UpdateScoreText(int newScore)
    {
        countText.text = newScore.ToString(); // Update the score text dynamically
    }
}