using UnityEngine;
using Player;

namespace Tools
{
    public class EMFReader : ToolBase
    {
        [Header("Lightmap Settings")]
        [Tooltip("Material whose emission map will be switched based on anomaly distance")]
        [SerializeField] private Material targetMaterial;
        [Tooltip("Emission/lightmap when no anomaly or >5m away")]
        [SerializeField] private Texture lightmap1;
        [Tooltip("Emission/lightmap for distances between 5m and 4m")]
        [SerializeField] private Texture lightmap2;
        [Tooltip("Emission/lightmap for distances between 4m and 3m")]
        [SerializeField] private Texture lightmap3;
        [Tooltip("Emission/lightmap for distances between 3m and 2m")]
        [SerializeField] private Texture lightmap4;
        [Tooltip("Emission/lightmap for distances under 2m")]
        [SerializeField] private Texture lightmap5;

        protected override void Update()
        {
            base.Update();

            if (!equipped)
                return;

            UpdateAnomalyDistanceLightmap();
        }

        protected override void Use(PlayerController player)
        {
            Debug.Log("EMF Reader used");
            // Optional toggle logic
        }

        /// <summary>
        /// Finds the closest "Anomaly" tagged object and updates the emission map accordingly.
        /// </summary>
        private void UpdateAnomalyDistanceLightmap()
        {
            GameObject[] anomalies = GameObject.FindGameObjectsWithTag("Anomaly");
            float closestDistance = float.MaxValue;

            foreach (var obj in anomalies)
            {
                float dist = Vector3.Distance(transform.position, obj.transform.position);
                if (dist < closestDistance)
                    closestDistance = dist;
            }

            ApplyLightmapBasedOnDistance(closestDistance);
        }

        /// <summary>
        /// Chooses the correct map based on distance thresholds:
        /// >5m => lightmap1, 5-4m => lightmap2, 4-3m => lightmap3,
        /// 3-2m => lightmap4, <2m => lightmap5.
        /// </summary>
        private void ApplyLightmapBasedOnDistance(float distance)
        {
            Texture chosenMap = lightmap1;

            if (distance <= 2f)
            {
                chosenMap = lightmap5;
            }
            else if (distance <= 3f)
            {
                chosenMap = lightmap4;
            }
            else if (distance <= 4f)
            {
                chosenMap = lightmap3;
            }
            else if (distance <= 5f)
            {
                chosenMap = lightmap2;
            }
            else
            {
                chosenMap = lightmap1;
            }

            if (targetMaterial != null && chosenMap != null)
            {
                targetMaterial.EnableKeyword("_EMISSION");
                targetMaterial.SetTexture("_EmissionMap", chosenMap);
                targetMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
        }
    }
}
