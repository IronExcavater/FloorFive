using System.Collections;
using UnityEngine;

[System.Serializable]
public class Anomaly
{
    public GameObject target;
    private Transform _trans;
    
    public enum Trigger
    {
        Default,
        AfterSeen, // Wait until seen for x seconds
        AfterUnseen, // Wait until seen then unseen
    }

    public enum Mode
    {
        EndAsAnomaly,
        BeginAsAnomaly,
    }
    
    public Trigger trigger = Trigger.Default;
    public Mode mode = Mode.EndAsAnomaly;

    public float seenDistance = 3;
    public float seenAngle = 100;
    
    public bool visible = true;
    public float transitionTime = 2;
    public float triggerDelay = 2;
    public Vector3 positionOffset = Vector3.zero;
    public Vector3 rotationOffset = Vector3.zero;
    public Vector3 scaleOffset = Vector3.zero;

    private Vector3 _startPos;
    private Quaternion _startRot;
    private Vector3 _startScale;
    private bool _startVisible;

    private void Start()
    {
        _trans = target.transform;
        
        _startPos = _trans.position;
        _startRot = _trans.rotation;
        _startScale = _trans.localScale;
        _startVisible = target.activeSelf;
    }

    public IEnumerator Apply()
    {
        if (!_trans) Start();
        
        var startPos = _startPos;
        var startRot = _startRot;
        var startScale = _startScale;
        var startVisible = _startVisible;
        
        var endPos = _startPos + _trans.TransformDirection(positionOffset);
        var endRot = _startRot * Quaternion.Euler(rotationOffset);
        var endScale = _startScale + scaleOffset;
        var endVisible = visible;
        
        if (mode == Mode.BeginAsAnomaly)
        {
            (startPos, endPos) = (endPos, startPos);
            (startRot, endRot) = (endRot, startRot);
            (startScale, endScale) = (endScale, startScale);
            (startVisible, endVisible) = (endVisible, startVisible);
        }
        
        _trans.localPosition = startPos;
        _trans.localRotation = startRot;
        _trans.localScale = startScale;
        target.SetActive(startVisible);

        var time = 0f;
        var isActive = false;
        var isSeen = false;
        while (!isActive)
        {
            switch (trigger)
            {
                case Trigger.Default:
                    isActive = time > triggerDelay;
                    time += Time.deltaTime;
                    break;
                case Trigger.AfterSeen:
                    if (isSeen)
                    {
                        isActive = time > triggerDelay;
                        time += Time.deltaTime;
                    }
                    else isSeen = SeenByPlayer();
                    break;
                case Trigger.AfterUnseen:
                    if (isSeen)
                    {
                        if (!SeenByPlayer())
                        {
                            isActive = time > triggerDelay;
                            time += Time.deltaTime;
                        }
                    }
                    else isSeen = SeenByPlayer();
                    break;
            }
            
            yield return null;
        }
        
        Debug.Log("ANOMALY TRIGGERED");
        
        time = 0f;
        while (time < transitionTime)
        {
            var t = time / transitionTime;

            _trans.localPosition = Vector3.Lerp(startPos, endPos, t);
            _trans.localRotation = Quaternion.Slerp(startRot, endRot, t);
            _trans.localScale = Vector3.Lerp(startScale, endScale, t);

            time += Time.deltaTime;
            yield return null;
        }

        _trans.localPosition = endPos;
        _trans.localRotation = endRot;
        _trans.localScale = endScale;
        target.SetActive(endVisible);
    }

    private bool SeenByPlayer()
    {
        var cam = Camera.main;
        if (!cam) return false;
        
        var toObject = target.transform.position - cam.transform.position;
        if (toObject.magnitude > seenDistance) return false;
        
        var dot = Vector3.Dot(cam.transform.forward, toObject.normalized);
        return dot > Mathf.Cos(seenAngle * Mathf.Deg2Rad);
    }
}