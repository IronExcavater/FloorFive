using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Area : MonoBehaviour
{
    public Connector[] connectors;
    protected readonly List<(Connector connector, Action<Area> handler)> _enterHandlers = new();
    protected readonly List<(Connector connector, Action<Area> handler)> _exitHandlers = new();

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
        
        foreach (var (connector, handler) in _enterHandlers) connector.OnPlayerEnter -= handler;
        _enterHandlers.Clear();
        
        foreach (var (connector, handler) in _exitHandlers) connector.OnPlayerExit -= handler;
        _exitHandlers.Clear();
    }
}