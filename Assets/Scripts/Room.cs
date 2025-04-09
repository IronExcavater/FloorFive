using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Room : Area
{
    public List<Anomaly> anomalies;

    private bool _isComplete;
    private int _numberOfAnomalies;
    
    public void Activate(Connector startConnector)
    {
        Debug.Log("ENTERING ROOM: Starting room");
        _isComplete = false;

        TriggerAnomalies();
        HandleTriggers(startConnector);
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
            
            Action<Area> onCloseExit = area =>
            {
                if (!area.Equals(hallway)) return;
                
                if (startConnector && !_isComplete && !startConnector.Equals(farConnector))
                {
                    Debug.Log("LEAVING ROOM: Finishing room (opposite side of start)");
                    _isComplete = true;
                    if (_numberOfAnomalies > 0) GameManager.Score++;
                    else GameManager.Score = 0;
                }
            };

            Action<Area> onFarExit = area =>
            {
                if (area.Equals(hallway)) return;

                if (startConnector && !_isComplete && startConnector.Equals(farConnector))
                {
                    Debug.Log("LEAVING ROOM: Finishing room (same side as start)");
                    _isComplete = true;
                    if (_numberOfAnomalies == 0) GameManager.Score++;
                    else GameManager.Score = 0;
                }
            };
            
            closeConnector.OnPlayerExit += onCloseExit;
            farConnector.OnPlayerExit += onFarExit;
            
            ExitHandlers.Add((closeConnector, onCloseExit));
            ExitHandlers.Add((farConnector, onFarExit));
        });
    }
}