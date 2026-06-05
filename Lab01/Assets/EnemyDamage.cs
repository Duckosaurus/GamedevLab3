using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    [SerializeField] private float damage = 25f;

    private void OnTriggerEnter(Collider other)
    {
        TryDamage(other);
    }

    private void OnTriggerStay(Collider other)
    {
        // Keeps hurting the player while in contact (rate-limited by PlayerHealth's cooldown).
        TryDamage(other);
    }

    private void TryDamage(Collider other)
    {
        PlayerMovement player = other.GetComponentInParent<PlayerMovement>();
        if (player == null) return;

        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
        if (health == null) return;

        // Skip damage when the player is stomping from above; EnemyPatrol handles that kill.
        bool playerAbove = player.transform.position.y > transform.position.y + 0.5f;
        bool playerFalling = player.VerticalVelocity < 0f;
        if (playerAbove && playerFalling) return;

        health.TakeDamage(damage);
    }
}
