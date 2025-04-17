using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private static InputManager _instance;
    
    [SerializeField] private PlayerInput playerInput;
    
    public static Vector2 Move { get; private set; }
    public static Vector2 Look { get; private set; }

    private static bool _run;
    public static event Action<bool> OnRun;

    public static bool Run
    {
        get => _run;
        private set
        {
            if (_run == value) return;
            _run = value;
            OnRun?.Invoke(value);
        }
    }

    private static bool _interact;
    public static event Action<bool> OnInteract;

    public static bool Interact
    {
        get => _interact;
        private set
        {
            if (_interact == value) return;
            _interact = value;
            OnInteract?.Invoke(value);
        }
}

    private static bool _flashlight;
    public static event Action<bool> OnFlashlight;

    public static bool Flashlight
    {
        get => _flashlight;
        private set
        {
            if (_flashlight == value) return;
            _flashlight = value;
            OnFlashlight?.Invoke(value);
        }
    }

    private InputActionMap _currentMap;

    private readonly List<InputAction> _actions = new();
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(this);

            SetupInputs();
        }
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        SetupInputs();
        EnableInputs(true);
    }

    private void OnDisable()
    {
        EnableInputs(false);
    }

    private void SetupInputs()
    {
        if (!playerInput || playerInput.currentActionMap == null) return;

        SetupAction("Move", ctx => Move = ctx.ReadValue<Vector2>().normalized);
        SetupAction("Look", ctx => Look = ctx.ReadValue<Vector2>());
        SetupAction("Run", ctx => Run = ctx.ReadValueAsButton());
        SetupAction("Interact", ctx => Interact = ctx.ReadValueAsButton());
        SetupAction("Flashlight", ctx =>
        {
            if (ctx.performed && ctx.ReadValueAsButton()) Flashlight = !Flashlight; 
        });
    }

    private void SetupAction(string name, Action<InputAction.CallbackContext> handler)
    {
        var action = playerInput.currentActionMap.FindAction(name);
        if (action == null) return;
        
        action.performed += handler.Invoke;
        action.canceled += handler.Invoke;
        
        _actions.Add(action);
    }

    private static void EnableInputs(bool enable)
    {
        foreach (var action in _instance._actions) EnableAction(action, enable);
}

    private static void EnableAction(InputAction action, bool enable)
    {
        if (action == null) return;
        if (enable) action.Enable();
        else action.Disable();
    }
    
    public static void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
