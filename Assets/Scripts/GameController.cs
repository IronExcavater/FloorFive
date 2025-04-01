using Unity.Mathematics.Geometry;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private static GameController Game { get; set; }
    
    [SerializeField] private GameObject[] checkpointPrefabs;
    [SerializeField] private GameObject[] roomPrefabs;
    
    public int numberOfRooms;
    
    private void Awake()
    {
        if (Game == null)
        {
            Game = this;
            DontDestroyOnLoad(this);
        }
        else Destroy(gameObject);
    }

    private void Start()
    {
        Game.numberOfRooms = Random.Range(2, 6);
    }

    public static Connection CreateRoom(Connection connection)
    {
        Debug.Log("Creating room");
        
        var roomObj = Instantiate(Game.numberOfRooms > 0
            ? Game.roomPrefabs[Random.Range(0, Game.roomPrefabs.Length)]
            : Game.checkpointPrefabs[Random.Range(0, Game.checkpointPrefabs.Length)]);
        
        if (Game.numberOfRooms <= 0) Game.numberOfRooms = Random.Range(2, 6);
        Game.numberOfRooms--;
        
        var room = roomObj.GetComponent<Room>();
        var roomConnection = room.connections[Random.Range(0, room.connections.Length)];
        
        var rotOffset = roomConnection.transform.rotation.eulerAngles.y - 180 - connection.transform.rotation.eulerAngles.y;
        Debug.Log("newCon: " + roomConnection.transform.rotation.eulerAngles.y + " , oldCon: " + connection.transform.rotation.eulerAngles.y + " , rotOffset: " + rotOffset);
        roomObj.transform.Rotate(new Vector3(0, rotOffset, 0), Space.Self);
        
        var posOffset = roomConnection.transform.position - roomObj.transform.position;
        roomObj.transform.position = connection.transform.position - posOffset;
        
        roomConnection.other = connection;
        return roomConnection;
    }
}
