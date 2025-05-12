using System;
using Level;
using Load;
using Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

namespace Tools
{
    [RequireComponent(typeof(Rigidbody))]
    public abstract class ToolBase : Interactable
    {
        [Header("Tool Offsets")]
        public Vector3 toolOffsetPosition;
        public Vector3 toolOffsetRotation;
        public Vector3 handOffsetPosition;
        public Vector3 handOffsetRotation;

        public event Action OnUsed;
        
        protected Rigidbody _rigidbody;
        protected AudioSource _audioSource;

        public bool equipped;

        private Room _currentRoom;
        protected float _stress;
        
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

        protected override void Awake()
        {
            base.Awake();
            _rigidbody = GetComponent<Rigidbody>();
            _audioSource = GetComponentInChildren<AudioSource>();
        }

        protected virtual void Update()
        {
            if (!equipped) return;
            transform.localPosition = toolOffsetPosition;
            transform.localRotation = Quaternion.Euler(toolOffsetRotation);
        }

        protected override void Interact(PlayerController player)
        {
            _rigidbody.isKinematic = true;
            _collider.enabled = false;
            player.AddTool(this);
            equipped = true;
        }
        
        public void OnUse(PlayerController player)
        {
            OnUsed?.Invoke();
            Use(player);
        }

        private void HandleStress(float stress)
        {
            _stress = stress;
        }
        
        protected abstract void Use(PlayerController player);
    }
}