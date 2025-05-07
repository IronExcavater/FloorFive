using UnityEngine;

using Level;
public class FlashBeacon : ToolBase
{
    public GameObject flashBeaconObj;
    
    public bool on;
    
    void Start()
    {
        on = false;
        flashBeaconObj.SetActive(false);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            OnInteract();
        }
    }
    public override void OnInteract()
    {
        if (!on)
        { 
            flashBeaconObj.SetActive(true);
            on = true;
            Debug.Log("FlashBeacon on");
        }

        else if (on)
        {
            flashBeaconObj.SetActive(false);
            on = false;
            Debug.Log("FlashBeacon off");
        }
    }
}
