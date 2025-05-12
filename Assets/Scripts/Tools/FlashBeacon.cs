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
    public float coolDownTime = 5f; 

    private bool on = false;
    private bool coolDownActive = false;

    void Start()
    {
        on = false;
        flashBeaconObj.SetActive(false);
        radius.enabled = false;
    }


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


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MovingAnomaly"))
        {
            var anomaly = other.GetComponent<MovingAnomaly>();
            if (anomaly != null)
            {
                anomaly.OnFlash(); 
            }
        }
    }


    private IEnumerator Flashing()
    {
        yield return new WaitForSeconds(flashPeriod);
        flashBeaconObj.SetActive(false);
        radius.enabled = false;
        on = false;
    }


    private IEnumerator CoolDownRoutine()
    {
        coolDownActive = true;
        yield return new WaitForSeconds(coolDownTime);
        coolDownActive = false;
    }
}
