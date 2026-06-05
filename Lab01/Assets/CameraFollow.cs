using UnityEngine;

/// <summary>
/// Keeps the camera locked to the player with a fixed world-space offset.
/// The offset preserves whatever framing the camera had when it was set up.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(9f, 6f, 0f);
    [SerializeField] private float smoothSpeed = 10f;
    [SerializeField] private bool lookAtTarget = true;
    [SerializeField] private Vector3 lookAtOffset = new Vector3(0f, 1.5f, 0f);

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desired = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);

        if (lookAtTarget)
            transform.LookAt(target.position + lookAtOffset);
    }
}
