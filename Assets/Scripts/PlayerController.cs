using HomebrewIK;
using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Parameters")]
    public float walkSpeed = 2;
    public float runSpeed = 4;

    public float animationBlend = 4;
    
    public float mouseSensitivity = 20;
    public float maxMouseDelta = 5;
    public Vector2 xLookLimits = new(-70, 70);

    private Vector2 _currentVel;
    private Vector2 _lookRotation;
    
    [Header("Components")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Animator ani;
    [SerializeField] private AnimatorCache aniCache;
    [SerializeField] private CapsuleCollider capCol;
    [SerializeField] private Transform camTrans;
    [SerializeField] private Transform camRootTrans;
    [SerializeField] private CsHomebrewIK footIK;

    private void Start()
    {
        LockCursor();
    }

    private void OnEnable()
    {
        footIK.OnStep += OnStep;
    }

    private void OnDisable()
    {
        footIK.OnStep -= OnStep;
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
        
        var actualSpeed = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;
        var blend = _currentVel.magnitude > 0 ? Mathf.Clamp01(actualSpeed / _currentVel.magnitude) : 0;

        var prevXVel = ani.GetFloat(aniCache.GetHash("xVel"));
        var prevYVel = ani.GetFloat(aniCache.GetHash("yVel"));
        
        ani.SetFloat(aniCache.GetHash("xVel"), Mathf.Lerp(prevXVel, _currentVel.x * blend, Time.deltaTime * 10));
        ani.SetFloat(aniCache.GetHash("yVel"), Mathf.Lerp(prevYVel, _currentVel.y * blend, Time.deltaTime * 10));
    }

    private void Look()
    {
        var input = InputManager.Look;
        
        // Add lookInput (-y, x) and clamp x to -90 and 90 degrees
        var lookDelta = mouseSensitivity * Time.smoothDeltaTime * new Vector2(-input.y, input.x);
        lookDelta.x = Mathf.Clamp(lookDelta.x, -maxMouseDelta, maxMouseDelta);
        lookDelta.y = Mathf.Clamp(lookDelta.y, -maxMouseDelta, maxMouseDelta);
        
        _lookRotation += lookDelta;
        _lookRotation.x = Mathf.Clamp(_lookRotation.x, xLookLimits.x, xLookLimits.y);
        
        // Apply rotation of xy to camera and y to body
        transform.rotation = Quaternion.Euler(0, _lookRotation.y, 0);
        camTrans.position = camRootTrans.position;
        camTrans.localRotation = Quaternion.Euler(_lookRotation.x, 0, 0);
    }

    private void OnStep(bool isRightFoot)
    {
        var audioSource = isRightFoot ? footIK.leftFootAudioSource : footIK.rightFootAudioSource;
        var audioClip = AudioManager.GetRandomClip(AudioManager.Audio.step);
        audioSource.PlayOneShot(audioClip);
    }

    internal void ToggleControl(bool v)
    {
        throw new NotImplementedException();
    }
}
