using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    public Room roomPrefab;
    public Hallway hallwayPrefab;
    public Transform levelTrans;
    
    private ObjectPool<Room> _roomPool;
    private ObjectPool<Hallway> _hallwayPool;
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(this);

            _roomPool = new ObjectPool<Room>(roomPrefab, 2, levelTrans);
            _hallwayPool = new ObjectPool<Hallway>(hallwayPrefab, 3, levelTrans);
        }
        else Destroy(gameObject);
    }

    public static void CreateSection(Connector connector)
    {
        var oppositeConnector = connector.area.GetOppositeConnector(connector);
        
        var room = _instance._roomPool.Get();
        var roomConnector = room.GetRandomConnector();
        ConnectConnectors(room.transform, roomConnector.transform, oppositeConnector.transform);
        oppositeConnector.Connect(roomConnector);
        
        var otherRoomConnector = room.GetOppositeConnector(roomConnector);

        var hallway = _instance._hallwayPool.Get();
        var hallwayConnector = hallway.GetRandomConnector();
        ConnectConnectors(hallway.transform, hallwayConnector.transform, otherRoomConnector.transform);
        otherRoomConnector.Connect(hallwayConnector);
        
        room.Activate(connector);
    }

    public static void DeleteSection(Connector connector)
    {
        var room = connector?.area as Room;
        var hallway = room?.GetOppositeConnector(connector)?.connection?.area as Hallway;

        if (room) _instance._roomPool.Release(room);
        else connector?.area.gameObject.SetActive(false);
        
        if (hallway) _instance._hallwayPool.Release(hallway);
    }

    private static void ConnectConnectors(Transform newArea, Transform newConnector, Transform existingConnector)
    {
        var toTarget = Quaternion.LookRotation(-existingConnector.forward, existingConnector.up);
        var rotOffset = toTarget * Quaternion.Inverse(newConnector.rotation);
        
        newArea.rotation = rotOffset * newArea.rotation;
        newArea.position = existingConnector.position - (newConnector.position - newArea.position);
    }
}