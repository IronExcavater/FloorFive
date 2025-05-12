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

        [Header("Lightmap Settings")]
        [Tooltip("Material whose emission map will be switched based on anomaly distance")]
        [SerializeField] private Material targetMaterial;
        [Tooltip("Emission/lightmap when no anomaly detected")]
        [SerializeField] private Texture lightmap1;
        [Tooltip("Emission/lightmap for distances between 10m and 7m")]
        [SerializeField] private Texture lightmap2;
        [Tooltip("Emission/lightmap for distances between 7m and 5m")]
        [SerializeField] private Texture lightmap3;
        [Tooltip("Emission/lightmap for distances between 5m and 3m")]
        [SerializeField] private Texture lightmap4;
        [Tooltip("Emission/lightmap for distances under 3m")]
        [SerializeField] private Texture lightmap5;

        protected override void Update()
        {
            base.Update();

            if (!equipped)
                return;

            SimulateConeRaycast();
        }

        protected override void Use(PlayerController player)
        {
            Debug.Log("EMF Reader toggled");
            // You can add toggle logic here if needed
        }

        private void SimulateConeRaycast()
        {
            Vector3 origin = transform.position;
            Vector3 forward = transform.forward;

            float closestHit = float.MaxValue;

            // Shoot multiple rays within cone and record the closest anomaly
            for (int i = 0; i < rayCount; i++)
            {
                Vector3 dir = RandomDirectionInCone(forward, coneAngle);
                if (Physics.Raycast(origin, dir, out RaycastHit hit, coneDistance, hitMask))
                {
                    Debug.DrawRay(origin, dir * hit.distance, Color.red);

                    // Check for "Anomaly" tag
                    if (hit.collider.CompareTag("Anomaly") && hit.distance < closestHit)
                    {
                        closestHit = hit.distance;
                    }
                }
                else
                {
                    Debug.DrawRay(origin, dir * coneDistance, Color.green);
                }
            }

            // Update lightmap based on the closest anomaly distance
            UpdateLightmapByDistance(closestHit);
        }

        private void UpdateLightmapByDistance(float distance)
        {
            Texture chosenMap = lightmap1;

            // No anomaly found
            if (distance == float.MaxValue)
            {
                chosenMap = lightmap1;
            }
            else if (distance > 7f)
            {
                chosenMap = lightmap2;
            }
            else if (distance > 5f)
            {
                chosenMap = lightmap3;
            }
            else if (distance > 3f)
            {
                chosenMap = lightmap4;
            }
            else
            {
                chosenMap = lightmap5;
            }

            // Apply to material emission map
            if (targetMaterial != null && chosenMap != null)
            {
                targetMaterial.EnableKeyword("_EMISSION");
                targetMaterial.SetTexture("_EmissionMap", chosenMap);
                targetMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
        }

        private Vector3 RandomDirectionInCone(Vector3 direction, float angle)
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
