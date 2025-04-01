using UnityEngine;

public class Area : MonoBehaviour
{
    public Connector[] connectors;

    public Connector GetOppositeConnector(Connector connector)
    {
        return connector.Equals(connectors[0]) ? connectors[1] : connectors[0];
    }

    public Connector GetRandomConnector()
    {
        return connectors[Random.Range(0, connectors.Length)];
    }

    public void DisconnectAll()
    {
        foreach (var connector in connectors) connector.Disconnect();
    }
}