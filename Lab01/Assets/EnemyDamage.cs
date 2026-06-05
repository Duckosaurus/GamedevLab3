using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    [SerializeField] private float damage = 25f;

    private void OnTriggerEnter(Collider other)
    {
        PlayerHealth health = other.GetComponent<PlayerHealth>();
        if (health == null) return;

        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player == null) return;

        bool playerAbove = other.transform.position.y > transform.position.y + 0.5f;
        bool playerFalling = player.VerticalVelocity < 0f;

        if (playerAbove && playerFalling) return;

        health.TakeDamage(damage);
    }
}
