using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Input { get; private set; }
    
    public PlayerInput playerInput;

    private void Awake()
    {
        if (Input == null)
        {
            Input = this;
            DontDestroyOnLoad(this);
        }
        else Destroy(gameObject);
    }
    
    // Utility functions
    public static InputAction FindAction(string actionNameOrId)
    {
        return Input.playerInput.actions.FindAction(actionNameOrId);
    }
}
