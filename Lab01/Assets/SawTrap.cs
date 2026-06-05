using UnityEngine;

public class SawTrap : MonoBehaviour
{
    [SerializeField] private float damage = 20f;
    [SerializeField] private float rotateSpeed = 360f;

    void Update()
    {
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerHealth health = other.GetComponent<PlayerHealth>();
        if (health != null)
            health.TakeDamage(damage);
    }
}
