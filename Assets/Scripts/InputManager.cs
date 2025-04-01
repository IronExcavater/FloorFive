using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private static InputManager Input { get; set; }
    
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
