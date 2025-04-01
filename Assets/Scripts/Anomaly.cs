using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Anomaly : MonoBehaviour
{
    [SerializeField] private List<Renderer> rends;
    
    [SerializeField] private List<AnomalyState> anomalyStates;

    private Vector3 _originalPosition;
    private Quaternion _originalRotation;
    private Vector3 _originalScale;
    private Color _originalColor;
    
    private void Start()
    {
        _originalPosition = transform.localPosition;
        _originalRotation = transform.localRotation;
        _originalScale = transform.localScale;
    }

    public void ApplyRandomAnomaly()
    {
        if (anomalyStates.Count == 0) return;

        var state = anomalyStates[Random.Range(0, anomalyStates.Count)];

        transform.localPosition = _originalPosition + state.positionOffset;
        transform.localRotation = _originalRotation * Quaternion.Euler(state.rotationOffset);
        transform.localScale = Vector3.Scale(_originalScale, state.scaleMultiplier); 
        
        rends.ForEach(rend =>
        {
            rend.material.color = state.color;
        });
        gameObject.SetActive(!state.hidden);
    }
}