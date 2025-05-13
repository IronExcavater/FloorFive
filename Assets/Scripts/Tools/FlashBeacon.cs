using UnityEngine;
using System.Collections;
using System.Linq;
using Player;

namespace Tools
{
    public class FlashBeacon : ToolBase
    {
        [Header("FlashBeacon Settings")]
        public float flashPeriod = 2f;
        public float coolDownTime = 5f;

        private bool on = false;
        private bool coolDownActive = false;

        public GameObject lightSource;
        
        public GameObject particleEffect;
        public Material laser;
        

        void Start()
        {
            on = false;
            lightSource.SetActive(false);
            
        }
        
        protected override void Use(PlayerController player)
        {
            if (coolDownActive) return;
            Activate();
            StartCoroutine(Flashing());
            StartCoroutine(CoolDownRoutine());
        }

        private void revealAnomalies()
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

        private void Activate()
        {
            on = true;
            lightSource.SetActive(true);
            revealAnomalies();
        }

        // flashPeriod 후 비활성화
        private IEnumerator Flashing()
        {
            yield return new WaitForSeconds(flashPeriod);
            on = false;
            lightSource.SetActive(false);
        }

        // 쿨타임 처리
        private IEnumerator CoolDownRoutine()
        {
            coolDownActive = true;
            yield return new WaitForSeconds(coolDownTime);
            coolDownActive = false;
        }
    }
}
