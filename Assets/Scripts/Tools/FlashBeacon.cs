using UnityEngine;

using Level;
public class FlashBeacon : ToolBase
{
    public GameObject flashBeaconObj;
    private bool on;
    private bool off;


    void Start()
    {
        flashBeaconObj.SetActive(false);
        off = true;
        on = false;
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
        if (off)
        {
            flashBeaconObj.SetActive(true);
            off = false;
            on = true;
            Debug.Log("Let there be light");
        }

        else if (on)
        {
            flashBeaconObj.SetActive(false);
            on = false;
            off = true;
            Debug.Log("Turned off");
        }
        
    }
}
