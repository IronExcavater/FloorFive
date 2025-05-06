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
                
            }
        }

        private void TriggerAnomaly()
        {
            foreach (AnomalyBase anomaly in anomalies)
            {
                
            }
        }
    }
}