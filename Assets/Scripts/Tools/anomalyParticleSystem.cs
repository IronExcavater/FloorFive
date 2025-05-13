using UnityEngine;
using System.Collections;
using Player;
using Tools;
using Anomaly;
using System.Linq;

namespace particleSystem
{
    public class anomalyParticleSystem : ToolBase
    {
        public GameObject particleEffect;
        public Material laser;
        protected override void Use(PlayerController player)
        {

            var anomalies = _currentRoom._anomalies;
            var validAnomalies = anomalies.Where(anomaly => anomaly.Active);

            foreach (var obj in validAnomalies)
            {
                Vector3 origin = obj._startPos;
                Vector3 currentPosition = obj.transform.position;

                GameObject originMarker = Instantiate(particleEffect, origin, Quaternion.identity);
                GameObject currentPositionMarker = Instantiate(particleEffect, currentPosition, Quaternion.identity);

                connectPoints(origin, currentPosition);

                Destroy(originMarker, 2f);
                Destroy(currentPositionMarker, 2f);

            }
        }

        private void connectPoints(Vector3 start, Vector3 end)
        {
            GameObject line = new GameObject("line");
            var lineRenderer = line.AddComponent<LineRenderer>();

            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);

            lineRenderer.material = laser;
            lineRenderer.startWidth = 0.05f;
            lineRenderer.endWidth = 0.05f;

            Destroy(line, 2f);
        }

    }
}
