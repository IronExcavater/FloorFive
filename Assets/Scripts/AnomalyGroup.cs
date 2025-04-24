using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AnomalyGroup
{
    public List<Anomaly> anomalies;

    public void Trigger(MonoBehaviour executor)
    {
        foreach (var target in anomalies)
        {
            executor.StartCoroutine(target.Apply());
        }
    }
}