using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Parameters")]
    public float walkSpeed;
    public float runSpeed;

    public float animationBlend;
    public bool animationBusy;
    
    public float mouseSensitivity;
    public Vector2 xLookLimits;

    private Vector2 _currentVel;
    private Vector2 _lookRotation;
    
    [Header("Components")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Animator ani;
    [SerializeField] private AnimatorCache aniCache;
    [SerializeField] private CapsuleCollider capCol;
    [SerializeField] private Transform camTrans;
    [SerializeField] private Transform camRootTrans;

    private void Start()
    {
        LockCursor();
    }

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void FixedUpdate()
    {
        Look();
        Move();
    }

    private void Move()
    {
        var input = InputManager.Move;
        var speed = input == Vector2.zero ? 0 : InputManager.Run ? runSpeed : walkSpeed;
        
        _currentVel = Vector3.Lerp(_currentVel, input * speed, Time.fixedDeltaTime * animationBlend);
        var worldVel = transform.TransformVector(new Vector3(_currentVel.x, 0, _currentVel.y));
        var deltaVel = worldVel - new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        
        rb.AddForce(deltaVel, ForceMode.VelocityChange);
        
        ani.SetFloat(aniCache.GetHash("xVel"), _currentVel.x);
        ani.SetFloat(aniCache.GetHash("yVel"), _currentVel.y);
    }

    private void Look()
    {
        var input = InputManager.Look;
        
        // Add lookInput (-y, x) and clamp x to -90 and 90 degrees
        _lookRotation += mouseSensitivity * Time.smoothDeltaTime * new Vector2(-input.y, input.x);
        _lookRotation.x = Mathf.Clamp(_lookRotation.x, xLookLimits.x, xLookLimits.y);
        
        // Apply rotation of xy to camera and y to body
        transform.rotation = Quaternion.Euler(0, _lookRotation.y, 0);
        camTrans.position = camRootTrans.position;
        camTrans.localRotation = Quaternion.Euler(_lookRotation.x, 0, 0);
    }
}
