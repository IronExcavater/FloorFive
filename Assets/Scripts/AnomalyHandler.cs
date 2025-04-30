using System.Collections.Generic;
using UnityEngine;

public class AnomalySystem : MonoBehaviour
{
    [Header("Parent Object")]
    public GameObject parentObject;

    [Header("Anomaly Settings")]
    public float positionShiftStrength = 0.5f;
    public float rotationShiftStrength = 15f;
    public float scaleShiftStrength = 0.2f;
    [Tooltip("0 = affect all children")]
    public int numberOfAnomalies = 3;

    private Dictionary<GameObject, OriginalTransform> _originalTransforms = new();
    [SerializeField] private List<GameObject> _affectedObjects = new();

    [System.Serializable]
    private struct OriginalTransform
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
    }

    void Start()
    {
        if (parentObject == null)
        {
            Debug.LogError("Parent object not assigned!");
            return;
        }
        StoreOriginalTransforms();
        TriggerAnomaly();
    }

    private void StoreOriginalTransforms()
    {
        _originalTransforms.Clear();
        foreach (Transform child in parentObject.transform)
        {
            GameObject obj = child.gameObject;
            _originalTransforms[obj] = new OriginalTransform
            {
                position = obj.transform.localPosition,
                rotation = obj.transform.localRotation,
                scale = obj.transform.localScale
            };
        }
    }

    public void TriggerAnomaly()
    {
        if (_originalTransforms.Count == 0) return;

        // Get all available children
        var candidates = new List<GameObject>(_originalTransforms.Keys);

        // Determine actual number to affect (0 = all)
        int targetCount = numberOfAnomalies > 0 ?
            Mathf.Min(numberOfAnomalies, candidates.Count) :
            candidates.Count;

        // Select random objects
        while (_affectedObjects.Count < targetCount && candidates.Count > 0)
        {
            int randomIndex = Random.Range(0, candidates.Count);
            GameObject selected = candidates[randomIndex];
            candidates.RemoveAt(randomIndex);

            if (!_affectedObjects.Contains(selected))
            {
                _affectedObjects.Add(selected);
                ApplyAnomaly(selected);
            }
        }
    }

    private void ApplyAnomaly(GameObject obj)
    {
        OriginalTransform original = _originalTransforms[obj];

        // Position
        obj.transform.localPosition = original.position +
            Random.insideUnitSphere * positionShiftStrength;

        // Rotation
        obj.transform.localRotation = original.rotation *
            Quaternion.Euler(
                Random.Range(-rotationShiftStrength, rotationShiftStrength),
                Random.Range(-rotationShiftStrength, rotationShiftStrength),
                Random.Range(-rotationShiftStrength, rotationShiftStrength)
            );

        // Scale
        float scaleFactor = 1 + Random.Range(-scaleShiftStrength, scaleShiftStrength);
        obj.transform.localScale = original.scale * scaleFactor;
      

    }

    public void RestoreAllObjects()
    {
        foreach (GameObject obj in _affectedObjects)
        {
            if (_originalTransforms.TryGetValue(obj, out OriginalTransform original))
            {
                obj.transform.localPosition = original.position;
                obj.transform.localRotation = original.rotation;
                obj.transform.localScale = original.scale;
            }
        }
        _affectedObjects.Clear();
    }

    public void RestoreSpecificObject(string objectName)
    {
        for (int i = _affectedObjects.Count - 1; i >= 0; i--)
        {
            GameObject obj = _affectedObjects[i];
            if (obj.name == objectName)
            {
                if (_originalTransforms.TryGetValue(obj, out OriginalTransform original))
                {   
                    obj.transform.localPosition = original.position;
                    obj.transform.localRotation = original.rotation;
                    obj.transform.localScale = original.scale;
                }
                _affectedObjects.RemoveAt(i);
            }
        }
    }
}