using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float damageCooldown = 1f;

    private float lastDamageTime = -999f;

    public void TakeDamage(float amount)
    {
        if (Time.time - lastDamageTime < damageCooldown) return;
        lastDamageTime = Time.time;

        if (GameManager.Instance != null)
            GameManager.Instance.TakeDamage(amount);
    }
}
