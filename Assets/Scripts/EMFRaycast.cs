using UnityEngine;

public class ConeRaycaster : MonoBehaviour
{
    [Header("Cone Settings")]
    [Range(0.1f, 90f)]
    public float coneAngle = 5f;       // Half-angle of the cone in degrees
    public float coneDistance = 10f;   // How far each ray goes
    public int rayCount = 100;         // Number of rays
    public LayerMask hitMask;

    void Update()
    {
        SimulateConeRaycast();
    }

    void SimulateConeRaycast()
    {
        Vector3 origin = transform.position;
        Vector3 forward = transform.forward;

        for (int i = 0; i < rayCount; i++)
        {
            // Generate a random direction inside the cone
            Vector3 randomDirection = RandomDirectionInCone(forward, coneAngle);

            if (Physics.Raycast(origin, randomDirection, out RaycastHit hit, coneDistance, hitMask))
            {
                Debug.DrawRay(origin, randomDirection * hit.distance, Color.red);
                Debug.Log("Hit: " + hit.collider.name);
            }
            else
            {
                Debug.DrawRay(origin, randomDirection * coneDistance, Color.green);
            }
        }
    }

    // Generates a direction within a cone using spherical interpolation
    Vector3 RandomDirectionInCone(Vector3 direction, float angle)
    {
        float angleRad = angle * Mathf.Deg2Rad;

        // Random point on a circle
        float u = Random.value;
        float v = Random.value;

        float theta = 2f * Mathf.PI * u;      // Azimuthal angle
        float phi = Mathf.Acos(1 - v * (1 - Mathf.Cos(angleRad)));  // Polar angle

        float x = Mathf.Sin(phi) * Mathf.Cos(theta);
        float y = Mathf.Sin(phi) * Mathf.Sin(theta);
        float z = Mathf.Cos(phi);

        // Convert from local to world space
        Vector3 localDir = new Vector3(x, y, z);
        return Quaternion.LookRotation(direction) * localDir;
    }
}