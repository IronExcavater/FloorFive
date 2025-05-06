using System.Collections.Generic;
using Anomaly;
using UnityEngine;

namespace Level
{
    public class Room : MonoBehaviour
    {
        public int floorNumber;
        
        [SerializeField] private State _status = State.Ready;

        public enum State
        {
            Ready,
            Active,
            Complete
        }
        public State Status
        {
            get => _status;
            set
            {
                if (_status == value) return;
                _status = value;
                _remainingTime = duration;
                _remainingAnomalyGap = anomalyGap;
            }
        }
        
        public float duration = 180f; // 3 minutes
        private float _remainingTime;

        public float anomalyGap = 10f;
        private float _remainingAnomalyGap;

        [SerializeField] private List<AnomalyBase> anomalies;

        public void Update()
        {
            if (Status == State.Active)
            {
                _remainingTime -= Time.deltaTime;
                _remainingAnomalyGap -= Time.deltaTime;
                
                if (_remainingTime <= 0)
                {
                    Status = State.Complete;
                }

                if (_remainingAnomalyGap <= 0)
                {
                    TriggerAnomaly();
                
                    float t = Mathf.Clamp01(1 - _remainingTime / duration);
                    float slope = Mathf.Lerp(anomalyGap, 1, t * t);
                    _remainingAnomalyGap = slope;
                }
            }
        }

        private void TriggerAnomaly()
        {
            if (anomalies.Count == 0) return;
            
            int index = Random.Range(0, anomalies.Count);
            anomalies[index].Active = true;
        }
    }
}