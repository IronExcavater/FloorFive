using UnityEngine;
using Anomaly
public class MovingAnomaly : AnomalyBase
{
    public GameObject player;
    public float moveSpeed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    void OnTriggerEnter(Collider player)
    {
        if (player.gameObject.tag == "Player") 
        {   
            Vector3 newPosition = Vector3.MoveTowards(transform.position, player.transform.position, moveSpeed * Time.deltaTime);
            transform.position = newPosition;
            Debug.Log("Anomaly detected weeem woooo weeee woooooo");
        }
        
        if (player.gameObject.tag == "FlashBeacon") 
        {   
            Vector3 newPosition = Vector3.MoveTowards(transform.position, player.transform.position, moveSpeed * Time.deltaTime);
            transform.position = newPosition;
            Debug.Log("Anomaly detected weeem woooo weeee woooooo");
        }
        
        
        
        
    }
}
