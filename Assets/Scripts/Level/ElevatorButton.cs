using Utils;

namespace Level
{
    public class ElevatorButton : Interactable
    {
        private Elevator _elevator;
        public ButtonType buttonType;
        
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