using System;
using System.Linq;
using UnityEngine;

public class Hallway : Area
{
    private void OnEnable()
    {
        connectors.ToList().ForEach(connector =>
        {
            Action<Area> onEnter = area =>
            {
                if (area.Equals(this))
                {
                    
                }
                else
                {
                    Debug.Log("ENTERING HALLWAY: Creating section");
                    GameManager.CreateSection(connector);
                }
            };

            Action<Area> onExit = area =>
            {
                if (area.Equals(this))
                {
                    
                }
                else
                {
                    Debug.Log("LEAVING HALLWAY: Deleting section");
                    GameManager.DeleteSection(GetOppositeConnector(connector).connection);
                }
            };

            connector.OnPlayerEnter += onEnter;
            connector.OnPlayerExit += onExit;
            
            _enterHandlers.Add((connector, onEnter));
            _exitHandlers.Add((connector, onExit));
        });
    }
}