using System.Collections.Generic;
using System.Linq;
using Animation;
using Level;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;

namespace Player
{
    [RequireComponent(typeof(PlayerInput), typeof(Rigidbody), typeof(CapsuleCollider))]
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
        public float maxCameraDelta = 10; // TODO: Not implemented
        
        [Header("Model IK")]
        public Transform headTransform;
        public Transform handTransform;
        public Transform toolTarget;
        
        [Header("Movement")]
        public float walkSpeed = 5;
        public float sprintSpeed = 8;
        public float acceleration = 5;
        public float liftForce = 20;

        [Header("Interact")]
        public float interactRadius = 0.5f;
        public float interactDistance = 2;
        public LayerMask defaultMask;
        public LayerMask movableMask;
        public LayerMask interactMask;
        
        private Dictionary<int, ToolBase> _tools;
        private int _toolIndex = -1;

        private ToolBase EquippedTool() => _tools.GetValueOrDefault(_toolIndex, null);

        private PlayerInput _input;
        private Rigidbody _rigidbody;
        private Animator _animator;
        private AnimatorCache _animatorCache;
        
        private Vector2 _lookRotation;
        private float _targetZoom;
        private float _lookZoom;
        
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
            _tools = new();
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            HandleInput();
            HandleAttack();
            HandleInteract();
            HandleToolSwitch();
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
            _lookRotation += lookDelta;
            _lookRotation.x = Mathf.Clamp(_lookRotation.x, -90, 90);
            
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
            headTransform.LookAt(cameraPivot.position + cameraTransform.forward * 3);
            Vector3 localAngles = headTransform.localEulerAngles;

            // Convert from [0, 360] → [-180, 180] for clamping
            localAngles.x = localAngles.x > 180 ? localAngles.x - 360 : localAngles.x;
            localAngles.y = localAngles.y > 180 ? localAngles.y - 360 : localAngles.y;

            // Clamp
            localAngles.x = Mathf.Clamp(localAngles.x, -20, 20);
            localAngles.y = Mathf.Clamp(localAngles.y, -30, 30);
            localAngles.z = 0;

            // Just use the clamped values
            headTransform.localEulerAngles = localAngles;
        }
        
        private void HandleHandIK()
        {
            if (EquippedTool() == null) return;

            Vector3 handPosition = toolTarget.transform.position + EquippedTool().handOffsetPosition;
            Quaternion handRotation = Quaternion.Euler(toolTarget.transform.eulerAngles + EquippedTool().handOffsetRotation);
            
            handTransform.position = Vector3.Lerp(handTransform.position, handPosition, Time.deltaTime * acceleration);
            handTransform.rotation = Quaternion.Slerp(handTransform.rotation, handRotation, Time.deltaTime * acceleration);
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

            if (toTarget.magnitude > interactDistance * 1.5f)
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
                    out var hit, interactDistance, movableMask)
                && hit.collider.TryGetComponent(out Rigidbody rigidbody)
                && !rigidbody.isKinematic)
            {
                GrabbedTarget = rigidbody;
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
                    out var hit, interactDistance, interactMask)
                && hit.collider.TryGetComponent(out Interactable interactable)
                && Physics.Raycast(cameraPivot.position, cameraTransform.forward, out var hit2,
                    Vector3.Distance(hit.point, cameraPivot.position), defaultMask)
                && hit.collider.gameObject.Equals(hit2.collider.gameObject))
            {
                InteractTarget = interactable;
            }
            else InteractTarget = null;
            
            if (_input.actions["Interact"].WasPerformedThisFrame())
            {
                if (InteractTarget != null) InteractTarget?.OnInteract();
                else _tools[_toolIndex]?.OnInteract();
            }
        }

        private void HandleToolSwitch()
        {
            for (int i = 0; i < 9; i++)
            {
                if (Keyboard.current[(Key)((int)Key.Digit1 + i)].wasPressedThisFrame)
                {
                    if (_tools.TryGetValue(i, out ToolBase tool))
                        _toolIndex = _tools.First(pair => pair.Value == tool).Key;;
                }
            }

            float scroll = _input.actions["Zoom"].IsPressed() ? 0 : _input.actions["Scroll"].ReadValue<float>();
            if (scroll != 0 && _tools.Count > 0)
            {
                List<int> sortedTools = _tools.Keys.OrderBy(x => x).ToList(); // Not efficient
                
                int direction = scroll > 0 ? 1 : -1;
                _toolIndex = sortedTools[(_toolIndex + direction + sortedTools.Count) % sortedTools.Count];
            }
        }

        public void AddTool(int keyboardDigit, ToolBase tool)
        {
            if (!_tools.ContainsValue(tool)) return;
            
            _tools.TryAdd(keyboardDigit, tool);
            
            if (_tools.TryGetValue(_toolIndex, out ToolBase currentTool)) currentTool.gameObject.SetActive(false);
            
            tool.gameObject.SetActive(true);
            tool.transform.SetParent(handTransform);
            tool.transform.localPosition = tool.toolOffsetPosition;
            tool.transform.localEulerAngles = tool.toolOffsetRotation;
                
            _toolIndex = _tools.First(pair => pair.Value == tool).Key;
        }
    }
}