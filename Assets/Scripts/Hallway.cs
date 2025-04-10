using System;
using System.Linq;
using UnityEngine;

public class Hallway : Area
{
    private void OnEnable()
    {
        Connectors.ToList().ForEach(connector =>
        {
            Action<Area> onExit = area =>
            {
                if (area.Equals(this))
                {
                    Debug.Log("ENTERING HALLWAY: Creating section");
                    GameManager.CreateSection(connector);
                }
                else
                {
                    Debug.Log("LEAVING HALLWAY: Deleting section");
                    GameManager.DeleteSection(connector);
                }
            };

            connector.OnAreaExit += onExit;
            ExitHandlers.Add((connector, onExit));
        });
    }
}