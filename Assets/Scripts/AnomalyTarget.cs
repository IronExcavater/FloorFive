using System.Collections;
using UnityEngine;

[System.Serializable]
public class AnomalyTarget
{
    public GameObject gameObject;

    private Transform trans;
    [HideInInspector] public MeshRenderer rend;

    public Vector3 positionOffset = Vector3.zero;
    public Vector3 rotationOffset = Vector3.zero;
    public Vector3 scaleOffset = Vector3.zero;
    public Color targetColor = Color.white;
    public float transitionTime;
    public bool visible = true;

    private Vector3 startPos;
    private Quaternion startRot;
    private Vector3 startScale;
    private Color startColor;

    private void Start()
    {
        trans = gameObject.transform;
        rend = gameObject.GetComponent<MeshRenderer>();
        
        startPos = trans.position;
        startRot = trans.rotation;
        startScale = trans.localScale;
        startColor = rend ? rend.material.color : Color.white;
    }

    public void Reset()
    {
        
    }

    public IEnumerator Apply()
    {
        var endPos = startPos + trans.TransformDirection(positionOffset);
        var endRot = startRot * Quaternion.Euler(rotationOffset);
        var endScale = startScale + scaleOffset;
        var endColor = targetColor;

        float time = 0;
        while (time < transitionTime)
        {
            var t = time / transitionTime;

            trans.position = Vector3.Lerp(startPos, endPos, t);
            trans.rotation = Quaternion.Slerp(startRot, endRot, t);
            trans.localScale = Vector3.Lerp(startScale, endScale, t);

            if (rend) rend.material.color = Color.Lerp(startColor, endColor, t);

            time += Time.deltaTime;
            yield return null;
        }

        trans.position = endPos;
        trans.rotation = endRot;
        trans.localScale = endScale;
        if (rend) rend.material.color = endColor;
        gameObject.SetActive(visible);
    }
}