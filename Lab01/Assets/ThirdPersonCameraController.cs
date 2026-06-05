using UnityEngine;

/// <summary>
/// Mouse-controlled third-person camera. Orbits behind/above the player.
/// Move the mouse to rotate around the player. Locks the cursor during play,
/// and releases it on the Game Over / Victory screen so UI buttons stay clickable.
/// </summary>
public class ThirdPersonCameraController : MonoBehaviour
{
    [SerializeField] private Transform target;

    [Header("Framing")]
    [SerializeField] private float distance = 6f;
    [SerializeField] private float height = 2f;

    [Header("Mouse")]
    [SerializeField] private float mouseSensitivity = 3f;
    [SerializeField] private float minPitch = -10f;
    [SerializeField] private float maxPitch = 70f;
    [SerializeField] private float startPitch = 20f;

    private float yaw;
    private float pitch;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = startPitch;
    }

    void LateUpdate()
    {
        if (target == null) return;

        bool dead = GameManager.Instance != null && GameManager.Instance.IsDead;

        if (dead)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
            pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 lookPoint = target.position + Vector3.up * height;
        Vector3 desiredPos = lookPoint + rotation * new Vector3(0f, 0f, -distance);

        transform.position = desiredPos;
        transform.LookAt(lookPoint);
    }
}
