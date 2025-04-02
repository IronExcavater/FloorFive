using System;
using UnityEngine;

public class Connector : MonoBehaviour
{
    public Area area;
    public Connector connection;
    
    public Action<Area> OnPlayerEnter;
    public Action<Area> OnPlayerExit;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        OnPlayerEnter?.Invoke(GetActiveArea(other.transform.position));
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        OnPlayerExit?.Invoke(GetActiveArea(other.transform.position));
    }

    private Area GetActiveArea(Vector3 playerPosition)
    {
        var axis = transform.forward;
        
        var playerToArea = playerPosition - area.transform.position;
        var selfToArea = transform.position - area.transform.position;
        
        var playerDistance = Vector3.Dot(playerToArea, axis);
        var selfDistance = Vector3.Dot(selfToArea, axis);
        
        return Math.Abs(playerDistance) < Math.Abs(selfDistance) ? area : connection.area;
    }

    public void Connect(Connector connector)
    {
        connection = connector;
        connector.connection = this;
    }

    public void Disconnect()
    {
        if (connection) connection.connection = null;
        connection = null;
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;

        var start = transform.position;
        var direction = transform.forward;
        var end = start + direction * 0.8f;

        Gizmos.DrawLine(start, end);

        var arrowRight = Quaternion.Euler(0, 150, 0) * direction;
        var arrowLeft = Quaternion.Euler(0, -150, 0) * direction;

        Gizmos.DrawLine(end, end + arrowRight * 0.3f);
        Gizmos.DrawLine(end, end + arrowLeft * 0.3f);
    }
}