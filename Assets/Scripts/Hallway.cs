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
                if (area.Equals(this)) return;
                
                Debug.Log("ENTERING HALLWAY: Creating section");
                GameManager.CreateSection(connector);
            };

            Action<Area> onExit = area =>
            {
                if (area.Equals(this)) return;
                
                Debug.Log("LEAVING HALLWAY: Deleting section");
                GameManager.DeleteSection(connector);
            };

            connector.OnPlayerEnter += onEnter;
            connector.OnPlayerExit += onExit;
            
            EnterHandlers.Add((connector, onEnter));
            ExitHandlers.Add((connector, onExit));
        });
    }
}