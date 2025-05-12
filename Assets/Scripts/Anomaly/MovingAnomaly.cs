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
        StartCoroutine(FlashRed());
    }

    IEnumerator FlashRed()
    {
        if (anomalyRenderer != null && !isFlashing)
        {
            isFlashing = true;
            anomalyRenderer.material.color = flashColor;

            yield return new WaitForSeconds(flashDuration);

            anomalyRenderer.material.color = originalColor;
            isFlashing = false;
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