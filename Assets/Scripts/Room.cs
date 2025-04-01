using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public List<GameObject> objects;
    public List<Trigger> triggers;

    private Trigger _entryTrigger;
    private Anomaly _anomaly;
    private bool _isComplete;

    private bool HasAnomaly => _anomaly != null;

    private void Start()
    {
        triggers.ForEach(trigger =>
        {
            trigger.OnPlayerEnter += () =>
            {
                if (_entryTrigger == null) _entryTrigger = trigger;
                else
                {
                    if (!_isComplete)
                    {
                        GameController.RoomCompleted(_entryTrigger.Equals(trigger) && HasAnomaly
                                                     || !_entryTrigger.Equals(trigger) && !HasAnomaly);
                        _isComplete = true;
                    }
                }
                
            };
        });
        
        // 50% chance of anomaly
        if (Random.Range(0, 100) < 50 || objects.Count == 0) return;
        var anomalyObj = objects[Random.Range(0, objects.Count)];
        _anomaly = anomalyObj.GetComponent<Anomaly>();
        _anomaly.ApplyRandomAnomaly();
    }
}