using System.Collections.Generic;

namespace Subtitle
{
    public enum SubtitleEvent
    {
        OnAnomalyTriggered,
        OnCloakAnomalyTriggered,
        OnCameraAnomalyTriggered,
        
        OnAnomalyCompleted,
        OnCloakAnomalyCompleted,
        OnCameraAnomalyCompleted,
        
        OnRoomLoaded,
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

    public static class SubtitleUtils
    {
        private static readonly Dictionary<SubtitleEvent, SubtitleEvent?> ParentMap = new()
        {
            { SubtitleEvent.OnCloakAnomalyTriggered, SubtitleEvent.OnAnomalyTriggered },
            { SubtitleEvent.OnCameraAnomalyTriggered, SubtitleEvent.OnAnomalyTriggered },

            { SubtitleEvent.OnCloakAnomalyCompleted, SubtitleEvent.OnAnomalyCompleted },
            { SubtitleEvent.OnCameraAnomalyCompleted, SubtitleEvent.OnAnomalyCompleted },
        };

        public static bool Matches(SubtitleEvent stepEvent, SubtitleEvent triggeredEvent)
        {
            if (stepEvent == triggeredEvent) return true;

            var current = stepEvent;
            while (ParentMap.TryGetValue(current, out var parent))
            {
                if (parent == triggeredEvent) return true;
                current = parent.Value;
            }

            return false;
        }
    }
}