using HomebrewIK;
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

    public float interactDistance = 3;
    public float interactRadius = 0.5f;
    public LayerMask interactMask;
    
    private Flashlight _flashlight;

    private Vector2 _currentVel;
    private Vector2 _lookRotation;
    
    [Header("Components")]
    private Rigidbody _rb;
    private Animator _ani;
    private AnimatorCache _aniCache;
    [SerializeField] private Transform camTrans;
    [SerializeField] private Transform camTarget;
    private AnimatorIK _animatorIK;
    private CsHomebrewIK _footIK;
    [SerializeField] private Transform flashlightTarget;
    [SerializeField] private Transform flashlightOffset;

    private void Awake()
    {
        _rb = GetComponentInChildren<Rigidbody>();
        _ani = GetComponentInChildren<Animator>();
        _aniCache = GetComponentInChildren<AnimatorCache>();
        _animatorIK = GetComponentInChildren<AnimatorIK>();
        _footIK = GetComponentInChildren<CsHomebrewIK>();

        _animatorIK.OnAnimatorIKUpdate += FlashlightIK;
    }

    private void Start()
    {
        InputManager.LockCursor(true);
    }

    private void OnEnable()
    {
        _footIK.OnStep += OnStep;
        InputManager.OnInteract += OnInteract;
    }

    private void OnDisable()
    {
        _footIK.OnStep -= OnStep;
        InputManager.OnInteract -= OnInteract;
    }

    private void Update()
    {
        Look();
    }

    private void FixedUpdate()
    {
        Move();
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
        camTrans.position = camTarget.position;
        camTrans.localRotation = Quaternion.Euler(_lookRotation.x, 0, 0);
    }

    private void Move()
    {
        // Apply camera rotation of xy to camera and y to body
        transform.rotation = Quaternion.Euler(0, _lookRotation.y, 0);
        camTrans.position = camTarget.position;
        camTrans.localRotation = Quaternion.Euler(_lookRotation.x, 0, 0);
        
        var input = InputManager.Move;
        var speed = input == Vector2.zero ? 0 : InputManager.Run ? runSpeed : walkSpeed;
        
        _currentVel = Vector3.Lerp(_currentVel, input * speed, Time.fixedDeltaTime * animationBlend);
        var worldVel = transform.TransformVector(new Vector3(_currentVel.x, 0, _currentVel.y));
        var deltaVel = worldVel - new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);
        
        _rb.AddForce(deltaVel, ForceMode.VelocityChange);
        
        var actualSpeed = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z).magnitude;
        var blend = _currentVel.magnitude > 0 ? Mathf.Clamp01(actualSpeed / _currentVel.magnitude) : 0;

        var prevXVel = _ani.GetFloat(_aniCache.GetHash("xVel"));
        var prevYVel = _ani.GetFloat(_aniCache.GetHash("yVel"));
        
        _ani.SetFloat(_aniCache.GetHash("xVel"), Mathf.Lerp(prevXVel, _currentVel.x * blend, Time.deltaTime * 10));
        _ani.SetFloat(_aniCache.GetHash("yVel"), Mathf.Lerp(prevYVel, _currentVel.y * blend, Time.deltaTime * 10));
    }

    private void OnInteract(bool value)
    {
        if (!value) return;
        var ray = new Ray(camTrans.position, camTrans.forward);
        if (Physics.SphereCast(ray, interactRadius, out var hit, interactDistance, interactMask))
        {
            if (hit.collider.TryGetComponent<IInteractable>(out var interactable)) interactable.Interact(gameObject);
        }
    }

    private void OnStep(bool isRightFoot)
    {
        var audioSource = isRightFoot ? _footIK.leftFootAudioSource : _footIK.rightFootAudioSource;
        var audioClip = AudioManager.GetRandomClip(AudioManager.Audio.step);
        audioSource.PlayOneShot(audioClip);
    }

    public void SetFlashlight(Flashlight flashlight)
    {
        _flashlight = flashlight;
        flashlight.transform.SetParent(flashlightTarget, worldPositionStays: false);
        Debug.Log(flashlight.transform);
        flashlight.transform.localPosition = Vector3.zero;
        flashlight.transform.localRotation = Quaternion.Euler(-90, 0, 0);
    }

    private void FlashlightIK(int layerIndex)
    {
        var hasFlashlight = _flashlight != null;
        Debug.Log("hasFlashlight: " + hasFlashlight);
        
        _ani.SetIKPositionWeight(AvatarIKGoal.RightHand, hasFlashlight ? 1 : 0);
        _ani.SetIKRotationWeight(AvatarIKGoal.RightHand, hasFlashlight ? 1 : 0);
        _ani.SetIKPosition(AvatarIKGoal.RightHand, flashlightOffset.position);
        _ani.SetIKRotation(AvatarIKGoal.RightHand, flashlightOffset.rotation);
    }
}
