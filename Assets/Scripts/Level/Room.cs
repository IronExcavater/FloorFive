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
                
                if (_status == State.Complete) _anomalies.ForEach(anomaly => anomaly.Active = false);
            }
        }
        
        public float duration = 180f; // 3 minutes
        private float _remainingTime;
        
        [Header("Anomaly Gap")]
        public AnimationCurve anomalyCurve = AnimationCurve.EaseInOut(0, 20, 1f, 10f);
        private float _remainingAnomalyGap;

        private List<AnomalyBase> _anomalies;

        [Header("Player Stress")]
        public float maxActiveTime = 60;
        public int maxActiveCount = 5;
        private float _stress = 0;
        public AnimationCurve stressCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        public event Action<float> OnStressed;
        public event Action OnPassedOut;

        private float _normalisedTime;
        private float _normalisedCount;
        

        private void Awake()
        {
            _anomalies = GetComponentsInChildren<AnomalyBase>().ToList();
        }

        private void Update()
        {

            HandleTriggering();
            HandleStress();
        }

        private void HandleTriggering()
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
            float activeCount = 0;
            float totalActiveTime = 0;
            _anomalies.ForEach(anomaly =>
            {
                totalActiveTime += anomaly.activeTime;
                activeCount += anomaly.Active ? 1 : 0;
            });
            
            _normalisedTime = Mathf.Clamp01(totalActiveTime / maxActiveTime);
            _normalisedCount = Mathf.Clamp01(activeCount / maxActiveCount);
            
            float stressT = Mathf.Clamp01((_normalisedTime + _normalisedCount) * 0.5f);
            _stress = Mathf.MoveTowards(_stress, stressT, Time.deltaTime * 0.2f);
            
            OnStressed?.Invoke(stressCurve.Evaluate(_stress));
            if (Status == State.Active && _stress >= 1)
            {
                OnPassedOut?.Invoke();
                Status = State.Ready;
            }
        }
    }
}