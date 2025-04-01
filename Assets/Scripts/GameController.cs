using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour
{
    private static GameController Game { get; set; }
    
    [SerializeField] private GameObject[] checkpointPrefabs;
    [SerializeField] private GameObject[] roomPrefabs;

    public static Action<int> OnScoreChanged;
    
    public int numberOfRooms;
    public int score;
    
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

    public static Connection CreateArea(Connection connection)
    {
        var areaObj = Instantiate(Game.numberOfRooms > 0
            ? Game.roomPrefabs[Random.Range(0, Game.roomPrefabs.Length)]
            : Game.checkpointPrefabs[Random.Range(0, Game.checkpointPrefabs.Length)]);
        
        if (Game.numberOfRooms <= 0) Game.numberOfRooms = Random.Range(2, 6);
        Game.numberOfRooms--;
        
        var area = areaObj.GetComponent<Area>();
        var newConnection = area.connections[Random.Range(0, area.connections.Length)];
        
        var rotOffset = newConnection.transform.rotation.eulerAngles.y - 180 - connection.transform.rotation.eulerAngles.y;
        areaObj.transform.Rotate(new Vector3(0, rotOffset, 0), Space.Self);
        
        var posOffset = newConnection.transform.position - areaObj.transform.position;
        areaObj.transform.position = connection.transform.position - posOffset;
        
        newConnection.other = connection;
        return newConnection;
    }

    public static void RoomCompleted(bool isSuccessful)
    {
        Debug.Log("Room completed, successful = " + isSuccessful);
        if (isSuccessful) Game.score++;
        else Game.score = 0;
        OnScoreChanged?.Invoke(Game.score);
    }
}
