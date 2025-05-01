using UnityEngine;
/* ========== Anomaly.cs ========== 
 
 
 A template code for an Anomaly class in Unity.

 
 ======================================
 */

public class Anomaly : MonoBehaviour
{
    [Header("Type & Timing")]
    public AnomalyType anomalyType;
    public float failTime = 20f; // Time before fail if not fixed

    [Header("Offsets (relative to original)")]
    public Vector3 offsetPosition = Vector3.zero;
    public Vector3 offsetRotation = Vector3.zero;

    [Header("Animation")]
    public float fixLerpSpeed = 2f;

    // Internal state
    private Vector3 _originalPosition;
    private Quaternion _originalRotation;
    private Vector3 _anomalyPosition;
    private Quaternion _anomalyRotation;

    private float _lifeTime = 0f;
    private bool _isEnabled = false;
    private bool _isTriggered = false;
    private bool _isFixed = false;

    [Header("Type-Specific Settings")]
    public ParticleSystem fireEffect;      


    private void Awake()
    {
        _originalPosition = transform.position;
        _originalRotation = transform.rotation;
        _anomalyPosition = _originalPosition + transform.TransformDirection(offsetPosition);
        _anomalyRotation = _originalRotation * Quaternion.Euler(offsetRotation);
    }

    private void Update()
    {
        if (!_isEnabled || !_isTriggered || _isFixed)
            return;
        // Type specific logic// 
        switch (anomalyType)
        {
            case AnomalyType.EMF:
               
                break;

            case AnomalyType.Camera:
                break;
            
            case AnomalyType.Flashlight:
                break;

            case AnomalyType.FlashBeacon:
                break;
        }
        // Check if the anomaly is fixed (back to original position/rotation)

        if (!IsFixed())
        {
            _lifeTime += Time.deltaTime;
            if (_lifeTime >= failTime)
            {
                OnFail();
            }
        }
        else
        {
            _lifeTime = 0f;
            _isFixed = true;
            SmoothFix();
        }
    }

    /// <summary>
    /// Triggers the anomaly: moves to anomaly position/rotation and starts timer.
    /// </summary>
    public void TriggerAnomaly()
    {
        if (_isTriggered) return;
        _isTriggered = true;
        _isFixed = false;
        _lifeTime = 0f;

        transform.position = _anomalyPosition;
        transform.rotation = _anomalyRotation;
    }

    /// <summary>
    /// Enables anomaly logic (timer runs).
    /// </summary>
    public void EnableAnomaly()
    {
        _isEnabled = true;
    }

    /// <summary>
    /// Disables anomaly logic (timer pauses).
    /// </summary>
    public void DisableAnomaly()
    {
        _isEnabled = false;
    }

    /// <summary>
    /// Determines if anomaly is back to original position/rotation.
    /// </summary>
    private bool IsFixed()
    {
        float posDiff = Vector3.Distance(transform.position, _originalPosition);
        float rotDiff = Quaternion.Angle(transform.rotation, _originalRotation);
        return posDiff < 0.1f && rotDiff < 5f;
    }

    /// <summary>
    /// Smoothly lerps anomaly back to original state.
    /// </summary>
    private void SmoothFix()
    {
        // If you have AnimationManager.NewTween, use it here instead!
        transform.position = Vector3.Lerp(transform.position, _originalPosition, Time.deltaTime * fixLerpSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, _originalRotation, Time.deltaTime * fixLerpSpeed);
        // Example: AnimationManager.NewTween(transform, _originalPosition, _originalRotation);
    }

    private void OnFail()
    {
        Debug.LogWarning($"Anomaly FAILED: {anomalyType} at {gameObject.name}");
        // TODO: Add fail logic (e.g., notify Room, show UI, etc.)
        _isTriggered = false;
        _isEnabled = false;
    }
}