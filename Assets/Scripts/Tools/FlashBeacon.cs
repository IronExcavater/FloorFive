using UnityEngine;
using System.Collections;
using Player;
using Tools;
using particleSystem;


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
