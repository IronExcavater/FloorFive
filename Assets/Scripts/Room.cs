using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public List<GameObject> objects;
    public List<Trigger> triggers;

    private Trigger _entryTrigger;
    private Anomaly _anomaly;

    private bool HasAnomaly => _anomaly != null;

    private void Start()
    {
        // 50% chance of anomaly
        if (Random.Range(0, 100) < 50 || objects.Count == 0) return;
        var anomalyObj = objects[Random.Range(0, objects.Count)];
        _anomaly = anomalyObj.GetComponent<Anomaly>();
        _anomaly.ApplyRandomAnomaly();

        triggers.ForEach(trigger =>
        {
            trigger.OnPlayerEnter += () =>
            {
                if (_entryTrigger == null) _entryTrigger = trigger;
                else
                {
                    GameController.RoomCompleted(_entryTrigger.Equals(trigger) && !HasAnomaly
                                                 || !_entryTrigger.Equals(trigger) && HasAnomaly);
                }
                
            };
        });
    }
}