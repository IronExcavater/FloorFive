using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Parameters")]
    public float moveSpeed;
    public float jumpForce;
    public Vector2 mouseSensitivity;
    public float airMoveReduction;
    public LayerMask worldLayer;
    
    private bool IsGrounded => Physics.Raycast(
        transform.position, 
        Vector3.down, 
        capCol.height / 2f + 0.2f, worldLayer);

    private Vector2 _lookRotation;
    
    [Header("Components")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private CapsuleCollider capCol;
    [SerializeField] private Transform camTrans;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    private void Update()
    {
        var lookInput = InputManager.FindAction("Look").ReadValue<Vector2>();
        lookInput = new Vector2(lookInput.x / Screen.width, lookInput.y / Screen.height);
        // Add lookInput (-y, x) and clamp x to -90 and 90 degrees
        _lookRotation += new Vector2(-lookInput.y, lookInput.x) * mouseSensitivity / Time.deltaTime;
        _lookRotation.x = Mathf.Clamp(_lookRotation.x, -90f, 90f);
        
        // Apply rotation of xy to camera and y to body
        camTrans.position = transform.position + new Vector3(0, capCol.height / 4f, 0);
        camTrans.rotation = Quaternion.Euler(_lookRotation.x, _lookRotation.y, 0);
        transform.rotation = Quaternion.Euler(0, _lookRotation.y, 0);
    }
    
    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        var moveInput = InputManager.FindAction("Move").ReadValue<Vector2>();
        moveInput.Normalize();
        
        // Apply force relative to player orientation, reduce force if not grounded
        var moveForce = transform.forward * moveInput.y + transform.right * moveInput.x;
        rb.AddForce(moveForce * (moveSpeed * (!IsGrounded ? airMoveReduction : 1)), ForceMode.Force);
        
        // Limit velocity magnitude to walkSpeed
        var xzVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        if (xzVelocity.magnitude > moveSpeed)
        {
            xzVelocity = xzVelocity.normalized * moveSpeed;
            rb.linearVelocity = new Vector3(xzVelocity.x, rb.linearVelocity.y, xzVelocity.z);
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!IsGrounded) return;
        
        // Remove y velocity and apply jump force
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
}
