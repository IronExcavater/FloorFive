using System;
using UnityEngine;

public class Trigger : MonoBehaviour
{
    private Area _area;
    public Action<Connector> OnPlayerEnter;
    public Action<Connector> OnPlayerExit;

    private void Start()
    {
        _area = GetComponentInParent<Area>();
    }
    
    // private void OnTriggerEnter(Collider other)
    // {
    //     if (!other.CompareTag("Player")) return;
    //     OnPlayerEnter?.Invoke(ClosestConnector(other.transform.position));
    // }
    //
    // private void OnTriggerExit(Collider other)
    // {
    //     if (!other.CompareTag("Player")) return;
    //     OnPlayerExit?.Invoke(ClosestConnector(other.transform.position));
    // }
    //
    // private Connector ClosestConnector(Vector3 playerPosition) // TODO: Rework algorithm
    // {
    //     var axis = transform.forward;
    //
    //     // var axis = transform.forward;
    //     //
    //     // var playerToArea = playerPosition - _area.transform.position;
    //     // var triggerToArea = transform.position - _area.transform.position;
    //     //
    //     // var playerDistance = Vector3.Dot(playerToArea, axis);
    //     // var triggerDistance = Vector3.Dot(triggerToArea, axis);
    //     //
    //     // return Math.Abs(playerDistance) < Math.Abs(triggerDistance) ? _area : connection.area;
    // }
}