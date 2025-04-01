using System;
using UnityEngine;

public class Trigger : MonoBehaviour
{
    public Action OnPlayerEnter;
    
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        OnPlayerEnter?.Invoke();
    }
}