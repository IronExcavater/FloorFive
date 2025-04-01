using System;
using TMPro;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private TextMeshPro countText;

    private void OnEnable()
    {
        GameController.OnScoreChanged += UpdateScoreText;
    }

    private void OnDisable()
    {
        GameController.OnScoreChanged -= UpdateScoreText;
    }

    private void UpdateScoreText(int newScore)
    {
        countText.text = newScore.ToString();
    }
}