using System;
using Animation;
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
        
        [Header("Movement")]
        public float walkSpeed = 5;
        public float sprintSpeed = 8;
        public float acceleration = 5;
        public float liftForce = 20;

        [Header("Interact")]
        public float interactRadius = 0.5f;
        public float interactDistance = 2;
        public LayerMask interactMask;

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
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            HandleInput();
            HandleLook();
            HandleZoom();
            HandleAttack();
            HandleInteract();
        }

        private void FixedUpdate()
        {
            HandleMove();
            HandleGrab();
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
            
            Vector3 target = Vector3.Lerp(firstPersonTarget.position, thirdPersonTarget.position, _lookZoom);
            cameraPivot.position = target;
            
            cameraTransform.localPosition = new Vector3(0, 0.4f, -1) * _lookZoom;
            cameraTransform.LookAt(cameraPivot.position + cameraPivot.forward, cameraPivot.up);

            // Handle camera collision for TPS
            Vector3 direction = cameraTransform.position - cameraPivot.position;
            if (Physics.SphereCast(
                    cameraPivot.position, 0.2f, direction.normalized,
                    out var hit, direction.magnitude, interactMask))
            {
                Vector3 closestPoint = Utils.Utils.ClosestPointOnLine(cameraPivot.position, direction, hit.point + hit.normal * 0.2f);
                cameraTransform.position = closestPoint;
            }
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
                    out var hit, interactDistance, interactMask)
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
                && hit.collider.TryGetComponent(out Interactable interactable))
            {
                InteractTarget = interactable;
            }
            else
            {
                InteractTarget = null;
            }

            if (_input.actions["Interact"].WasPerformedThisFrame())
            {
                _interactTarget?.Interact();
            }
        }
        
        internal void ToggleControl(bool v)
        {
            throw new NotImplementedException();
        }
    }
}