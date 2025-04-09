using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Area : MonoBehaviour
{
    public Connector[] connectors;
    protected readonly List<(Connector connector, Action<Area> handler)> EnterHandlers = new();
    protected readonly List<(Connector connector, Action<Area> handler)> ExitHandlers = new();

    public Connector GetOppositeConnector(Connector connector)
    {
        if (connectors.Length != 2) return null;
        return connector.Equals(connectors[0]) ? connectors[1] : connectors[0];
    }

    public Connector GetRandomConnector()
    {
        return connectors[Random.Range(0, connectors.Length)];
    }
    
    private void OnDisable()
    {
        foreach (var connector in connectors) connector.Disconnect();
        
        foreach (var (connector, handler) in EnterHandlers) connector.OnPlayerEnter -= handler;
        EnterHandlers.Clear();
        
        foreach (var (connector, handler) in ExitHandlers) connector.OnPlayerExit -= handler;
        ExitHandlers.Clear();
    }
}