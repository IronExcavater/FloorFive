using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Anomaly
{
    public List<AnomalyTarget> targets;

    public void Trigger(MonoBehaviour executor)
    {
        foreach (var target in targets)
        {
            executor.StartCoroutine(target.Apply());
        }
    }
}