using Player;
using UnityEngine;

namespace Tools
{
    public class Flashlight : ToolBase
    {
        [Header("Flashlight")]
        public float minIntensity = 0.8f;
        public float maxIntensity = 3;
        public float minChance = 0.05f;
        public float maxChance = 0.4f;
        public float minDuration = 0.05f;
        public float maxDuration = 0.2f;
        public float minCooldown = 0.05f;

        private Light _light;
        private float _flickerDuration;
        private float _flickerCooldown;

        protected override void Awake()
        {
            base.Awake();
            _light = GetComponentInChildren<Light>();
        }

        protected override void Update()
        {
            base.Update();
            
            if (!_light.enabled || !_equipped) return;
            
            float intensity = Mathf.Lerp(minIntensity, maxIntensity, 1 - _stress);

            float flickerChance = Mathf.Lerp(minChance, maxChance, _stress); // Higher stress → more flickering
            if (_flickerCooldown <= 0 && Random.value < flickerChance)
            {
                float rand = (1 + Random.value) / 2f;
                _flickerDuration = Mathf.Lerp(minDuration, maxDuration, _stress) + rand;
                _flickerCooldown = _flickerDuration + minCooldown + (1 - _stress) * rand;
            }

            _flickerDuration -= Time.deltaTime;
            _flickerCooldown -= Time.deltaTime;

            _light.intensity = _flickerDuration > 0 ? 0.6f : intensity;
        }
        
        protected override void Use(PlayerController player)
        {
            Debug.Log("Flashlight used");
            _light.enabled = !_light.enabled;
        }
    }
}