using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Room : Area
{
    public List<Anomaly> anomalies;
    
    public void Activate(Connector startConnector)
    {
        Debug.Log("ENTERING ROOM: Starting room");
        var isComplete = false;
        
        connectors.ToList().ForEach(connector =>
        {
            var closeConnector = connector.connection;
            var hallway = closeConnector.area;
            var farConnector = hallway.GetOppositeConnector(closeConnector);
            
            Action<Area> onCloseExit = area =>
            {
                if (!area.Equals(hallway)) return;
                
                if (startConnector && !isComplete && !startConnector.Equals(farConnector))
                {
                    Debug.Log("LEAVING ROOM: Finishing room (opposite side of start)");
                    isComplete = true;
                }
            };

            Action<Area> onFarExit = area =>
            {
                if (area.Equals(hallway)) return;

                if (startConnector && !isComplete && startConnector.Equals(farConnector))
                {
                    Debug.Log("LEAVING ROOM: Finishing room (same side as start)");
                    isComplete = true;
                }
            };
            
            closeConnector.OnPlayerExit += onCloseExit;
            farConnector.OnPlayerExit += onFarExit;
            
            _exitHandlers.Add((closeConnector, onCloseExit));
            _exitHandlers.Add((farConnector, onFarExit));
        });
    }
}