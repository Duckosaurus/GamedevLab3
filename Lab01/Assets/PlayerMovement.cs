using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 playerVelocity;
    private Vector3 platformVelocity;
    private bool groundedPlayer;
    private bool wasGroundedLastFrame;

    [SerializeField] private float playerSpeed = 5.0f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravityValue = -9.81f;
    [SerializeField] private float stompBounceHeight = 2.0f;

    public bool IsGrounded => groundedPlayer;
    public bool IsMoving { get; private set; }
    public bool JumpedThisFrame { get; private set; }
    public bool LandedThisFrame { get; private set; }
    public float VerticalVelocity => playerVelocity.y;

    void Start()
    {
        controller = GetComponent<CharacterController>();
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

        wasGroundedLastFrame = groundedPlayer;
        groundedPlayer = controller.isGrounded;

        JumpedThisFrame = false;
        LandedThisFrame = false;

        if (groundedPlayer && !wasGroundedLastFrame)
            LandedThisFrame = true;

        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f;
        }

        Vector3 move = new Vector3(-Input.GetAxis("Horizontal"), 0, -Input.GetAxis("Vertical"));
        IsMoving = move.sqrMagnitude > 0.01f;
        controller.Move(move * Time.deltaTime * playerSpeed);

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
                return;
            }
        }
        platformVelocity = Vector3.zero;
    }
}