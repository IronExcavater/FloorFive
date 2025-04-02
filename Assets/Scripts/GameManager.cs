using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Game { get; private set; }

    public Room roomPrefab;
    public Hallway hallwayPrefab;
    public Transform levelTrans;
    
    private ObjectPool<Room> roomPool;
    private ObjectPool<Hallway> hallwayPool;
    
    private void Awake()
    {
        if (Game == null)
        {
            Game = this;
            DontDestroyOnLoad(this);

            roomPool = new ObjectPool<Room>(roomPrefab, 2, levelTrans);
            hallwayPool = new ObjectPool<Hallway>(hallwayPrefab, 3, levelTrans);
        }
        else Destroy(gameObject);
    }

    public static void CreateSection(Connector connector)
    {
        var room = Game.roomPool.Get();
        var roomConnector = room.GetRandomConnector();
        ConnectConnectors(room.transform, roomConnector.transform, connector.transform);
        connector.Connect(roomConnector);
        
        var otherRoomConnector = room.GetOppositeConnector(roomConnector);

        var hallway = Game.hallwayPool.Get();
        var hallwayConnector = hallway.GetRandomConnector();
        ConnectConnectors(hallway.transform, hallwayConnector.transform, otherRoomConnector.transform);
        otherRoomConnector.Connect(hallwayConnector);
    }

    public static void DeleteSection(Connector connector)
    {
        // TODO: Broken due to null errors that shouldn't logically exist
        var room = connector.area as Room;
        var hallway = room?.GetOppositeConnector(connector)?.connection?.area as Hallway;

        if (room)
        {
            room.DisconnectAll();
            Game.roomPool.Release(room);
        }

        if (hallway)
        {
            hallway.DisconnectAll();
            Game.hallwayPool.Release(hallway);
        }
    }

    private static void ConnectConnectors(Transform newArea, Transform newConnector, Transform existingConnector)
    {
        Debug.Log("Connecting " + newArea.name);
        
        var toTarget = Quaternion.LookRotation(-existingConnector.forward, existingConnector.up);
        var rotOffset = toTarget * Quaternion.Inverse(newConnector.rotation);
        
        newArea.rotation = rotOffset * newArea.rotation;
        newArea.position = existingConnector.position - (newConnector.position - newArea.position);
    }
}