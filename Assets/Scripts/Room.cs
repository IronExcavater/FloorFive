using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Room : Area
{
    public List<AnomalyGroup> anomalies;

    private bool _isComplete;
    private int _numberOfAnomalies;
    
    public void Activate(Connector startConnector)
    {
        _isComplete = false;

        TriggerAnomalies();
        HandleTriggers(startConnector);
        Debug.Log($"ENTERING ROOM: room has {_numberOfAnomalies} anomalies");
    }

    private void TriggerAnomalies()
    {
        _numberOfAnomalies = Random.Range(0, GetMaxAnomalies());
        var shuffled = anomalies.OrderBy(_ => Random.value).ToList();
        
        for (var i = 0; i < Mathf.Min(_numberOfAnomalies, shuffled.Count); i++)
        {
            shuffled[i].Trigger(this);
        }
    }

    private int GetMaxAnomalies()
    {
        var score = GameManager.Score;
        if (score <= 0) return 0;
        if (score <= 2) return 1;
        if (score <= 4) return 2;
        return 3;
    }

    private void HandleTriggers(Connector startConnector)
    {
        connectors.ToList().ForEach(connector =>
        {
            var closeConnector = connector.connection;
            var hallway = closeConnector.area;
            var farConnector = hallway.GetOppositeConnector(closeConnector);
            
            Action<Area> onCloseEnter = area =>
            {
                if (area.Equals(hallway)) return;
                
                if (startConnector && !_isComplete)
                {
                    _isComplete = true;
                    
                    var exitedSameSide = startConnector.Equals(farConnector);
                    var correctExit = exitedSameSide ? _numberOfAnomalies > 0 : _numberOfAnomalies == 0;
                    GameManager.Score = correctExit ? GameManager.Score + 1 : 0;
                    
                    Debug.Log($"EXITING ROOM: Anomalies: {_numberOfAnomalies}, Score: {GameManager.Score}");
                }
            };
            
            closeConnector.OnPlayerEnter += onCloseEnter;
            EnterHandlers.Add((closeConnector, onCloseEnter));
        });
    }
}