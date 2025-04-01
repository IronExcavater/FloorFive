using UnityEngine;

public class Connection : MonoBehaviour
{
    private Room _room;
    public Connection other;
    public Trigger trigger;

    private void Start()
    {
        _room = GetComponentInParent<Room>();
        trigger.OnPlayerEnter += () =>
        {
            if (other != null) return;
            _room.DestroyConnections();
            other = GameController.CreateRoom(this);
        };
    }
}