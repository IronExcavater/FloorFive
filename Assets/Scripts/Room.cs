using System.Linq;
using UnityEngine;

public class Room : Area
{
    
    private void Start()
    {
        Connector startConnector = null;
        var isComplete = false;
        
        connectors.ToList().ForEach(connector =>
        {
            var closeConnector = connector.connection;
            var hallway = closeConnector.area;
            var farConnector = hallway.GetOppositeConnector(closeConnector);
            
            closeConnector.OnPlayerExit += area =>
            {
                if (!area.Equals(hallway)) return;
                
                if (startConnector && !isComplete && !startConnector.Equals(farConnector))
                {
                    Debug.Log("LEAVING ROOM: Finishing room (opposite side of start)");
                    isComplete = true;
                }
            };

            farConnector.OnPlayerExit += area =>
            {
                if (area.Equals(hallway)) return;

                if (!startConnector)
                {
                    Debug.Log("ENTERING ROOM: Starting room");
                    startConnector = closeConnector;
                }
                else if (!isComplete && startConnector.Equals(farConnector))
                {
                    Debug.Log("LEAVING ROOM: Finishing room (same side as start)");
                    isComplete = true;
                }
            };
        });
    }
}