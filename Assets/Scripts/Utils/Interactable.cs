using System;
using Player;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Utils
{
    public abstract class Interactable : MonoBehaviour
    {
        public new bool enabled = true;
        
        [Header("Interaction")]
        public string promptText = "Press E";
        public bool showOutline = true;
        public Vector3 promptOffset;

        public event Action OnInteracted;
        
        private Outline _outline;
        [HideInInspector] public new Collider collider;

        protected virtual void Awake()
        {
            _outline = GetComponent<Outline>();
            collider = GetComponentInChildren<Collider>();
            
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

        public void OnInteract(PlayerController player)
        {
            OnInteracted?.Invoke();
            Interact(player);
        }

        protected abstract void Interact(PlayerController player);
    }
}