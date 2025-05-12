using System;
using UnityEngine;

namespace UI
{
    public enum SubtitleEvent
    {
        OnAnomalyTriggered,
        OnCloakAnomalyTriggered,
        OnCameraAnomalyTriggered,
        OnAnomalyCompleted,
        OnCloakAnomalyCompleted,
        OnCameraAnomalyCompleted,
        
        OnRoomActivated,
        OnRoomCompleted,
        
        OnToolAdded,
        OnGrabbed,
        OnPassedOut,
        
        OnElevatorOpened,
        OnElevatorClosed,
        OnElevatorRode,
        OnElevatorCrashed,
    }
    
    [Serializable]
    public class SubtitleStep
    {
        public SubtitleEvent subtitleEvent;
        [TextArea] public string text;
        public AudioClip audioClip;
        public float delay = 0f;
        public float duration = 5f;

        public bool hasSubStep = false;
        public float subStepDelay = 10f;
        [TextArea] public string subStepText;
        public AudioClip subStepAudioClip;
        public float subStepDuration = 5f;
    }
}