using System;
using UnityEngine;

public class Connector : MonoBehaviour
{
    [HideInInspector] public Area area;
    private Trigger _trigger;
    public Connector connection;
    
    public Action<Area> OnAreaEnter;
    public Action<Area> OnAreaExit;

    private Action<Collider> _onPlayerEnter;
    private Action<Collider> _onPlayerExit;

    private void Awake()
    {
        area = GetComponentInParent<Area>();
        _trigger = GetComponentInChildren<Trigger>();

        _onPlayerEnter = other => OnAreaEnter?.Invoke(GetActiveArea(other.transform.position));
        _onPlayerExit = other => OnAreaExit?.Invoke(GetActiveArea(other.transform.position));
    }

    private void OnEnable()
    {
        _trigger.OnPlayerEnter += _onPlayerEnter;
        _trigger.OnPlayerExit += _onPlayerExit;
    }

    private void OnDisable()
    {
        _trigger.OnPlayerEnter -= _onPlayerEnter;
        _trigger.OnPlayerExit -= _onPlayerExit;
    }

    private Area GetActiveArea(Vector3 playerPosition)
    {
        var axis = _trigger.transform.forward;
        
        var playerToArea = playerPosition - area.transform.position;
        var triggerToArea = _trigger.transform.position - area.transform.position;
        
        var playerDistance = Vector3.Dot(playerToArea, axis);
        var triggerDistance = Vector3.Dot(triggerToArea, axis);
        
        Debug.Log($"playerToArea: {playerDistance}, triggerToArea: {triggerDistance}");
        return Math.Abs(playerDistance) < Math.Abs(triggerDistance) ? area : connection.area;
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