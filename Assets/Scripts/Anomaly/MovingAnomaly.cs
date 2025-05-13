using UnityEngine;
using Anomaly;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(SphereCollider))]
public class MovingAnomaly : AnomalyBase
{
    public GameObject player;
    public AudioClip awakeSound;
    public AudioClip deactivateSound;
    public Color flashColor = Color.red;         
    public float flashDuration = 0.2f;             
    private AudioSource audioSource;
    private Renderer anomalyRenderer;
    private Color originalColor;
    private bool isFlashing = false;

    
    private enum State { Dormant, Awake }
    private State currentState = State.Dormant;

    

    void Start()
    {
        
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");

        audioSource = GetComponent<AudioSource>();

        var col = GetComponent<SphereCollider>();
        col.isTrigger = true;

        var rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        anomalyRenderer = GetComponent<Renderer>();
        if (anomalyRenderer != null)
        {
            originalColor = anomalyRenderer.material.color;
        }
        else
        {
            Debug.LogWarning("No Renderer found on anomaly object!");
        }
    }
    

    void OnTriggerEnter(Collider other)
    {
        if (currentState == State.Dormant && other.CompareTag("Player"))
        {
            BecomeAwake();
        }
    }

    public void OnFlash()
    {
        StartCoroutine(showPosition());
    }

    IEnumerator showPosition();
    {
        if (anomalyRenderer != null && !isFlashing)
        {
            
        }
    }

    void BecomeAwake()
    {
        currentState = State.Awake;
        if (awakeSound != null) audioSource.PlayOneShot(awakeSound);
        Debug.Log("Anomaly is now awake!");
    }

    void BecomeDormant()
    {
        currentState = State.Dormant;
        if (deactivateSound != null) audioSource.PlayOneShot(deactivateSound);
        Debug.Log("Anomaly is now dormant!");
    }
}