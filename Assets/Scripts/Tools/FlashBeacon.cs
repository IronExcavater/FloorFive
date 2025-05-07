using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Level;
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
    }

    void Update()
    {
        if (coolDown.coolDownActive) return;
        
        if (Input.GetKeyDown(KeyCode.F))
        {
            OnInteract();
            Activate();
            StartCoroutine(Flashing());
            coolDown.StartCoolDown();
        }

        if (!coolDown.coolDownActive)
        {
            Debug.Log("FlashBeacon ready to flash some underage anomalies babyyy");
        }
    }

    public override void OnInteract()
    {
        Debug.Log("I dont consent to being picked up >:(");
	
    }

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
}
