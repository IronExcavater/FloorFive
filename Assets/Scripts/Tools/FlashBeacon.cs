using UnityEngine;

using Level
public class FlashBeacon : ToolBase
{
    public GameObject FlashBeacon;
    public bool FlashBeaconActive = false;


    public override void OnInteract()
    {
        if (FlashBeaconActive == false && Input.GetKeyDown("F"))
        {
            FlashBeacon.SetActive(true);
            FlashBeaconActive = true;
            Debug.Log("Let there be light");
        }
        
        else if (FlashBeaconActive == true && Input.GetKeyDown("F"))
        {
            FlashBeacon.SetActive(false);
            FlashBeaconActive = false;
            Debug.Log("Turned off")
        }
    }
    
    
}
