using UnityEngine;

public class Area : MonoBehaviour
{
    public Connection[] connections;
    
    public void DestroyConnections()
    {
        foreach (var connection in connections)
        {
            if (connection.other == null) continue;
            Destroy(connection.other.transform.parent.gameObject);
        }
    }
}
