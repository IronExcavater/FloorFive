using UnityEngine;
using Anomaly;

[RequireComponent(typeof(SphereCollider))]
public class MovingAnomaly : AnomalyBase
{
    public GameObject player;
    public float moveSpeed = 2f;
    public float stopDistance = 1.5f;
    public AudioClip awakeSound;
    public AudioClip deactivateSound;
    
    private enum State { Dormant, Awake }
    private State currentState = State.Dormant;

    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");

        // Collider ����: Ʈ���ŷ� ����
        var col = GetComponent<SphereCollider>();
        col.isTrigger = true;

        // Rigidbody�� �ִٸ� isKinematic����
        var rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
    }

    void Update()
    {
        if (currentState == State.Awake && player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            if (distanceToPlayer > stopDistance)
            {
                Vector3 direction = (player.transform.position - transform.position).normalized;
                transform.position += direction * moveSpeed * Time.deltaTime;
            }
        }
    }

    // Ʈ���ŷ� �÷��̾� ����
    void OnTriggerEnter(Collider other)
    {
        if (currentState == State.Dormant && other.CompareTag("Player"))
        {
            BecomeAwake();
        }
    }

    // FlashBeacon ��� �� �޼��带 ȣ��
    public void OnFlash()
    {
        if (currentState == State.Dormant)
        {
            BecomeAwake();
        }
        else if (currentState == State.Awake)
        {
            BecomeDormant();
        }
    }

    void BecomeAwake()
    {
        currentState = State.Awake;
        if (awakeSound != null) _audioSource.PlayOneShot(awakeSound);
        // �ʿ��ϴٸ� ����Ʈ/�ִϸ��̼� �� �߰�
        Debug.Log("Anomaly is now awake!");
    }

    void BecomeDormant()
    {
        currentState = State.Dormant;
        if (deactivateSound != null) _audioSource.PlayOneShot(deactivateSound);
        // �ʿ��ϴٸ� ����Ʈ/�ִϸ��̼� �� �߰�
        Debug.Log("Anomaly is now dormant!");
    }
}
