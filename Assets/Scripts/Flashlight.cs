using UnityEngine;

public class Flashlight : MonoBehaviour, IInteractable
{
    public float power = 20;
    public float powerUsage = 1;
    public float maxIntensity = 3;
    public float minIntensity = 0.8f;
    public float maxFlicker = 0.2f;
    public float minFlicker = 0.05f;
    public float minCooldown = 0.05f;

    private Light _light;
    private float _flickerDuration;
    private float _flickerCooldown;
    
    [Header("Components")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Collider col;

    private void Awake()
    {
        _light = GetComponentInChildren<Light>();
        _light.enabled = false;
    }

    private void Update()
    {

        if (!_light.enabled) return;
        
        power = Mathf.Max(power - Time.deltaTime * powerUsage, 0);

        if (power < 15)
        {
            var intensity = Mathf.Lerp(minIntensity, maxIntensity, power / 15);

            var flickerChance = 0.2f + 0.8f * intensity;
            if (_flickerCooldown <= 0 && Random.value < flickerChance)
            {
                var rand = (1 + Random.value) / 2;
                _flickerDuration = Mathf.Lerp(minFlicker, maxFlicker, intensity) + rand;
                _flickerCooldown = _flickerDuration + minCooldown + intensity * rand;
            }
            
            _flickerDuration -= Time.deltaTime;
            _flickerCooldown -= Time.deltaTime;

            _light.intensity = _flickerDuration > 0 ? 0.6f : intensity;
        }
        else
        {
            _light.intensity = maxIntensity;
        }
    }

    private void OnFlashlight(bool value)
    {
        _light.enabled = value;
    }

    public void Interact(GameObject playerObject)
    {
        InputManager.OnFlashlight += OnFlashlight;
        
        rb.isKinematic = true;
        col.enabled = false;
        var player = playerObject.GetComponent<PlayerController>();
        player.SetFlashlight(this);
    }

    public void OnDisable()
    {
        InputManager.OnFlashlight -= OnFlashlight;
    }
}
