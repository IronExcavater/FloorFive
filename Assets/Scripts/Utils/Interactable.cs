using System;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Utils
{
    [RequireComponent(typeof(Collider))]
    public abstract class Interactable : MonoBehaviour
    {
        public new bool enabled = true;
        
        [Header("Interaction")]
        public string promptText = "Press E";
        public bool showOutline = true;
        public Vector3 promptOffset;

        public event Action OnInteracted;
        
        private Outline _outline;

        protected virtual void Awake()
        {
            _outline = GetComponent<Outline>();
            
            Utils.SetLayerRecursive(gameObject, LayerMask.NameToLayer("Interact"));
        }

        public void Show()
        {
            if (showOutline && _outline != null) _outline.enabled = true;
            InteractionUI.ShowPrompt(this, promptText);
        }

        public void Hide()
        {
            if (showOutline && _outline != null) _outline.enabled = false;
            InteractionUI.HidePrompt();
        }

        public void OnInteract()
        {
            OnInteracted?.Invoke();
            Interact();
        }

        protected abstract void Interact();
    }
}