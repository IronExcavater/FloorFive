using UnityEngine;

/// <summary>
/// Interface for objects that can be interacted with by the player.
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// Gets a value indicating whether this object can currently be interacted with.
    /// </summary>
    bool CanInteract { get; }

    /// <summary>
    /// Gets the interaction message to display to the player.
    /// </summary>
    string InteractMessage { get; }

    /// <summary>
    /// Called when the player interacts with this object (e.g., presses the interaction key).
    /// </summary>
    void OnInteract();

    /// <summary>
    /// Called when the player starts looking at this object (used for visual highlighting).
    /// </summary>
    /// <param name="highlightColor">The color to use for highlighting.</param>
    void OnStartHighlight(Color highlightColor);

    /// <summary>
    /// Called when the player stops looking at this object (removes visual highlighting).
    /// </summary>
    void OnEndHighlight();

    
}