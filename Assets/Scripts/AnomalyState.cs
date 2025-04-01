using UnityEngine;

[System.Serializable]
public class AnomalyState
{
    public string name;
    public Vector3 positionOffset;
    public Vector3 rotationOffset;
    public Vector3 scaleMultiplier = Vector3.one;
    public Color color = Color.white;
    public bool hidden = false;
}