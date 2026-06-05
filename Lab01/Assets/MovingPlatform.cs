using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private float platformSpeed = 2f;
    [SerializeField] private Vector3 start;
    [SerializeField] private Vector3 end;

    [Header("Damage")]
    [Tooltip("If enabled, standing on this platform hurts the player.")]
    [SerializeField] private bool dealsDamage = false;
    [SerializeField] private float damage = 10f;

    public bool DealsDamage => dealsDamage;
    public float DamageAmount => damage;

    private Vector3 lastPosition;

    void FixedUpdate()
    {
        float pingPong = Mathf.PingPong(Time.fixedTime * platformSpeed, 1.0f);
        Vector3 newPosition = Vector3.Lerp(start, end, pingPong);

        transform.localPosition = newPosition;
    }

    public Vector3 GetVelocity()
    {
        Vector3 velocity = (transform.localPosition - lastPosition) / Time.fixedDeltaTime;
        lastPosition = transform.localPosition;
        return velocity;
    }
}