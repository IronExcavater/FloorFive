using System.Linq;
using UnityEngine;

public class Hallway : Area
{
    private void Start()
    {
        connectors.ToList().ForEach(connector =>
        {
            connector.OnPlayerEnter += area =>
            {
                if (area.Equals(this))
                {
                    
                }
                else
                {
                    Debug.Log("ENTERING HALLWAY: Creating section");
                    GameManager.CreateSection(GetOppositeConnector(connector));
                }
            };

            connector.OnPlayerExit += area =>
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
        });
    }
}