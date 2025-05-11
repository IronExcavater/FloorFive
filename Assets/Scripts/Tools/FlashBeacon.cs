using UnityEngine;
using System.Collections;
using Player;
using Tools;

public class FlashBeacon : ToolBase
{
    public GameObject flashBeaconObj;
	public Collider radius;
    [SerializeField] public CoolDown coolDown;
    public bool on;
    public float flashPeriod;
    
    void Start()
    {
        on = false;
        flashBeaconObj.SetActive(false);
		radius.enabled = false;
		Debug.Log("okey lesh do this");
    }
/*
    protected override void Update()
    {
		base.Update();
        if (coolDown.coolDownActive) return;
        
        if (Input.GetKeyDown(KeyCode.F))
        {
            Activate();
            StartCoroutine(Flashing());
            coolDown.StartCoolDown();
        }

        if (!coolDown.coolDownActive)
        {
            Debug.Log("FlashBeacon ready to flash some underage anomalies babyyy");
        }
    }
*/

    void Activate()
    {
        if (!on)
        { 
        flashBeaconObj.SetActive(true);
		radius.enabled = true;
        on = true;

        Debug.Log("FlashBeacon Flashing");
        }
    }

	void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == "MovingAnomaly") 
        {
         	Debug.Log("Anomaly detected weeem woooo weeee woooooo");
        }
	}

    public IEnumerator Flashing()
    {
        yield return new WaitForSeconds(flashPeriod);
        flashBeaconObj.SetActive(false);
		radius.enabled = false;
        on = false;
        Debug.Log("FlashBeacon on cooldown");
    }

    protected override void Use(PlayerController player)
    {
		if (coolDown.coolDownActive) return;
		Debug.Log("thing is happening");
        Activate();
		StartCoroutine(Flashing());
        coolDown.StartCoolDown();
    }
}
