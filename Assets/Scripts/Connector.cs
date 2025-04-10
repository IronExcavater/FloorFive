using UnityEngine;

public class Connector : MonoBehaviour
{
    public Connector connection;
    
    public bool IsViewed
    {
        get
        {
            var camera = Camera.main;
            if (!camera) return false;

            var axis = transform.forward;
            var origin = transform.position;
            origin.y = camera.transform.position.y;
            
            var direction = camera.transform.position - origin;
            var distanceOnAxis = Vector3.Dot(direction, axis);
            var sign = Mathf.Sign(distanceOnAxis);
            
            if (direction.magnitude > 30) return false;
            if (direction.magnitude < 2) return true;

            origin += transform.forward * (distanceOnAxis * 0.3f * sign);
            direction = camera.transform.position - origin;

            if (Physics.Raycast(origin, direction.normalized, out var hit, direction.magnitude))
            {
                Debug.DrawRay(origin, direction.normalized * hit.distance,
                    hit.collider.CompareTag("Player") ? Color.green : Color.red);
                return hit.collider.CompareTag("Player");
            }
            return false;
        }
    }

    public void Connect(Connector connector)
    {
        connection = connector;
        connector.connection = this;
    }

    public void Disconnect()
    {
        if (connection) connection.connection = null;
        connection = null;
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;

        var start = transform.position;
        var direction = transform.forward;
        var end = start + direction * 0.8f;

        Gizmos.DrawLine(start, end);

        var arrowRight = Quaternion.Euler(0, 150, 0) * direction;
        var arrowLeft = Quaternion.Euler(0, -150, 0) * direction;

        Gizmos.DrawLine(end, end + arrowRight * 0.3f);
        Gizmos.DrawLine(end, end + arrowLeft * 0.3f);
    }
}