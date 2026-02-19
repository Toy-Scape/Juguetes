using UnityEngine;

public class PlayerMovementAudioSystem : MonoBehaviour
{
    public PlayerController player;

    [Header("Audio Sources")]
    public AudioSource footstepSource;
    public AudioSource foleySource;

    [Header("Volumes")]
    [Range(0,2)] public float jumpVolume = 1f;
    [Range(0,2)] public float landVolume = 1f;
    [Range(0,2)] public float climbGrabVolume = 1f;
    [Range(0,2)] public float climbLoopVolume = 0.6f;
    [Range(0,2)] public float grabObjectVolume = 1f;

    [Header("Jump")]
    public AudioClip[] jumpClips;

    [Header("Landing")]
    public AudioClip landingClip;
    public float minFallSpeed = 4f;
    public float maxFallSpeed = 20f;

    [Header("Climb")]
    public AudioClip climbGrabClip;
    public AudioClip climbLoopClip;

    [Header("Grabbing Objects")]
    public AudioClip grabObjectClip;

    float lastYVelocity;
    bool wasGrounded;
    bool wasClimbing;
    bool wasGrabbing;
    bool wasPicking;

    void Update()
    {
        HandleAirSounds();
        HandleClimbLoop();
        HandleGrabSounds();
    }

    void HandleAirSounds()
    {
        bool grounded = player.CharacterController.isGrounded;
        float yVel = player.CharacterController.velocity.y;

        // SALTO
        if (wasGrounded && !grounded && yVel > 0.1f)
            PlayJump();

        if (!grounded)
            lastYVelocity = yVel;

        // LANDING logic
        // Only play land sound if we were falling fast enough AND we weren't just climbing
        if (!wasGrounded && grounded && !wasClimbing)
        {
            // Use absolute value to ignore direction, though usually negative
            float speed = Mathf.Abs(lastYVelocity);
            if (speed > minFallSpeed)
            {
                PlayLanding(speed);
            }
        }

        wasGrounded = grounded;
    }

    void HandleClimbLoop()
    {
        bool isClimbingNow = player.Context.IsGrabbingLedge || player.Context.IsWallClimbing;

        if (!wasClimbing && isClimbingNow)
        {
            PlayClimbGrab();
        }

        if (isClimbingNow)
        {
            // Ensure loop is playing
            if (!footstepSource.isPlaying || footstepSource.clip != climbLoopClip)
            {
                if (footstepSource != null && climbLoopClip != null)
                {
                    footstepSource.clip = climbLoopClip;
                    footstepSource.loop = true;
                    footstepSource.volume = climbLoopVolume;
                    footstepSource.Play();
                }
            }
        }
        else
        {
            // Stop loop if we were climbing but now are not
            if (wasClimbing)
            {
                if (footstepSource != null && footstepSource.clip == climbLoopClip)
                {
                    footstepSource.Stop();
                    footstepSource.loop = false; // Reset for footsteps usage
                    footstepSource.clip = null;
                }
            }
        }

        wasClimbing = isClimbingNow;
    }

    public void PlayClimbMovement()
    {
        if (climbLoopClip == null || foleySource == null) return;

        // Play as one shot on foley source to not interrupt the loop on footstep source
        foleySource.PlayOneShot(climbLoopClip, climbLoopVolume);
    }




    void HandleGrabSounds()
    {
        if (!wasGrabbing && player.Context.IsGrabbing)
            foleySource.PlayOneShot(grabObjectClip, grabObjectVolume);

        if (!wasPicking && player.Context.IsPicking)
            foleySource.PlayOneShot(grabObjectClip, grabObjectVolume);

        wasGrabbing = player.Context.IsGrabbing;
        wasPicking = player.Context.IsPicking;
    }

    void PlayJump()
    {
        if (jumpClips == null || jumpClips.Length == 0) return;
        foleySource.pitch = Random.Range(0.95f, 1.05f);
        foleySource.PlayOneShot(jumpClips[Random.Range(0, jumpClips.Length)], jumpVolume);
    }

    void PlayLanding(float fallSpeed)
    {
        if (landingClip == null) return;
        float t = Mathf.InverseLerp(minFallSpeed, maxFallSpeed, fallSpeed);
        float dynamicVolume = landVolume * Mathf.Lerp(0.4f, 1f, t);

        foleySource.pitch = Random.Range(0.95f, 1.05f);
        foleySource.PlayOneShot(landingClip, dynamicVolume);
    }

    void PlayClimbGrab()
    {
        if (climbGrabClip != null)
            foleySource.PlayOneShot(climbGrabClip, climbGrabVolume);
    }
}
