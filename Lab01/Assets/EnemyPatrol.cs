using System.Collections;
using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Patrol")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float moveSpeed = 2f;

    [Header("Stomp")]
    [SerializeField] private AudioClip squashSound;
    [SerializeField] private float squashVolume = 1f;
    [SerializeField] private float squashDuration = 0.3f;
    [SerializeField] private float destroyDelay = 0.15f;

    private Transform currentTarget;
    private Animator animator;
    private AudioSource audioSource;
    private bool isDead;

    void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        currentTarget = pointB;

        if (animator != null)
        {
            animator.SetFloat("Vert", 0.5f);
            animator.SetFloat("State", 0f);
        }
    }

    void Update()
    {
        if (isDead || pointA == null || pointB == null) return;

        Vector3 direction = (currentTarget.position - transform.position);
        direction.y = 0;

        if (direction.magnitude < 0.3f)
        {
            currentTarget = currentTarget == pointA ? pointB : pointA;
            return;
        }

        transform.rotation = Quaternion.LookRotation(direction.normalized);
        transform.position += direction.normalized * moveSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDead) return;

        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player == null) return;

        bool playerAbove = other.transform.position.y > transform.position.y + 0.5f;
        bool playerFalling = player.VerticalVelocity < 0f;

        if (playerAbove && playerFalling)
        {
            player.StompBounce();
            StartCoroutine(SquashAndDestroy());
        }
    }

    private IEnumerator SquashAndDestroy()
    {
        isDead = true;

        if (animator != null)
        {
            animator.SetFloat("Vert", 0f);
            animator.enabled = false;
        }

        if (squashSound != null && audioSource != null)
            audioSource.PlayOneShot(squashSound, squashVolume);

        Vector3 originalScale = transform.localScale;
        Vector3 squashedScale = new Vector3(originalScale.x * 1.5f, originalScale.y * 0.1f, originalScale.z * 1.5f);

        float elapsed = 0f;
        while (elapsed < squashDuration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(originalScale, squashedScale, elapsed / squashDuration);
            yield return null;
        }

        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }
}
