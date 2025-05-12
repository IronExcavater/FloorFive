using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Animation;
using Audio;
using Load;
using Player;
using Tools;
using UI;

namespace Level
{
    [RequireComponent(typeof(Animator), typeof(AnimatorCache))]
    public class Elevator : MonoBehaviour
    {
        private Room _currentRoom;

        public MeshRenderer[] numberMeshRenderers;
        public Texture2D[] numberEmissionTextures;

        public Button externalButton;
        public Button internalButton;
        
        public Collider doorCollider;
        public float exitDistance = 1f;

        [SerializeField] private List<ToolBase> toolPrefabs;

        public int levelBuildIndex;

        private Animator _animator;
        private AnimatorCache _animatorCache;
        private AudioSource _audioSource;
        private PlayerController _player;
        
        private bool _doorsOpen;
        private bool _isAnimating;

        public event Action OnElevatorOpened;
        public event Action OnElevatorRode;
        public event Action OnElevatorClosed;
        public event Action OnElevatorCrashed;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _animatorCache = GetComponent<AnimatorCache>();
            _audioSource = GetComponentInChildren<AudioSource>();
        }

        private void OnEnable()
        {
            externalButton.OnInteracted += OnExternalButton;
            internalButton.OnInteracted += OnInternalButton;
        }

        private void OnDisable()
        {
            externalButton.OnInteracted -= OnExternalButton;
            internalButton.OnInteracted -= OnInternalButton;
        }

        private void Start()
        {
            _player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
            
            _currentRoom = GameObject.FindGameObjectWithTag("Room")?.GetComponent<Room>();
            if (_currentRoom != null)
            {
                StartCoroutine(ElevatorStage(2, 3));
            }
        }

        private void Update()
        {
            if (_doorsOpen && Vector3.Distance(_player.transform.position, transform.position) >= exitDistance)
            {
                StartCoroutine(ElevatorStage(0, 1));
            }
            
            doorCollider.enabled = !_doorsOpen;

            externalButton.enabled = _currentRoom != null && _currentRoom.Status == Room.State.Ready && _currentRoom.IsToolEquipped;
            internalButton.enabled = _currentRoom == null && !_isAnimating ||
                                     _currentRoom != null && _currentRoom.Status == Room.State.Complete && !_doorsOpen ||
                                     _currentRoom != null && _currentRoom.Status == Room.State.Ready && !_isAnimating && !_doorsOpen;
        }

        private void OnExternalButton()
        {
            StartCoroutine(ElevatorStage(3, 4));
        }

        private void OnInternalButton()
        {
            if (_currentRoom == null)
                StartCoroutine(ElevatorStage(2, 3));
            else if (_currentRoom.Status == Room.State.Complete)
                StartCoroutine(ElevatorStage(0, 3));
            else if (_currentRoom.Status == Room.State.Ready)
                StartCoroutine(ElevatorStage(2, 3));
        }
        
        /// <summary>
        /// Animate elevator through this method
        /// </summary>
        /// <param name="startStage">inclusive</param>
        /// <param name="endStage">exclusive</param>
        /// <returns></returns>
        private IEnumerator ElevatorStage(int startStage, int endStage)
        {
            _isAnimating = true;
            if (startStage <= 0 && endStage > 0) yield return StartCoroutine(ElevatorClose());
            if (startStage <= 1 && endStage > 1) yield return StartCoroutine(ElevatorRide());
            if (startStage <= 2 && endStage > 2) yield return StartCoroutine(ElevatorOpen());
            if (startStage <= 3 && endStage > 3) yield return StartCoroutine(ElevatorCrash());
            _isAnimating = false;
        }
        
        private IEnumerator ElevatorClose(float waitTime = 0)
        {
            _doorsOpen = false;
            OnElevatorClosed?.Invoke();
            SubtitleUI.TriggerEvent(SubtitleEvent.OnElevatorClosed);
            
            yield return new WaitForSeconds(waitTime);
            
            _audioSource.PlayOneShot(AudioManager.AudioGroupDictionary.GetValue("elevatorClose").GetFirstClip());
            _animator.SetTrigger(_animatorCache.GetHash("Close"));
            
            yield return new WaitUntil(() => !_audioSource.isPlaying);
        }

        private IEnumerator ElevatorRide()
        {
            _currentRoom = null;
            OnElevatorRode?.Invoke();
            SubtitleUI.TriggerEvent(SubtitleEvent.OnElevatorRode);
            
            _audioSource.PlayOneShot(AudioManager.AudioGroupDictionary.GetValue("elevatorRide").GetFirstClip());

            yield return StartCoroutine(LoadLevel());
            if (_currentRoom) _currentRoom.Mute = true;

            yield return new WaitForSeconds(2);
            if (_currentRoom) UpdateFloorDisplay(_currentRoom.floorNumber);
            
            yield return new WaitUntil(() => !_audioSource.isPlaying);
            if (_currentRoom) _currentRoom.Mute = false;
        }
        
        private IEnumerator ElevatorOpen(float waitTime = 0)
        {
            _doorsOpen = true;
            OnElevatorOpened?.Invoke();
            SubtitleUI.TriggerEvent(SubtitleEvent.OnElevatorOpened);

            if (_currentRoom == null) yield return StartCoroutine(LoadLevel());
            if (_currentRoom) UpdateFloorDisplay(_currentRoom.floorNumber);
            
            _audioSource.PlayOneShot(AudioManager.AudioGroupDictionary.GetValue("ding").GetFirstClip());
            
            yield return new WaitForSeconds(waitTime);
            
            _audioSource.PlayOneShot(AudioManager.AudioGroupDictionary.GetValue("elevatorOpen").GetFirstClip());
            _animator.SetTrigger(_animatorCache.GetHash("Open"));
            
            yield return new WaitUntil(() => !_audioSource.isPlaying);
        }
        
        private IEnumerator ElevatorCrash(float waitTime = 0)
        {
            OnElevatorCrashed?.Invoke();
            SubtitleUI.TriggerEvent(SubtitleEvent.OnElevatorCrashed);
            
            yield return new WaitForSeconds(waitTime);
            
            _audioSource.PlayOneShot(AudioManager.AudioGroupDictionary.GetValue("elevatorCrash").GetFirstClip());
            
            yield return new WaitForSeconds(10);
            _currentRoom.Status = Room.State.Active;
            
            yield return new WaitUntil(() => !_audioSource.isPlaying);
        }

        private IEnumerator LoadLevel()
        {
            if (levelBuildIndex == -1)
            {
                levelBuildIndex = LoadManager.MainMenuSceneIndex;
                for (int i = 0; i < levelBuildIndex - 3; i++)
                {
                    _player.AddTool(Instantiate(toolPrefabs[i]));
                }
            }
            else levelBuildIndex++;
            
            int activeLevelBuildIndex = LoadManager.ActiveLevelBuildIndex;
            
            LoadManager.UnloadScene(activeLevelBuildIndex);
            LoadManager.LoadScene(levelBuildIndex);
            yield return new WaitUntil(() => !LoadManager.IsLoading);
            
            _currentRoom = GameObject.FindGameObjectWithTag("Room")?.GetComponent<Room>();
        }

        private void UpdateFloorDisplay(int floorNumber)
        {
            if (numberEmissionTextures == null || floorNumber < 0 ||
                floorNumber >= numberEmissionTextures.Length) return;
            
            foreach (MeshRenderer renderer in numberMeshRenderers)
            {
                var material = renderer.material;
                material.SetTexture("_EmissionMap", numberEmissionTextures[floorNumber]);
            }
        }
    }
}