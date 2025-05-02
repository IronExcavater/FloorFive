using System;
using UnityEngine;
using UnityEngine.InputSystem;

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
        
        [Header("Movement")]
        public float walkSpeed = 5;
        public float sprintSpeed = 8;
        public float acceleration = 5;
        public float jumpHeight = 1;
        public float liftForce = 20;

        [Header("Interact")]
        public float interactRadius = 0.5f;
        public float interactDistance = 2;
        public LayerMask interactMask;

        private PlayerInput _input;
        private Rigidbody _rigidbody;
        private CapsuleCollider _capsuleCollider;
        
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
        
        private IInteractable _interactTarget;
        
        private void Awake()
        {
            _input = GetComponent<PlayerInput>();
            _rigidbody = GetComponent<Rigidbody>();
            _capsuleCollider = GetComponent<CapsuleCollider>();
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            HandleInput();
            HandleJump();
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

        private bool IsFPS => _lookZoom <= 0.05;

        private bool IsGrounded(out RaycastHit hit)
        {
            return Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out hit, 0.2f, interactMask);
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

            if (IsFPS)
            {
                modelPivot.localRotation = Quaternion.Slerp(
                    modelPivot.localRotation,
                    Quaternion.Euler(0, _lookRotation.y, 0),
                    cameraLerpSpeed * Time.deltaTime);
            }
        }

        private void HandleZoom()
        {
            float scrollDelta = _input.actions["Zoom"].ReadValue<float>() * Time.deltaTime * scrollSensitivity;
            _targetZoom = Mathf.Clamp(_targetZoom + scrollDelta, 0, 5);
            _lookZoom = Mathf.Lerp(_lookZoom, _targetZoom, cameraLerpSpeed * Time.deltaTime);
            
            Vector3 target = Vector3.Lerp(firstPersonTarget.position, thirdPersonTarget.position, _lookZoom);
            cameraPivot.position = target;
            
            cameraTransform.localPosition = new Vector3(0, 0.4f, -1) * _lookZoom;
            cameraTransform.LookAt(cameraPivot.position + cameraPivot.forward, cameraPivot.up);

            // Handle camera collision for TPS
            if (Physics.SphereCast(
                    cameraPivot.position, 0.2f, -cameraTransform.forward,
                    out var hit, (cameraTransform.position - cameraPivot.position).magnitude, interactMask))
            {
                cameraTransform.position = hit.point + hit.normal * 0.2f;
            }
        }

        private void HandleMove()
        {
            // Handle model rotation for TPS
            if (!IsFPS & _moveInput != Vector2.zero)
            {
                float targetAngle = Mathf.Atan2(_moveInput.x, _moveInput.y) * Mathf.Rad2Deg + cameraPivot.eulerAngles.y;
                modelPivot.rotation = Quaternion.Slerp(modelPivot.rotation, Quaternion.Euler(0, targetAngle, 0), Time.deltaTime * cameraLerpSpeed);
            }

            float targetSpeed = _input.actions["Sprint"].IsPressed() ? sprintSpeed : walkSpeed;
            Vector3 inputDirection = new Vector3(_moveInput.x, 0, _moveInput.y).normalized;
            
            Quaternion yawRotation = Quaternion.Euler(0, cameraPivot.eulerAngles.y, 0);
            Vector3 moveDirection = yawRotation * inputDirection;
            moveDirection.y = 0;
            moveDirection.Normalize();
            
            Vector3 targetVelocity = moveDirection * targetSpeed;
            _moveVelocity = Vector3.Lerp(_moveVelocity, targetVelocity, Time.fixedDeltaTime * acceleration);

            Vector3 currentVelocity = _rigidbody.linearVelocity;
            currentVelocity.y = 0;
            _rigidbody.AddForce(_moveVelocity - currentVelocity, ForceMode.VelocityChange);
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

        private void HandleJump()
        {
            if (_input.actions["Jump"].WasPressedThisFrame() && IsGrounded(out RaycastHit hit)
                && (hit.rigidbody == null || hit.rigidbody != GrabbedTarget))
            {
                Vector3 impulse = _rigidbody.mass * Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y) * Vector3.up;
                _rigidbody.AddForce(impulse, ForceMode.Impulse);
            }
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
                && hit.collider.TryGetComponent(out IInteractable interactable))
            {
                if (_interactTarget != interactable)
                {
                    _interactTarget?.OnStartHighlight(Color.white);
                    _interactTarget = interactable;
                    _interactTarget.OnEndHighlight();
                }
            }
            else
            {
                _interactTarget?.OnEndHighlight();
                _interactTarget = null;
            }

            if (_input.actions["Interact"].WasPerformedThisFrame() && _interactTarget != null)
            {
                _interactTarget.OnInteract();
            }
        }
        
        internal void ToggleControl(bool v)
        {
            throw new NotImplementedException();
        }
    }
}