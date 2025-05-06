using System.Collections.Generic;
using Anomaly;
using UnityEngine;

namespace Level
{
    public class Room : MonoBehaviour
    {
        public int floorNumber;
        
        private bool _active = true;
        public bool Active
        {
            get => _active;
            private set
            {
                if (_active == value) return;
                _active = value;
                
                if (_active)
                {
                    _remainingTime = duration;
                }
            }
        }
        
        public float duration = 180f; // 3 minutes
        private float _remainingTime;

        public float anomalyGap = 10f;
        private float _remainingAnomalyGap;

        [SerializeField] private List<AnomalyBase> anomalies;

        public void Update()
        {
            if (Active)
            {
                _remainingTime -= Time.deltaTime;
                _remainingAnomalyGap -= Time.deltaTime;
            }
            
            if (_remainingTime <= 0)
            {
                Active = false;
            }

            if (_remainingAnomalyGap <= 0)
            {
                TriggerAnomaly();
                
                float t = Mathf.Clamp01(1 - _remainingTime / duration);
                float slope = Mathf.Lerp(anomalyGap, 1, t * t);
                _remainingAnomalyGap = slope;
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