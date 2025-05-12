using UnityEngine;
using Anomaly;

public class MovingAnomaly : AnomalyBase
{
    public GameObject player;
    public float moveSpeed = 2f;
    private bool isFrozen = false;

    void Start()
    {
        // Find the player if not set in inspector
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        if (!isFrozen && Active && player != null)
        {
            // Move towards player
            Vector3 direction = (player.transform.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
        }
    }

    // Called by FlashBeacon when anomaly is hit
    public void Freeze()
    {
        isFrozen = true;
        // Optionally play freeze animation/effect
        Debug.Log("MovingAnomaly frozen by FlashBeacon!");
        // Optionally, start a coroutine to unfreeze after some time
    }

    // Optional: Unfreeze after some time or on event
    public void Unfreeze()
    {
        isFrozen = false;
        Debug.Log("MovingAnomaly unfrozen!");
    }
}
