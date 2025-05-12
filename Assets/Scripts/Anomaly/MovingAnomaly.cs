using UnityEngine;
using Anomaly;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(SphereCollider))]
public class MovingAnomaly : AnomalyBase
{
    public GameObject player;
    public float moveSpeed = 2f;
    public float stopDistance = 1.5f;
    public AudioClip awakeSound;
    public AudioClip deactivateSound;

    private AudioSource audioSource;

    private enum State { Dormant, Awake }
    private State currentState = State.Dormant;

    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");

        audioSource = GetComponent<AudioSource>();

        // Collider 세팅: 트리거로 설정
        var col = GetComponent<SphereCollider>();
        col.isTrigger = true;

        // Rigidbody가 있다면 isKinematic으로
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

    // 트리거로 플레이어 감지
    void OnTriggerEnter(Collider other)
    {
        if (currentState == State.Dormant && other.CompareTag("Player"))
        {
            BecomeAwake();
        }
    }

    // FlashBeacon 등에서 이 메서드를 호출
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
        if (awakeSound != null) audioSource.PlayOneShot(awakeSound);
        // 필요하다면 이펙트/애니메이션 등 추가
        Debug.Log("Anomaly is now awake!");
    }

    void BecomeDormant()
    {
        currentState = State.Dormant;
        if (deactivateSound != null) audioSource.PlayOneShot(deactivateSound);
        // 필요하다면 이펙트/애니메이션 등 추가
        Debug.Log("Anomaly is now dormant!");
    }
}
