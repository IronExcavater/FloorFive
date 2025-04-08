using TMPro;

public class Checkpoint : Area
{
    public TextMeshPro scoreText;

    private void Start()
    {
        scoreText.text = GameManager.Score.ToString();
        GameManager.OnScoreChange += score => scoreText.text = score.ToString();
    }
}