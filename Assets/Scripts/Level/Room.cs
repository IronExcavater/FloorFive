using System;
using System.Collections.Generic;
using System.Linq;
using Anomaly;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Level
{
    public class Room : MonoBehaviour
    {
        public int floorNumber;
        
        private State _status = State.Ready;

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
                _remainingAnomalyGap = 3f;
            }
        }
        
        public float duration = 180f; // 3 minutes
        private float _remainingTime;
        
        [Header("Anomaly Gap")]
        public AnimationCurve anomalyCurve = AnimationCurve.EaseInOut(0, 20, 1f, 10f);
        private float _remainingAnomalyGap;

        private List<AnomalyBase> _anomalies;
        
        [Header("Player Stress")]
        public int maxActiveAnomalies = 5;
        public float stress = 0;
        public AnimationCurve stressCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        public event Action<float> OnStressed;
        public event Action OnPassedOut;
        

        private void Awake()
        {
            _anomalies = GetComponentsInChildren<AnomalyBase>().ToList();
        }

        private void Update()
        {
            if (Status != State.Active) return;
            
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
                float slope = anomalyCurve.Evaluate(t);
                _remainingAnomalyGap = slope;
            }
            
            HandleStress();
        }

        private void TriggerAnomaly()
        {
            if (_anomalies.Count == 0) return;
            
            int index = Random.Range(0, _anomalies.Count);
            _anomalies[index].Active = true;
            Debug.Log($"Triggered anomaly: {_anomalies[index].name}");
        }

        private void HandleStress()
        {
            float totalActiveTime = _anomalies.Sum(anomaly => anomaly.activeTime);
            float stressT = Mathf.Clamp01(totalActiveTime / (maxActiveAnomalies * duration));
            stress = Mathf.MoveTowards(stress, stressT, Time.deltaTime * 0.1f);
            
            OnStressed?.Invoke(stressCurve.Evaluate(stress));
            if (stress >= 1) OnPassedOut?.Invoke();
        }
    }
}