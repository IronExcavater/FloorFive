using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Animation;
using Audio;
using Load;
using Player;
using Tools;

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

        public int sceneIndex;

        private Animator _animator;
        private AnimatorCache _animatorCache;
        private AudioSource _audioSource;
        private PlayerController _player;
        
        private bool _doorsOpen;
        private bool _isAnimating;

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
                StartCoroutine(ElevatorStage(1, 3));
            else if (_currentRoom.Status == Room.State.Complete)
                StartCoroutine(ElevatorStage(0, 3));
            else if (_currentRoom.Status == Room.State.Ready && !_doorsOpen)
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
            yield return new WaitForSeconds(waitTime);
            
            _audioSource.PlayOneShot(AudioManager.AudioGroupDictionary.GetValue("elevatorClose").GetFirstClip());
            _animator.SetTrigger(_animatorCache.GetHash("Close"));
            
            yield return new WaitUntil(() => !_audioSource.isPlaying);
        }

        private IEnumerator ElevatorRide()
        {
            _currentRoom = null;
            _audioSource.PlayOneShot(AudioManager.AudioGroupDictionary.GetValue("elevatorRide").GetFirstClip());
            
            SceneLoader menuManagers = FindObjectOfType<SceneLoader>();

            if (menuManagers != null)
            {
                sceneIndex = menuManagers.EsceneIndex;
                Debug.Log("Scene Index retrieved from MenuManagers: " + sceneIndex);
            }
            int activeLevelBuildIndex = LoadManager.ActiveLevelBuildIndex;
            yield return StartCoroutine(Utils.Utils.WaitForAll(this,
                LoadManager.UnloadScene(activeLevelBuildIndex),
                LoadManager.LoadScene(activeLevelBuildIndex != -1 ? activeLevelBuildIndex + 1 : sceneIndex)
            ));
            _currentRoom = GameObject.FindGameObjectWithTag("Room")?.GetComponent<Room>();
            if (_currentRoom) _currentRoom.Mute = true;

            yield return new WaitForSeconds(2);
            if (_currentRoom) UpdateFloorDisplay(_currentRoom.floorNumber);
            
            yield return new WaitUntil(() => !_audioSource.isPlaying);
            if (_currentRoom) _currentRoom.Mute = false;
        }
        
        private IEnumerator ElevatorOpen(float waitTime = 0)
        {
            _doorsOpen = true;
            _audioSource.PlayOneShot(AudioManager.AudioGroupDictionary.GetValue("ding").GetFirstClip());
            
            yield return new WaitForSeconds(waitTime);
            
            _audioSource.PlayOneShot(AudioManager.AudioGroupDictionary.GetValue("elevatorOpen").GetFirstClip());
            _animator.SetTrigger(_animatorCache.GetHash("Open"));
            
            yield return new WaitUntil(() => !_audioSource.isPlaying);
        }
        
        private IEnumerator ElevatorCrash(float waitTime = 0)
        {
            yield return new WaitForSeconds(waitTime);
            
            _audioSource.PlayOneShot(AudioManager.AudioGroupDictionary.GetValue("elevatorCrash").GetFirstClip());
            
            yield return new WaitForSeconds(10);
            _currentRoom.Status = Room.State.Active;
            
            yield return new WaitUntil(() => !_audioSource.isPlaying);
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