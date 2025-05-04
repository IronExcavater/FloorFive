using UnityEngine;

namespace Level
{
    public class Room : MonoBehaviour
    {
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

        public void Update()
        {
            if (Active)
            {
                _remainingTime -= Time.deltaTime;
            }
            
            if (_remainingTime <= 0)
            {
                Active = false;
            }
        }
    }
}