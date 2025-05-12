using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Animation;
using Audio;
using HomebrewIK;
using Level;
using Load;
using Tools;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Utils;

namespace Player
{
    [RequireComponent(typeof(PlayerInput), typeof(Rigidbody), typeof(CapsuleCollider))]
    [RequireComponent(typeof(AudioSource))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Camera")]
        public Transform cameraTransform;
        public Transform cameraPivot;
        public Transform modelPivot;
        public Transform firstPersonTarget;
        public Transform thirdPersonTarget;
        
        public float cameraLerpSpeed = 5;
        public float mouseSensitivity = 10;
        public float scrollSensitivity = 10;
        public float maxCameraDistance = 1;
        public float maxCameraDelta = 5;
        
        [Header("Model IK")]
        public Transform headTransform;
        public Transform toolTarget;
        
        [Header("Movement")]
        public float walkSpeed = 5;
        public float sprintSpeed = 8;
        public float acceleration = 5;
        public float liftForce = 20;

        [Header("Interact")]
        public float interactRadius = 0.5f;
        public float interactDistance = 1.5f;
        public LayerMask defaultMask;
        public LayerMask movableMask;
        public LayerMask interactMask;
        
        [Header("Transition")]
        [SerializeField] private CanvasGroup fadeOverlay;
        
        private List<ToolBase> _tools;
        private int _toolIndex = -1;

        public event Action<ToolBase> OnToolAdded;

        public int ToolIndex
        {
            get => _toolIndex;
            set
            {
                if (_toolIndex == value) return;

                ToolBase prevTool = EquippedTool();
                if (prevTool) prevTool.gameObject.SetActive(false);
                
                _toolIndex = value;
                
                ToolBase newTool = EquippedTool();
                if (newTool == null) return;
                
                newTool.gameObject.SetActive(true);
                newTool.transform.SetParent(toolTarget);
                newTool.transform.localPosition = newTool.toolOffsetPosition;
                newTool.transform.localEulerAngles = newTool.toolOffsetRotation;
            }
        }

        private ToolBase EquippedTool() => _toolIndex >= 0 && _toolIndex < _tools.Count ? _tools[_toolIndex] : null;
        
        private Room _currentRoom;
        
        private PlayerInput _input;
        private Rigidbody _rigidbody;
        private Animator _animator;
        private AnimatorCache _animatorCache;
        private AnimatorEventDispatcher _animatorEventDispatcher;
        private AudioSource _audioSource;
        private CsHomebrewIK _homebrewIK;
        
        private Vector2 _lookRotation;
        private float _targetZoom;
        private float _lookZoom;
        private Vector3 _startPosition;
        private Quaternion _startRotation;
        
        private Vector2 _moveInput;
        private Vector3 _moveVelocity;

        private Rigidbody _grabbedTarget;
        private Rigidbody GrabbedTarget
        {
            get => _grabbedTarget;
            set
            {
                if (value != null)
                {
                    _grabbedTarget = value;
                    _grabbedTarget.linearDamping = 2;
                    _grabbedTarget.angularDamping = 2;
                    _grabbedTarget.useGravity = false;
                }
                else if (_grabbedTarget != null)
                {
                    _grabbedTarget.linearDamping = 0;
                    _grabbedTarget.angularDamping = 0.05f;
                    _grabbedTarget.useGravity = true;
                    _grabbedTarget = null;
                }
            }
        }
        
        private Interactable _interactTarget;

        private Interactable InteractTarget
        {
            get => _interactTarget;
            set
            {
                if (_interactTarget == value) return;
                _interactTarget?.Hide();
                _interactTarget = value;
                _interactTarget?.Show();
            }
        }
        
        private void Awake()
        {
            _input = GetComponent<PlayerInput>();
            _rigidbody = GetComponent<Rigidbody>();
            _animator = GetComponentInChildren<Animator>();
            _animatorCache = GetComponentInChildren<AnimatorCache>();
            _animatorEventDispatcher = GetComponentInChildren<AnimatorEventDispatcher>();
            _audioSource = GetComponent<AudioSource>();
            _homebrewIK = GetComponentInChildren<CsHomebrewIK>();
            
            _startPosition = transform.position;
            _startRotation = transform.rotation;
            
            _tools = new();
        }
        

        private void OnEnable()
        {
            LoadManager.OnSceneLoaded += OnSceneLoaded;
            _animatorEventDispatcher.OnAnimatorIKUpdate += OnAnimatorIK;
            SubscribeToRoom();
        }

        private void OnDisable()
        {
            LoadManager.OnSceneLoaded -= OnSceneLoaded;
            _animatorEventDispatcher.OnAnimatorIKUpdate -= OnAnimatorIK;
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
            _currentRoom.OnPassedOut += HandlePassedOut;
        }
        
        private void UnsubscribeFromRoom()
        {
            if (_currentRoom == null) return;
            _currentRoom.OnPassedOut -= HandlePassedOut;
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            _currentRoom = GameObject.FindGameObjectWithTag("Room")?.GetComponent<Room>();
            SubscribeToRoom();
        }

        private void Update()
        {
            HandleInput();
            HandleAttack();
            HandleInteract();
            HandleUse();
            HandleTool();
        }

        private void FixedUpdate()
        {
            HandleMove();
            HandleGrab();
        }

        private void LateUpdate()
        {
            HandleLook();
            HandleZoom();
        }
        
        private void OnAnimatorIK(int layerIndex)
        {
            HandleHeadIK();
            HandleHandIK();
        }

        private void HandleInput()
        {
            _moveInput = _input.actions["Move"].ReadValue<Vector2>();
        }

        private void HandleLook()
        {
            Vector2 lookInput = _input.actions["Look"].ReadValue<Vector2>();
            
            Vector2 lookDelta = mouseSensitivity * Time.smoothDeltaTime * new Vector2(-lookInput.y, lookInput.x);
            lookDelta = Vector3.ClampMagnitude(lookDelta, maxCameraDelta);
            
            _lookRotation += lookDelta;
            _lookRotation.x = Mathf.Clamp(_lookRotation.x, -85, 85);
            
            cameraPivot.localRotation = Quaternion.Euler(_lookRotation.x, _lookRotation.y, 0);

            modelPivot.localRotation = Quaternion.Slerp(
                modelPivot.localRotation,
                Quaternion.Euler(0, _lookRotation.y, 0),
                cameraLerpSpeed * Time.deltaTime);
        }

        private void HandleZoom()
        {
            float scrollDelta = _input.actions["Zoom"].ReadValue<float>() * Time.deltaTime * scrollSensitivity;
            _targetZoom = Mathf.Clamp(_targetZoom + scrollDelta, 0, maxCameraDistance);
            _lookZoom = Mathf.Lerp(_lookZoom, _targetZoom, cameraLerpSpeed * Time.deltaTime);
            
            Vector3 pivotTarget = Vector3.Lerp(firstPersonTarget.position, thirdPersonTarget.position, _lookZoom);
            cameraPivot.position = pivotTarget;
            
            Vector3 cameraTarget = cameraPivot.TransformPoint(new Vector3(0, 0.4f, -1) * _lookZoom);

            // Handle camera collision for TPS
            Vector3 direction = cameraTarget - cameraPivot.position;
            if (Physics.SphereCast(
                    cameraPivot.position, 0.2f, direction.normalized,
                    out var hit, direction.magnitude, defaultMask))
            {
                cameraTarget = Utils.Utils.ClosestPointOnLine(cameraPivot.position, direction, hit.point + hit.normal * 0.2f);
            }
            
            cameraTransform.position = Vector3.Lerp(cameraTransform.position, cameraTarget, Time.deltaTime * cameraLerpSpeed);
            cameraTransform.LookAt(cameraPivot.position + cameraPivot.forward, cameraPivot.up);
        }

        private void HandleHeadIK()
        {
            _animator.SetLookAtWeight(
                1f,
                0f,
                1f,
                0.5f,
                0.7f
            );
            _animator.SetLookAtPosition(cameraPivot.position + cameraTransform.forward * 3);
        }
        
        private void HandleHandIK()
        {
            if (EquippedTool() == null) return;

            Quaternion handRotation = Quaternion.Euler(toolTarget.eulerAngles + EquippedTool().handOffsetRotation);
            Vector3 handPosition = toolTarget.position + toolTarget.rotation * EquippedTool().handOffsetPosition;
            
            _animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f);
            _animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1f);

            _animator.SetIKPosition(AvatarIKGoal.RightHand, handPosition);
            _animator.SetIKRotation(AvatarIKGoal.RightHand, handRotation);
        }

        private void HandleMove()
        {
            float targetSpeed = _input.actions["Sprint"].IsPressed() ? sprintSpeed : walkSpeed;
            Vector3 inputDirection = new Vector3(_moveInput.x, 0, _moveInput.y).normalized * targetSpeed;
            
            Quaternion yawRotation = Quaternion.Euler(0, cameraPivot.eulerAngles.y, 0);
            Vector3 moveDirection = yawRotation * inputDirection;
            _moveVelocity = Vector3.Lerp(_moveVelocity, moveDirection, Time.fixedDeltaTime * acceleration);

            Vector3 currentVelocity = _rigidbody.linearVelocity;
            currentVelocity.y = 0;
            _rigidbody.AddForce(_moveVelocity - currentVelocity, ForceMode.VelocityChange);
            
            var actualSpeed = new Vector3(_rigidbody.linearVelocity.x, 0, _rigidbody.linearVelocity.z).magnitude;
            var blend = inputDirection.magnitude > 0 ? Mathf.Clamp01(actualSpeed / inputDirection.magnitude) : 0;

            var prevXVel = _animator.GetFloat(_animatorCache.GetHash("xVel"));
            var prevYVel = _animator.GetFloat(_animatorCache.GetHash("yVel"));
        
            _animator.SetFloat(_animatorCache.GetHash("xVel"), Mathf.Lerp(prevXVel, inputDirection.x * blend, Time.deltaTime * 10));
            _animator.SetFloat(_animatorCache.GetHash("yVel"), Mathf.Lerp(prevYVel, inputDirection.z * blend, Time.deltaTime * 10));
        }

        private void HandleGrab()
        {
            if (GrabbedTarget == null) return;

            Vector3 holdPosition = cameraPivot.position + cameraTransform.forward;
            Vector3 toTarget = holdPosition - GrabbedTarget.position;

            if (toTarget.magnitude > interactDistance * 1.5f ||
                GrabbedTarget.isKinematic)
            {
                GrabbedTarget = null;
                return;
            }
            
            GrabbedTarget.AddForce(liftForce * toTarget - GrabbedTarget.linearVelocity, ForceMode.Acceleration);
            
            Quaternion targetRotation = Quaternion.LookRotation(modelPivot.forward, Vector3.up);
            GrabbedTarget.MoveRotation(Quaternion.Slerp(GrabbedTarget.rotation, targetRotation, Time.fixedDeltaTime * acceleration));
        }

        private void HandleAttack()
        {
            if (_input.actions["Attack"].WasPressedThisFrame()
                && GrabbedTarget == null
                && Physics.SphereCast(
                    cameraPivot.position, interactRadius, cameraTransform.forward,
                    out var hit, interactDistance, movableMask))
            {
                Rigidbody rigidbody = hit.collider.GetComponentInParent<Rigidbody>();
                if (rigidbody != null && !rigidbody.isKinematic)
                {
                    GrabbedTarget = rigidbody;
                    GrabbedTarget.TryGetComponent(out Movable movable);
                    movable.OnGrab();
                    SubtitleUI.TriggerEvent(SubtitleEvent.OnGrabbed);
                }
            }

            if (_input.actions["Attack"].WasReleasedThisFrame() && GrabbedTarget != null)
            {
                GrabbedTarget = null;
            }
        }

        private void HandleInteract()
        {
            if (Physics.SphereCast(
                    cameraPivot.position, interactRadius, cameraTransform.forward,
                    out var hit, interactDistance, interactMask) &&
                !Physics.Raycast(cameraPivot.position, hit.collider.bounds.center - cameraPivot.position,
                    Vector3.Distance(hit.collider.bounds.center, cameraPivot.position), defaultMask))
            {
                Interactable interactable = hit.collider.GetComponentInParent<Interactable>();
                if (interactable != null && interactable.enabled)
                {
                    InteractTarget = interactable;
                }
                else InteractTarget = null;
            }
            else InteractTarget = null;
            
            if (_input.actions["Interact"].WasPerformedThisFrame() && InteractTarget != null)
            {
                InteractTarget?.OnInteract(this);
            }
        }

        private void HandleUse()
        {
            if (_input.actions["Use"].WasPressedThisFrame())
            {
                Debug.Log("Tool used");
                EquippedTool()?.OnUse(this);
            }
        }

        private void HandleTool()
        {
            toolTarget.rotation = headTransform.rotation;
            
            for (int i = 0; i < 9; i++)
            {
                if (Keyboard.current[(Key)((int)Key.Digit1 + i)].wasPressedThisFrame)
                {
                    if (i < _tools.Count)
                    {
                        if (_tools[i] == EquippedTool()) ToolIndex = -1;
                        else ToolIndex = i;
                    }
                }
            }

            float scroll = _input.actions["Zoom"].IsPressed() ? 0 : _input.actions["Scroll"].ReadValue<float>();
            if (scroll != 0 && _tools.Count > 0)
            {
                int direction = scroll > 0 ? 1 : -1;
                int newIndex = (_toolIndex + direction + _tools.Count) % _tools.Count;
                ToolIndex = newIndex;
            }
        }

        public void AddTool(ToolBase tool)
        {
            tool.rigidbody.isKinematic = true;
            tool.colliders.ToList().ForEach(collider => collider.enabled = false);
            tool.equipped = true;
            _tools.Add(tool);
            
            Debug.Log($"Adding tool {tool.gameObject.name}");
            OnToolAdded?.Invoke(tool);
            SubtitleUI.TriggerEvent(SubtitleEvent.OnToolAdded);
            
            ToolIndex = _tools.Count - 1;
        }

        private void HandlePassedOut()
        {
            StartCoroutine(OnPassedOut());
        }

        private IEnumerator OnPassedOut()
        {
            AnimationManager.RemoveTweens(this);
            _homebrewIK.Mute = true;
            
            var fadeIn = AnimationManager.CreateTween(this, alpha => fadeOverlay.alpha = alpha,
                fadeOverlay.alpha, 1, 1, Easing.EaseOutBounce);
            _animator.SetTrigger(_animatorCache.GetHash("PassedOut"));
            _audioSource.PlayOneShot(AudioManager.AudioGroupDictionary.GetValue("passedOut").GetFirstClip());
            
            yield return new WaitUntil(() => !AnimationManager.HasTween(fadeIn) && !_audioSource.isPlaying);
            yield return new WaitForSeconds(3);
            transform.position = _startPosition;
            transform.rotation = _startRotation;
            
            var fadeOut = AnimationManager.CreateTween(this, alpha => fadeOverlay.alpha = alpha,
                fadeOverlay.alpha, 0, 1, Easing.EaseOutCubic);
            
            yield return new WaitUntil(() => !AnimationManager.HasTween(fadeOut));
            _homebrewIK.Mute = false;
        }
    }
}