using UnityEngine;
using Player;

namespace Tools
{
    public class EMFReader : ToolBase
    {
        [Header("Cone Settings")]
        [Range(0.1f, 90f)]
        public float coneAngle = 5f;
        public float coneDistance = 10f;
        public int rayCount = 100;
        public LayerMask hitMask;

        protected override void Update()
        {
            base.Update();

            if (!_equipped) return;

            SimulateConeRaycast();
        }

        protected override void Use(PlayerController player)
        {
            Debug.Log("EMF Reader used");
            // Optional: Implement toggling behavior or activation effects here
        }

        void SimulateConeRaycast()
        {
            Vector3 origin = transform.position;
            Vector3 forward = transform.forward;

            for (int i = 0; i < rayCount; i++)
            {
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

        Vector3 RandomDirectionInCone(Vector3 direction, float angle)
        {
            float angleRad = angle * Mathf.Deg2Rad;
            float u = Random.value;
            float v = Random.value;

            float theta = 2f * Mathf.PI * u;
            float phi = Mathf.Acos(1 - v * (1 - Mathf.Cos(angleRad)));

            float x = Mathf.Sin(phi) * Mathf.Cos(theta);
            float y = Mathf.Sin(phi) * Mathf.Sin(theta);
            float z = Mathf.Cos(phi);

            Vector3 localDir = new Vector3(x, y, z);
            return Quaternion.LookRotation(direction) * localDir;
        }
    }
}