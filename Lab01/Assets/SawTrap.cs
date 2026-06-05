using UnityEngine;

public class SawTrap : MonoBehaviour
{
    [SerializeField] private float damage = 20f;
    [SerializeField] private float rotateSpeed = 360f;

    [Tooltip("Local axis the saw spins around. Try (1,0,0), (0,0,1) or (0,1,0) until it looks right.")]
    [SerializeField] private Vector3 rotateAxis = Vector3.right;

    void Update()
    {
        transform.Rotate(rotateAxis.normalized, rotateSpeed * Time.deltaTime, Space.Self);
    }

    private void OnTriggerEnter(Collider other)
    {
        TryDamage(other);
    }

    private void OnTriggerStay(Collider other)
    {
        // Keeps dealing damage while the player stands in the saw.
        // PlayerHealth has its own cooldown, so this is rate-limited automatically.
        TryDamage(other);
    }

    private void TryDamage(Collider other)
    {
        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
        if (health != null)
            health.TakeDamage(damage);
    }
}
