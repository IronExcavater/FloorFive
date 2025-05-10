using Load;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Level
{
    [RequireComponent(typeof(AudioSource))]
    public class Stress : MonoBehaviour
    {
        [Header("Stress Effects")]
        [SerializeField] private Volume postProcessingVolume;
        [SerializeField] private float maxVignette = 0.45f;
        [SerializeField] private float maxMotionBlur = 0.3f;
        [SerializeField] private float maxChromaticAberration = 0.4f;
        [SerializeField] private float minSaturation = -60;
        [SerializeField] private float maxFilmGrain = 0.3f;
        [SerializeField] private float maxHeartbeatVolume = 0.8f;
        [SerializeField] private float maxHeartbeatPitch = 1.4f;
        [SerializeField] private float minCameraFov = 50;
        
        private Vignette _vignette;
        private MotionBlur _motionBlur;
        private ChromaticAberration _chromaticAberration;
        private ColorAdjustments _colorAdjustments;
        private FilmGrain _filmGrain;
        
        private Camera _mainCamera;
        private float _baseFov;
        private AudioSource _audioSource;
        
        private Room _currentRoom;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            
            postProcessingVolume.profile.TryGet(out _vignette);
            postProcessingVolume.profile.TryGet(out _motionBlur);
            postProcessingVolume.profile.TryGet(out _chromaticAberration);
            postProcessingVolume.profile.TryGet(out _colorAdjustments);
            postProcessingVolume.profile.TryGet(out _filmGrain);
            
            _mainCamera = Camera.main;
            if (_mainCamera != null) _baseFov = _mainCamera.fieldOfView;
            
            _currentRoom = GameObject.FindGameObjectWithTag("Room")?.GetComponent<Room>();
            SubscribeToRoom();
        }
        
        private void OnEnable()
        {
            LoadManager.OnSceneLoaded += OnSceneLoaded;
            SubscribeToRoom();
        }

        private void OnDisable()
        {
            LoadManager.OnSceneLoaded -= OnSceneLoaded;
            UnsubscribeFromRoom();
        }
        
        private void OnSceneLoaded(Scene scene)
        {
            UnsubscribeFromRoom();
            _currentRoom = GameObject.FindGameObjectWithTag("Room")?.GetComponent<Room>();
            SubscribeToRoom();
        }
        
        private void SubscribeToRoom()
        {
            if (_currentRoom == null) return;
            _currentRoom.OnStressed += HandleStress;
        }
        
        private void UnsubscribeFromRoom()
        {
            if (_currentRoom == null) return;
            _currentRoom.OnStressed -= HandleStress;
        }
        
        private void HandleStress(float stress)
        {
            _vignette.intensity.value = Mathf.Lerp(0, maxVignette, stress);
            _motionBlur.intensity.value = Mathf.Lerp(0, maxMotionBlur, stress);
            _chromaticAberration.intensity.value = Mathf.Lerp(0, maxChromaticAberration, stress);
            _colorAdjustments.saturation.value = Mathf.Lerp(0, minSaturation, stress);
            _filmGrain.intensity.value = Mathf.Lerp(0, maxFilmGrain, stress);
            if (_mainCamera != null) _mainCamera.fieldOfView = Mathf.Lerp(_baseFov, minCameraFov, stress);
            
            _audioSource.volume = Mathf.Lerp(0, maxHeartbeatVolume, stress);
            _audioSource.pitch = Mathf.Lerp(1, maxHeartbeatPitch, stress);
        }
    }
}