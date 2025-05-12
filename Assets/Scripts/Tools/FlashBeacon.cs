using UnityEngine;
using System.Collections;
using Player;
using Tools;

public class FlashBeacon : ToolBase
{
    public GameObject flashBeaconObj;
    public Collider radius;
    [SerializeField] public CoolDown coolDown;
    public float flashPeriod = 2f;
    private bool on = false;

    void Start()
    {
        on = false;
        flashBeaconObj.SetActive(false);
        radius.enabled = false;
    }

    // ToolBase에서 호출되는 사용 함수
    protected override void Use(PlayerController player)
    {
        if (coolDown.coolDownActive) return;
        Activate();
        StartCoroutine(Flashing());
        coolDown.StartCoolDown();
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

    // FlashBeacon의 범위에 들어온 MovingAnomaly를 정지시킴
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MovingAnomaly"))
        {
            var anomaly = other.GetComponent<MovingAnomaly>();
            if (anomaly != null)
            {
                anomaly.Freeze(flashPeriod); // flashPeriod 동안 정지
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
}
