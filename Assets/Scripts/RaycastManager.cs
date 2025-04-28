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
        if (Input.GetMouseButtonDown(0)) // More reliable than string version
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
        Debug.Log("raycast");
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance))
        {
            GameObject hitObject = hit.collider.gameObject;
            Debug.Log($"Hit object: {hitObject.name}", hitObject);

            if (_anomalySystem != null)
            {
                Transform current = hit.transform;
                int levelsChecked = 0;
                bool restored = false;

                while (current != null && levelsChecked < 4 && !restored)
                {
                    Debug.Log($"Checking: {current.name}");

                    // Store name before potentially nulling current
                    string currentName = current.name;

                    // Check if this object is in the anomaly list
                    if (_anomalySystem.IsObjectAnomalous(currentName))
                    {
                        _anomalySystem.RestoreSpecificObject(currentName);
                        restored = true;
                    }

                    current = current.parent;
                    levelsChecked++;
                }

                if (!restored)
                {
                    Debug.Log("No valid anomaly found in hierarchy");
                }
            }
        }
    }
}