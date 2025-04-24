using UnityEngine;
﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Area : MonoBehaviour
{
    public List<Connector> connectors;

    private bool _isViewed;
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
        connectors = GetComponentsInChildren<Connector>().ToList();

        OnViewChange += isViewed =>
        {
            if (!isViewed) GameManager.DeleteArea(this);
        };
    }

    private void Update()
    {
        var camera = Camera.main;
        if (camera == null) return;

        var roomViewed = false;
        connectors.ForEach(connector =>
        {
            if (!connector.IsViewed) return;
            if (connector.connection == null) GameManager.CreateArea(this, connector, _instance, _instance);
            roomViewed = true;
        });

        IsViewed = roomViewed || Vector3.Distance(camera.transform.position, transform.position) < 10;
    }
    
    public Connector GetRandomConnector()
    {
        return connectors[Random.Range(0, connectors.Count)];
    }

    public Connector GetClosestConnector()
    {
        var camera = Camera.main;
        if (camera == null) return connectors[0];
        
        return connectors.OrderBy(connector => Vector3.Distance(connector.transform.position, camera.transform.position)).First();
    }
}
