using UnityEngine;
using System.Collections;
using Player;
using Tools;


public class FlashBeacon : ToolBase
{
    public GameObject flashBeaconObj;
    public Collider radius;
    [Header("FlashBeacon Settings")]
    public float flashPeriod = 2f;
    public float coolDownTime = 5f; // 쿨타임을 public으로 지정

    private bool on = false;
    private bool coolDownActive = false;
    
    public GameObject particleEffect;
    public Material laser;

    void Start()
    {
        on = false;
        flashBeaconObj.SetActive(false);
        radius.enabled = false;
    }

    // ToolBase에서 호출되는 메인 함수
    protected override void Use(PlayerController player)
    {
        if (coolDownActive) return;
        Activate();
        StartCoroutine(Flashing());
        StartCoroutine(CoolDownRoutine());
        
        var anomalies = _currentRoom._anomalies;
        var validAnomalies = anomalies.Where(anomaly => anomaly.Active);
        
        foreach (var obj in validAnomalies)
        {
            Vector3 origin = anomalies.startPosition(obj);
            Vector3 currentPosition = obj.transform.position;
            
            GameObject originMarker = Instantiate(particleEffect, origin, Quaternion.identity);
            GameObject currentPositionMarker = Instantiate(particleEffect, currentPosition, Quaternion.identity);
            
            connectPoints(origin, currentPosition);
            
            Destroy(originMarker, 2f);  
            Destroy(currentMarker, 2f);  
            
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
        if (!on)
        {
            flashBeaconObj.SetActive(true);
            radius.enabled = true;
            on = true;
        }
    }

    // FlashBeacon 범위 내 MovingAnomaly 트리거
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MovingAnomaly"))
        {
            var anomaly = other.GetComponent<MovingAnomaly>();
            if (anomaly != null)
            {
                anomaly.OnFlash(); // anomaly의 OnFlash()를 호출 (Freeze 또는 상태 변환)
            }
        }
    }

    // flashPeriod 후 비활성화
    private IEnumerator Flashing()
    {
        yield return new WaitForSeconds(flashPeriod);
        flashBeaconObj.SetActive(false);
        radius.enabled = false;
        on = false;
    }

    // 쿨타임 처리
    private IEnumerator CoolDownRoutine()
    {
        coolDownActive = true;
        yield return new WaitForSeconds(coolDownTime);
        coolDownActive = false;
    }
}
