using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Area : MonoBehaviour
{
    protected Connector[] Connectors;
    protected readonly List<(Connector connector, Action<Area> handler)> EnterHandlers = new();
    protected readonly List<(Connector connector, Action<Area> handler)> ExitHandlers = new();

    private void Awake()
    {
        Connectors = GetComponentsInChildren<Connector>();
    }
    
    public Connector GetOppositeConnector(Connector connector)
    {
        if (Connectors.Length != 2) return null;
        return connector.Equals(Connectors[0]) ? Connectors[1] : Connectors[0];
    }

    public Connector GetRandomConnector()
    {
        return Connectors[Random.Range(0, Connectors.Length)];
    }
    
    private void OnDisable()
    {
        foreach (var connector in Connectors) connector.Disconnect();
        
        foreach (var (connector, handler) in EnterHandlers) connector.OnAreaEnter -= handler;
        EnterHandlers.Clear();
        
        foreach (var (connector, handler) in ExitHandlers) connector.OnAreaExit -= handler;
        ExitHandlers.Clear();
    }
}