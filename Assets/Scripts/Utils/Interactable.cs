using System;
using UnityEngine;
using UnityEngine.UI;

namespace Utils
{
    [RequireComponent(typeof(Collider))]
    public abstract class Interactable : MonoBehaviour
    {
        [Header("Interaction")]
        public string prompt = "Press E";
        public bool showOutline = true;
        public new bool enabled = true;

        public event Action OnInteracted;
        
        private Outline _outline;

        protected virtual void Awake()
        {
            _outline = GetComponent<Outline>();
        }

        public void Show()
        {
            if (showOutline && _outline != null) _outline.enabled = true;
        }

        public void Hide()
        {
            if (showOutline && _outline != null) _outline.enabled = true;
        }

        public void OnInteract()
        {
            OnInteracted?.Invoke();
            Interact();
        }

        protected abstract void Interact();
    }
}