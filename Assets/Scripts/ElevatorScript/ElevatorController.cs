using UnityEngine;
using UnityEngine.SceneManagement;

public class ElevatorController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private Transform platformArea;
    [SerializeField] private float platformRadius = 2f;

    [Header("Feedback")]
    [SerializeField] private GameObject interactionUI;
    [SerializeField] private AudioClip elevatorSound;

    [Header("Gizmos")]
    [SerializeField] private Color platformGizmoColor = Color.green;

    private bool _isPlayerOnPlatform = false;
    private int _currentLevel = 1;

    private void Start()
    {
        ParseCurrentLevel();
    }

    private void Update()
    {
        DetectPlayer();
        HandleInteraction();
    }

    private void ParseCurrentLevel()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        if (!int.TryParse(currentScene.Replace("Level", ""), out _currentLevel))
            Debug.LogError("Scene Name: it should follow 'LevelX' format ");
    }

    private void DetectPlayer()
    {
        if (platformArea == null) return;

        Collider[] hitColliders = Physics.OverlapSphere(
            platformArea.position,
            platformRadius
        );

        _isPlayerOnPlatform = System.Array.Exists(
            hitColliders,
            col => col.CompareTag(playerTag)
        );

        interactionUI.SetActive(_isPlayerOnPlatform);
    }

    private void HandleInteraction()
    {
        if (_isPlayerOnPlatform && Input.GetButtonDown("Interact"))
        {
            AudioSource.PlayClipAtPoint(elevatorSound, transform.position);
            LoadNextLevel();
        }
    }

    public void LoadNextLevel()
    {
        _currentLevel++;
        string sceneName = $"Level{_currentLevel}";

        if (_currentLevel > 5)
        {
            Debug.Log("Game Compelete, you've made it here!.");
            return;
        }

        if (Application.CanStreamedLevelBeLoaded(sceneName))
            SceneManager.LoadScene(sceneName);
        else
            Debug.LogError($"씬 '{sceneName}'을 찾을 수 없습니다!");
    }

    private void OnDrawGizmosSelected()
    {
        if (platformArea == null) return;

        Gizmos.color = platformGizmoColor;
        Gizmos.DrawWireSphere(platformArea.position, platformRadius);
    }
};

