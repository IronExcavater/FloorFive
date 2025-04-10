using System;
using UnityEngine;

public class Trigger : MonoBehaviour
{
    public Action<Collider> OnPlayerEnter;
    public Action<Collider> OnPlayerExit;
    
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        OnPlayerEnter?.Invoke(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        OnPlayerExit?.Invoke(other);
    }
}