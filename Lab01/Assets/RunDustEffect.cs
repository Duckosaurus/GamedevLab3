using UnityEngine;

public class RunDustEffect : MonoBehaviour
{
    [SerializeField] private ParticleSystem dustParticles;

    private PlayerMovement playerMovement;

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if (dustParticles == null) return;

        bool shouldEmit = playerMovement.IsGrounded && playerMovement.IsMoving;

        if (shouldEmit && !dustParticles.isEmitting)
            dustParticles.Play();
        else if (!shouldEmit && dustParticles.isEmitting)
            dustParticles.Stop();
    }
}
