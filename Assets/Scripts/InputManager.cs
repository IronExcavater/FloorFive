using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{

    private static InputManager _instance;  
    
    [SerializeField] private PlayerInput playerInput;
    
    public static Vector2 Move { get; private set; }
    public static Vector2 Look { get; private set; }
    public static bool Run { get; private set; }

    private InputActionMap _currentMap;

    private InputAction _move, _look, _run;
    
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

        _move = SetupAction("Move", ctx => Move = ctx.ReadValue<Vector2>().normalized);
        _look = SetupAction("Look", ctx => Look = ctx.ReadValue<Vector2>());
        _run = SetupAction("Run", ctx => Run = ctx.ReadValueAsButton());
    }

    private InputAction SetupAction(string name, Action<InputAction.CallbackContext> handler)
    {
        var action = playerInput.currentActionMap.FindAction(name);
        if (action == null) return null;
        
        action.performed += handler.Invoke;
        action.canceled += handler.Invoke;
        return action;
    }

    private void EnableInputs(bool enable)
    {
        EnableAction(_move, enable);
        EnableAction(_look, enable);
        EnableAction(_run, enable);
    }

    private void EnableAction(InputAction action, bool enable)
    {
        if (action == null) return;
        if (enable) action.Enable();
        else action.Disable();
    }
}
