using System.Collections;
using UnityEngine;

[System.Serializable]
public class AnomalyTarget
{
    public GameObject gameObject;

    private Transform trans;

    public Vector3 positionOffset = Vector3.zero;
    public Vector3 rotationOffset = Vector3.zero;
    public Vector3 scaleOffset = Vector3.zero;
    public float transitionTime = 2;
    public bool visible = true;

    private Vector3 startPos;
    private Quaternion startRot;
    private Vector3 startScale;
    private bool startVisible;

    private void Start()
    {
        trans = gameObject.transform;
        
        startPos = trans.position;
        startRot = trans.rotation;
        startScale = trans.localScale;
        startVisible = gameObject.activeSelf;
    }

    public void Reset()
    {
        trans.position = startPos;
        trans.rotation = startRot;
        trans.localScale = startScale;
        gameObject.SetActive(startVisible);
    }

    public IEnumerator Apply()
    {
        if (!trans) Start();
        else Reset();
        
        var endPos = startPos + trans.TransformDirection(positionOffset);
        var endRot = startRot * Quaternion.Euler(rotationOffset);
        var endScale = startScale + scaleOffset;

        float time = 0;
        while (time < transitionTime)
        {
            var t = time / transitionTime;

            trans.position = Vector3.Lerp(startPos, endPos, t);
            trans.rotation = Quaternion.Slerp(startRot, endRot, t);
            trans.localScale = Vector3.Lerp(startScale, endScale, t);

            time += Time.deltaTime;
            yield return null;
        }

        trans.position = endPos;
        trans.rotation = endRot;
        trans.localScale = endScale;
        gameObject.SetActive(visible);
    }
}