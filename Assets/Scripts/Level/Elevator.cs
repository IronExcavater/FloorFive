using UnityEngine;
using System.Collections;
using Animation;
using Audio;
using Load;
using Player;

namespace Level
{
    [RequireComponent(typeof(Animator), typeof(AnimatorCache))]
    public class Elevator : MonoBehaviour
    {
        public Room currentRoom;

        public MeshRenderer[] numberMeshRenderers;
        public Texture2D[] numberEmissionTextures;

        public float exitDistance = 2f;

        private Animator _animator;
        private AnimatorCache _animatorCache;
        private AudioSource _audioSource;
        private bool _doorsOpen;
        private PlayerController _player;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _animatorCache = GetComponent<AnimatorCache>();
            _audioSource = GetComponentInChildren<AudioSource>();
            _player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        }

        private void Update()
        {
            if (_doorsOpen && Vector3.Distance(_player.transform.position, transform.position) >= exitDistance)
            {
                StartCoroutine(ElevatorStage(0, 1));
            }
        }

        public void OnButtonInteracted(ElevatorButton.ButtonType buttonType)
        {
            switch (buttonType)
            {
                case ElevatorButton.ButtonType.External:
                    if (!currentRoom.Active) StartCoroutine(ElevatorStage(2, 3));
                    break;
                case ElevatorButton.ButtonType.Internal:
                    if (currentRoom == null) StartCoroutine(ElevatorStage(1, 3));
                    else if (currentRoom != null && !currentRoom.Active) StartCoroutine(ElevatorStage(0, 3));
                    break;
            }
        }

        private IEnumerator ElevatorStage(int startStage, int endStage)
        {
            if (startStage <= 0 && endStage > 0) yield return StartCoroutine(ElevatorClose());
            if (startStage <= 1 && endStage > 1) yield return StartCoroutine(ElevatorRide());
            if (startStage <= 2 && endStage > 2) yield return StartCoroutine(ElevatorOpen());
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
            currentRoom = null;
            _audioSource.PlayOneShot(AudioManager.AudioGroupDictionary.GetValue("elevatorRide").GetFirstClip());
            
            int activeLevelBuildIndex = LoadManager.ActiveLevelBuildIndex;
            yield return StartCoroutine(Utils.Utils.WaitForAll(this,
                LoadManager.UnloadScene(activeLevelBuildIndex),
                LoadManager.LoadScene(activeLevelBuildIndex != -1 ? activeLevelBuildIndex + 1 : 1)
            ));
            currentRoom = GameObject.FindGameObjectWithTag("Room").GetComponent<Room>();

            yield return new WaitForSeconds(6);
            UpdateFloorDisplay(currentRoom.floorNumber);
            
            yield return new WaitUntil(() => !_audioSource.isPlaying);
            
            _audioSource.PlayOneShot(AudioManager.AudioGroupDictionary.GetValue("ding").GetFirstClip());
        }
        
        private IEnumerator ElevatorOpen(float waitTime = 0)
        {
            _doorsOpen = true;
            yield return new WaitForSeconds(waitTime);
            
            _audioSource.PlayOneShot(AudioManager.AudioGroupDictionary.GetValue("elevatorOpen").GetFirstClip());
            _animator.SetTrigger(_animatorCache.GetHash("Open"));
            
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