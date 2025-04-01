using System.Linq;
using UnityEngine;

public class Room : Area
{
    private void Start()
    {
        connectors.ToList().ForEach(connector =>
        {
            connector.OnPlayerEnter += area =>
            {
                
            };

            connector.OnPlayerExit += area =>
            {
                
            };
        });
    }
}