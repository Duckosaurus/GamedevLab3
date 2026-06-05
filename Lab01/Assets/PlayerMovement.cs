using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 playerVelocity;
    private Vector3 platformVelocity;
    private bool groundedPlayer;
    private float airborneTime;

    [SerializeField] private float playerSpeed = 5.0f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravityValue = -9.81f;
    [SerializeField] private float stompBounceHeight = 2.0f;
    [SerializeField] private float rotationSpeed = 12f;
    [Tooltip("Minimum airtime (s) before a touchdown counts as a landing. Prevents land-sound spam on moving platforms.")]
    [SerializeField] private float landThreshold = 0.12f;
    [SerializeField] private Transform cameraTransform;

    private PlayerHealth playerHealth;

    public bool IsGrounded => groundedPlayer;
    public bool IsMoving { get; private set; }
    public bool JumpedThisFrame { get; private set; }
    public bool LandedThisFrame { get; private set; }
    public float VerticalVelocity => playerVelocity.y;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerHealth = GetComponent<PlayerHealth>();
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsDead)
        {
            IsMoving = false;
            JumpedThisFrame = false;
            LandedThisFrame = false;
            return;
        }

        JumpedThisFrame = false;
        LandedThisFrame = false;

        // isGrounded flickers on moving platforms, so only count a landing
        // after the player has actually been airborne for a short moment.
        bool rawGrounded = controller.isGrounded;
        if (rawGrounded)
        {
            if (airborneTime > landThreshold)
                LandedThisFrame = true;
            airborneTime = 0f;
        }
        else
        {
            airborneTime += Time.deltaTime;
        }
        groundedPlayer = rawGrounded;

        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f;
        }

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // Camera-relative movement.
        Vector3 move;
        if (cameraTransform != null)
        {
            Vector3 camForward = cameraTransform.forward;
            Vector3 camRight = cameraTransform.right;
            camForward.y = 0f; camRight.y = 0f;
            camForward.Normalize(); camRight.Normalize();
            move = camForward * v + camRight * h;
        }
        else
        {
            move = new Vector3(h, 0, v);
        }

        if (move.sqrMagnitude > 1f) move.Normalize();
        IsMoving = move.sqrMagnitude > 0.01f;
        controller.Move(move * Time.deltaTime * playerSpeed);

        // Rotate the player to face the movement direction (turns with the camera).
        if (IsMoving)
        {
            Quaternion targetRot = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        if (Input.GetButtonDown("Jump") && groundedPlayer)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
            JumpedThisFrame = true;
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }

    void FixedUpdate()
    {
        CheckForPlatform();

        if (groundedPlayer && platformVelocity != Vector3.zero)
        {
            controller.Move(platformVelocity * Time.fixedDeltaTime);
        }
    }

    public void StompBounce()
    {
        playerVelocity.y = Mathf.Sqrt(stompBounceHeight * -3.0f * gravityValue);
    }

    private void CheckForPlatform()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 1.2f, LayerMask.GetMask("Platforms")))
        {
            MovingPlatform platform = hit.collider.GetComponent<MovingPlatform>();
            if (platform != null)
            {
                platformVelocity = platform.GetVelocity();

                if (platform.DealsDamage && groundedPlayer && playerHealth != null)
                    playerHealth.TakeDamage(platform.DamageAmount);

                return;
            }
        }
        platformVelocity = Vector3.zero;
    }
}