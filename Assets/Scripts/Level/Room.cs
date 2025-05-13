using System;
using System.Collections.Generic;
using System.Linq;
using Anomaly;
using Tools;
using Subtitle;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Level
{
    public class Room : MonoBehaviour
    {
        public int floorNumber;
        public SubtitleSequence subtitleSequence;
        
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
                remainingTime = duration;
                _remainingAnomalyGap = initialAnomalyGap;

                if (_status == State.Active)
                {
                    OnRoomActivated?.Invoke();
                    SubtitleUI.TriggerEvent(SubtitleEvent.OnRoomActivated);
                }

                if (_status == State.Complete)
                {
                    OnRoomCompleted?.Invoke();
                    SubtitleUI.TriggerEvent(SubtitleEvent.OnRoomCompleted);
                }
                if (_status != State.Active) _anomalies.ForEach(anomaly => anomaly.Active = false);
            }
        }

        private bool _mute;

        public bool Mute
        {
            get => _mute;
            set
            {
                _mute = value;
                _audioSources.ForEach(audioSource => audioSource.mute = _mute);
            }
        }

        public event Action<AnomalyBase> OnAnomalyTriggered;
        public event Action<AnomalyBase> OnAnomalyCompleted;
        public event Action OnRoomActivated;
        public event Action OnRoomCompleted;
        
        public float duration = 180f; // 3 minutes
        public float remainingTime;

        private ToolBase _roomTool;

        public bool IsToolEquipped => _roomTool == null || _roomTool.equipped;
        
        [Header("Anomaly Gap")]
        public float initialAnomalyGap = 3f;
        public AnimationCurve anomalyCurve = AnimationCurve.EaseInOut(0, 20, 1f, 10f);
        private float _remainingAnomalyGap;

        public List<AnomalyBase> _anomalies;
        private List<AudioSource> _audioSources;

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
            gameObject.tag = "Room";
            
            _anomalies = GetComponentsInChildren<AnomalyBase>().ToList();
            _roomTool = GetComponentInChildren<ToolBase>();
            
            _audioSources = GetComponentsInChildren<AudioSource>().ToList();
        }

        private void Start()
        {
            SubtitleUI.LoadSequence(subtitleSequence);
            SubtitleUI.TriggerEvent(SubtitleEvent.OnRoomLoaded);
        }

        private void Update()
        {
            HandleTriggering();
            HandleStress();
        }

        private void HandleTriggering()
        {
            if (Status != State.Active) return;
            
            remainingTime -= Time.deltaTime;
            _remainingAnomalyGap -= Time.deltaTime;
            
            if (remainingTime <= 0)
            {
                Status = State.Complete;
            }

            if (_remainingAnomalyGap <= 0)
            {
                TriggerAnomaly();
            }
        }

        private void TriggerAnomaly()
        {
            if (_anomalies.Count == 0) return;
            
            Camera camera = Camera.main;
            if (camera == null) return;
            
            Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(camera);

            List<AnomalyBase> validAnomalies = _anomalies.Where(anomaly =>
                !anomaly.Active &&
                !GeometryUtility.TestPlanesAABB(frustumPlanes, anomaly.GetNormalBounds()) &&
                !GeometryUtility.TestPlanesAABB(frustumPlanes, anomaly.GetAnomalousBounds())
            ).ToList();
            
            if (validAnomalies.Count == 0) return;
            
            int index = Random.Range(0, validAnomalies.Count);
            var anomaly = validAnomalies[index];
            anomaly.Active = true;
            OnAnomalyTriggered?.Invoke(anomaly);
            Debug.Log($"Triggered anomaly: {anomaly.name}");
            
            float t = Mathf.Clamp01(1 - remainingTime / duration);
            float slope = anomalyCurve.Evaluate(t);
            _remainingAnomalyGap = slope;
            
            switch (anomaly)
            {
                case CloakAnomaly _:
                    SubtitleUI.TriggerEvent(SubtitleEvent.OnCloakAnomalyTriggered);
                    break;
                case CameraAnomaly _:
                    SubtitleUI.TriggerEvent(SubtitleEvent.OnCameraAnomalyTriggered);
                    break;
                case not null:
                    SubtitleUI.TriggerEvent(SubtitleEvent.OnAnomalyTriggered);
                    break;
            }
        }

        public void AnomalyCompleted(AnomalyBase anomaly)
        {
            OnAnomalyCompleted?.Invoke(anomaly);
            
            switch (anomaly)
            {
                case CloakAnomaly _:
                    SubtitleUI.TriggerEvent(SubtitleEvent.OnCloakAnomalyCompleted);
                    break;
                case CameraAnomaly _:
                    SubtitleUI.TriggerEvent(SubtitleEvent.OnCameraAnomalyCompleted);
                    break;
                case not null:
                    SubtitleUI.TriggerEvent(SubtitleEvent.OnAnomalyCompleted);
                    break;
            }
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
                SubtitleUI.TriggerEvent(SubtitleEvent.OnPassedOut);
                Status = State.Ready;
            }
        }
    }
}