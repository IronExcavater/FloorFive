using UnityEngine;
using Utils;

namespace Level
{
    public class ElevatorButton : Interactable
    {
        [Header("Elevator Button")]
        public ButtonType buttonType;
        
        private Elevator _elevator;
        
        public enum ButtonType
        {
            Internal,
            External
        }

        protected override void Awake()
        {
            base.Awake();
            _elevator = GetComponentInParent<Elevator>();
        }
        
        protected override void Interact()
        {
            _elevator.OnButtonInteracted(buttonType);
        }
    }
}