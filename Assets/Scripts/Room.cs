<<<<<<< HEAD

﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Room : Area
{
    public List<AnomalyGroup> anomalies;

    private bool _isActive;
    private int _numberOfAnomalies;

    protected override void Awake()
    {
        base.Awake();
        
        Connector startConnector = null;
        OnViewChange += isViewed =>
        {
            if (isViewed && !_isActive) // Start room
            {
                _isActive = true;
                startConnector = GetClosestConnector();
                TriggerAnomalies();
                Debug.Log($"ENTERING ROOM: room has {_numberOfAnomalies} anomalies");
            }

            if (!isViewed && _isActive) // End room
            {
                _isActive = false;
                var exitedSameSide = startConnector == GetClosestConnector();
                var correctExit = exitedSameSide ? _numberOfAnomalies > 0 : _numberOfAnomalies == 0;
                GameManager.Score = correctExit ? GameManager.Score + 1 : 0;
                
                Debug.Log($"EXITING ROOM: changed score to {GameManager.Score}");
            }
        };
    }

    private void TriggerAnomalies()
    {
        _numberOfAnomalies = Random.Range(0, GetMaxAnomalies());
        var shuffled = anomalies.OrderBy(_ => Random.value).ToList();
        
        for (var i = 0; i < Mathf.Min(_numberOfAnomalies, shuffled.Count); i++)
        {
            shuffled[i].Trigger(this);
        }
    }

    private int GetMaxAnomalies()
    {
        var score = GameManager.Score;
        if (score <= 0) return 0;
        if (score <= 2) return 1;
        if (score <= 4) return 2;
        return 3;
>>>>>>> feature/MenuAndUI
    }
}