using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    public List<Room> roomPrefab;
    public List<Checkpoint> checkpointPrefab;
    public Transform levelTrans;
    
    private ObjectPool<Room> _roomPool;
    private ObjectPool<Checkpoint> _checkpointPool;
    
    private static int _score;
    public static Action<int> OnScoreChange;
    public static int Score
    {
        get => _score;
        set
        {
            if (_score == value) return;
            
            _score = value;
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
            _checkpointPool = new ObjectPool<Checkpoint>(checkpointPrefab, 2, levelTrans);
        }
        else Destroy(gameObject);
    }

    public static void CreateArea(Area area, Connector connector)
    {
        var isCheckpoint = area is Checkpoint;
        
        Area newArea = isCheckpoint ? _instance._roomPool.Get() : _instance._checkpointPool.Get();
        Debug.Log($"CREATING AREA: {newArea.name}");
        var newConnector = newArea.GetRandomConnector();
            
        Connect(newArea.transform, newConnector.transform, connector.transform);
        connector.Connect(newConnector);
    }

    public static void DeleteArea(Area area)
    {
        Debug.Log($"DELETING AREA: {area.name}");
        
        switch (area)
        {
            case Room room:
                _instance._roomPool.Release(room);
                break;
            case Checkpoint checkpoint:
                _instance._checkpointPool.Release(checkpoint);
                break;
            default:
                area.gameObject.SetActive(false);
                break;
        }

        foreach (var connector in area.connectors)
        {
            connector.Disconnect();
        }
    }

    private static void Connect(Transform newArea, Transform newConnector, Transform existingConnector)
    {
        var toTarget = Quaternion.LookRotation(-existingConnector.forward, existingConnector.up);
        var rotOffset = toTarget * Quaternion.Inverse(newConnector.rotation);
        
        newArea.rotation = rotOffset * newArea.rotation;
        newArea.position = existingConnector.position - (newConnector.position - newArea.position);
    }
}