using UnityEngine;

public class RaycastDetector : MonoBehaviour
{
    [Header("Settings")]
    public float maxRayDistance = 100f;
    public bool showDebugRay = true;

    [Header("Dependencies")]
    [SerializeField] private AnomalySystem _anomalySystem;
    [SerializeField] private Camera _mainCamera;

    void Start()
    {
        // Automatically get main camera if not assigned
        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
            if (_mainCamera == null) Debug.LogError("No camera assigned and no main camera found!");
        }

        if (!_anomalySystem) Debug.LogError("Anomaly System reference missing!");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) // More reliable than string version
        {
            Debug.Log("check");
            PerformRaycast();
        }

        if (showDebugRay && _mainCamera != null)
        {
            // Draw ray from camera forward
            Debug.DrawRay(_mainCamera.transform.position,
                        _mainCamera.transform.forward * maxRayDistance,
                        Color.red);
        }
    }

    private void PerformRaycast()
    {
        if (_mainCamera == null) return;

        // Create ray through center of screen (better for first-person)
        Ray ray = _mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance))
        {
            GameObject hitObject = hit.collider.gameObject;
            Debug.Log($"Hit object: {hitObject.name}", hitObject);

            if (_anomalySystem != null)
            {

                _anomalySystem.RestoreSpecificObject(hitObject.name);

                Transform current = hit.transform;
                int levelsChecked = 0;

                while (current != null && levelsChecked < 4)
                {
                    Debug.Log($"Checking: {current.name}");
                    _anomalySystem.RestoreSpecificObject(current.name);

                    // Move up hierarchy
                    current = current.parent;
                    levelsChecked++;
                }
            }
        }
        else
        {
            Debug.Log("Raycast didn't hit anything");
        }
    }
}