using UnityEngine;
using UnityEngine.SceneManagement;


/// <summary>
/// This code controls the elevator functionality,
/// the range of dections, and the area where the player must be to trigger the elevator.
/// and elevator lead player to the next level.
/// </summary>
public class ElevatorController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float detectionRange = 3f;         // Player detection range
    [SerializeField] private string playerTag = "Player";       // Tag used to identify the player
    [SerializeField] private Transform platformArea;            // Area where player must be to trigger elevator
    [SerializeField] private float platformRadius = 2f;         // Area radius to check if player is on platform

    [Header("Gizmos")]
    [SerializeField] private Color platformGizmoColor = Color.green;   // Gizmo color for platform area
    [SerializeField] private Color detectionGizmoColor = Color.yellow; // Gizmo color for detection range

    private bool _isPlayerOnPlatform = false;
    private int _currentLevel = 1;

    private void Update()
    {
        if (platformArea != null)
        {
            Collider[] hitColliders = Physics.OverlapSphere(platformArea.position, platformRadius);
            _isPlayerOnPlatform = System.Array.Exists(hitColliders, col => col.CompareTag(playerTag));
        }

        if (_isPlayerOnPlatform && Input.GetKeyDown(KeyCode.E))
        {
            LoadNextLevel();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            Debug.Log("[Elevator] You are on the platform. Press 'E' to move to the next floor.");
        }
    }

    public void LoadNextLevel()
    {
        _currentLevel = Mathf.Clamp(_currentLevel + 1, 1, 5);
        SceneManager.LoadScene($"Level{_currentLevel}");
    }

    private void OnDrawGizmos()
    {
        // Show detection range around this elevator object
        Gizmos.color = detectionGizmoColor;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }

    private void OnDrawGizmosSelected()
    {
        // Show platform detection area more clearly when selected
        if (platformArea != null)
        {
            Gizmos.color = platformGizmoColor;
            Gizmos.DrawWireSphere(platformArea.position, platformRadius);
        }
    }
}
