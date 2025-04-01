using UnityEngine;

public class Connection : MonoBehaviour
{
    private Area _area;
    public Connection other;
    public Trigger trigger;

    private void Start()
    {
        _area = GetComponentInParent<Area>();
        trigger.OnPlayerEnter += () =>
        {
            if (other != null) return;
            _area.DestroyConnections();
            other = GameController.CreateArea(this);
        };
    }
}