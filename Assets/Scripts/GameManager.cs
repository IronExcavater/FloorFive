using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    public List<Room> roomPrefab;
    public List<Hallway> hallwayPrefab;
    public Checkpoint checkpointPrefab;
    public Transform levelTrans;
    
    private ObjectPool<Room> _roomPool;
    private ObjectPool<Hallway> _hallwayPool;
    private ObjectPool<Checkpoint> _checkpointPool;
    
    private int _score;
    private bool _canCheckpoint;
    public static Action<int> OnScoreChange;
    public static int Score
    {
        get => _instance._score;
        set
        {
            if (_instance._score == value) return;
            
            _instance._score = value;
            _instance._canCheckpoint = true;
            OnScoreChange?.Invoke(value);
        }
    }
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(this);

            _roomPool = new ObjectPool<Room>(roomPrefab, 2, levelTrans);
            _hallwayPool = new ObjectPool<Hallway>(hallwayPrefab, 3, levelTrans);
            _checkpointPool = new ObjectPool<Checkpoint>(checkpointPrefab, 1, levelTrans);
        }
        else Destroy(gameObject);
    }

    public static void CreateSection(Connector connector)
    {
        var oppositeConnector = connector.area.GetOppositeConnector(connector);
        var isCheckpoint = _instance._score % 3 == 0 && _instance._canCheckpoint;
        if (isCheckpoint) _instance._canCheckpoint = false;
        
        Area area = isCheckpoint ? _instance._checkpointPool.Get() : _instance._roomPool.Get();
        var areaConnector = area.GetRandomConnector();
        ConnectConnectors(area.transform, areaConnector.transform, oppositeConnector.transform);
        oppositeConnector.Connect(areaConnector);
        var otherAreaConnector = area.GetOppositeConnector(areaConnector);

        var hallway = _instance._hallwayPool.Get();
        var hallwayConnector = hallway.GetRandomConnector();
        ConnectConnectors(hallway.transform, hallwayConnector.transform, otherAreaConnector.transform);
        otherAreaConnector.Connect(hallwayConnector);
        
        (area as Room)?.Activate(connector);
    }

    public static void DeleteSection(Connector connector)
    {
        var oppositeConnector = connector.area.GetOppositeConnector(connector).connection;
        
        var area = oppositeConnector?.area;
        var hallway = area?.GetOppositeConnector(oppositeConnector)?.connection?.area as Hallway;

        switch (area)
        {
            case Room room:
                _instance._roomPool.Release(room);
                break;
            case Checkpoint checkpoint:
                _instance._checkpointPool.Release(checkpoint);
                break;
            case not null:
                area.gameObject.SetActive(false);
                break;
        }
        
        if (hallway) _instance._hallwayPool.Release(hallway);
    }

    private static void ConnectConnectors(Transform newArea, Transform newConnector, Transform existingConnector)
    {
        var toTarget = Quaternion.LookRotation(-existingConnector.forward, existingConnector.up);
        var rotOffset = toTarget * Quaternion.Inverse(newConnector.rotation);
        
        newArea.rotation = rotOffset * newArea.rotation;
        newArea.position = existingConnector.position - (newConnector.position - newArea.position);
    }
}