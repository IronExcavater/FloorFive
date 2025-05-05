using UnityEngine;
using System.Collections;
using Animation;
using Audio;
using Load;
using UnityEngine.Serialization;

namespace Level
{
    [RequireComponent(typeof(Animator), typeof(AnimatorCache))]
    public class Elevator : MonoBehaviour
    {
        public Room currentRoom;

        [FormerlySerializedAs("numberDisplayRenderers")] public MeshRenderer[] numberMeshRenderers;
        public Texture2D[] numberEmissionTextures;

        private Animator _animator;
        private AnimatorCache _animatorCache;
        private AudioSource _audioSource;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _animatorCache = GetComponent<AnimatorCache>();
            _audioSource = GetComponentInChildren<AudioSource>();

            StartCoroutine(ElevatorOpen());
        }

        public void OnButtonInteracted(ElevatorButton.ButtonType buttonType)
        {
            switch (buttonType)
            {
                case ElevatorButton.ButtonType.External:
                    if (currentRoom.Active) break;
                    StartCoroutine(ElevatorOpen());
                    break;
                case ElevatorButton.ButtonType.Internal:
                    if (currentRoom != null || currentRoom.Active) return;
                    StartCoroutine(ElevatorClose());
                    break;
            }
        }
        
        private IEnumerator ElevatorClose(float waitTime = 0)
        {
            yield return new WaitForSeconds(waitTime);
            
            _audioSource.PlayOneShot(AudioManager.AudioGroupDictionary.GetValue("elevatorClose").GetFirstClip());
            _animator.SetTrigger(_animatorCache.GetHash("Close"));
            
            currentRoom = null;
            
            _audioSource.PlayOneShot(AudioManager.AudioGroupDictionary.GetValue("elevatorRide").GetFirstClip());
            
            int activeLevelBuildIndex = LoadManager.ActiveLevelBuildIndex;
            yield return StartCoroutine(Utils.Utils.WaitForAll(this,
                LoadManager.UnloadScene(activeLevelBuildIndex),
                LoadManager.LoadScene(activeLevelBuildIndex != -1 ? activeLevelBuildIndex + 1 : 1)
                ));
            
            currentRoom = GameObject.FindGameObjectWithTag("Room").GetComponent<Room>();
            UpdateFloorDisplay(currentRoom.floorNumber);
            
            yield return new WaitUntil(() => !_audioSource.isPlaying);
        }
        
        private IEnumerator ElevatorOpen(float waitTime = 0)
        {
            yield return new WaitForSeconds(waitTime);

            _audioSource.PlayOneShot(AudioManager.AudioGroupDictionary.GetValue("ding").GetFirstClip());
            
            int activeLevelBuildIndex = LoadManager.ActiveLevelBuildIndex;
            yield return StartCoroutine(Utils.Utils.WaitForAll(this,
                LoadManager.UnloadScene(activeLevelBuildIndex),
                LoadManager.LoadScene(activeLevelBuildIndex != -1 ? activeLevelBuildIndex + 1 : 1)
                ));
            if (currentRoom == null) currentRoom = GameObject.FindGameObjectWithTag("Room").GetComponent<Room>();
            UpdateFloorDisplay(currentRoom.floorNumber);
            
            yield return new WaitUntil(() => !_audioSource.isPlaying);
            
            _audioSource.PlayOneShot(AudioManager.AudioGroupDictionary.GetValue("elevatorOpen").GetFirstClip());
            _animator.SetTrigger(_animatorCache.GetHash("Open"));
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