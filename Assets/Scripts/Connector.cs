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
        var playerToArea = Vector3.Distance(playerPosition, area.transform.position);
        var selfToArea = Vector3.Distance(transform.position, area.transform.position);

        return playerToArea < selfToArea ? area : connection.area;
    }

    public void Connect(Connector connector)
    {
        connection = connector;
        connector.connection = this;
    }

    public void Disconnect()
    {
        connection.connection = null;
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