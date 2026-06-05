using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(AudioSource))]
public class PlayerSoundEffects : MonoBehaviour
{
    [Header("Footsteps")]
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField] private float footstepInterval = 0.35f;
    [SerializeField] private float footstepVolume = 0.5f;

    [Header("Jump")]
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private float jumpVolume = 0.7f;

    [Header("Landing")]
    [SerializeField] private AudioClip landClip;
    [SerializeField] private float landVolume = 0.6f;

    private PlayerMovement playerMovement;
    private AudioSource audioSource;
    private float footstepTimer;

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        HandleFootsteps();

        if (playerMovement.JumpedThisFrame && jumpClip != null)
            audioSource.PlayOneShot(jumpClip, jumpVolume);

        if (playerMovement.LandedThisFrame && landClip != null)
            audioSource.PlayOneShot(landClip, landVolume);
    }

    private void HandleFootsteps()
    {
        if (!playerMovement.IsGrounded || !playerMovement.IsMoving || footstepClips.Length == 0)
        {
            footstepTimer = 0f;
            return;
        }

        footstepTimer -= Time.deltaTime;
        if (footstepTimer <= 0f)
        {
            AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];
            audioSource.PlayOneShot(clip, footstepVolume);
            footstepTimer = footstepInterval;
        }
    }
}
