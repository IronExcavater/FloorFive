using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Area : MonoBehaviour
{
    [HideInInspector] public Connector[] connectors;

    public bool _isViewed;
    public Action<bool> OnViewChange;
    public bool IsViewed
    {
        get => _isViewed;
        private set
        {
            if (_isViewed == value) return;
            _isViewed = value;
            OnViewChange?.Invoke(value);
        }
    }

    protected virtual void Awake()
    {
        connectors = GetComponentsInChildren<Connector>();

        OnViewChange += isViewed =>
        {
            if (!isViewed) GameManager.DeleteArea(this);
        };
    }

    private void Update()
    {
        var camera = Camera.main;
        if (camera == null) return;
        
        IsViewed = connectors.Any(connector =>
        {
            var isViewed = connector.IsViewed;
            if (isViewed && connector.connection == null) GameManager.CreateArea(this, connector);
            return isViewed;
        }) || Vector3.Distance(camera.transform.position, transform.position) < 10;
    }
    
    public Connector GetRandomConnector()
    {
        return connectors[Random.Range(0, connectors.Length)];
    }

    public Connector GetClosestConnector()
    {
        var camera = Camera.main;
        if (camera == null) return connectors[0];
        
        return connectors.OrderBy(connector => Vector3.Distance(connector.transform.position, camera.transform.position)).First();
    }
}