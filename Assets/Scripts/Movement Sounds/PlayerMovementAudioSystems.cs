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

        // LANDING (ignorar si venimos de escalada)
        if (!wasGrounded && grounded && !wasClimbing)
            PlayLanding(Mathf.Abs(lastYVelocity));

        wasGrounded = grounded;
    }

    void HandleClimbLoop()
    {
        bool isClimbingNow = player.Context.IsGrabbingLedge || player.Context.IsWallClimbing;

        if (!wasClimbing && isClimbingNow)
        {
            PlayClimbGrab();
        }

        if (isClimbingNow && !footstepSource.isPlaying)
        {
            footstepSource.PlayOneShot(climbLoopClip, climbLoopVolume);
        }

        wasClimbing = isClimbingNow;
    }

    public void PlayClimbMovement()
    {
        if (climbLoopClip == null || footstepSource == null) return;

        Debug.Log("Playing on foley");

        foleySource.Stop(); // 🔥 importante
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
        foleySource.pitch = Random.Range(0.95f, 1.05f);
        foleySource.PlayOneShot(jumpClips[Random.Range(0, jumpClips.Length)], jumpVolume);
    }

    void PlayLanding(float fallSpeed)
    {
        float t = Mathf.InverseLerp(minFallSpeed, maxFallSpeed, fallSpeed);
        float dynamicVolume = landVolume * Mathf.Lerp(0.4f, 1f, t);

        foleySource.pitch = Random.Range(0.95f, 1.05f);
        foleySource.PlayOneShot(landingClip, dynamicVolume);
    }

    void PlayClimbGrab()
    {
        foleySource.PlayOneShot(climbGrabClip, climbGrabVolume);
    }
}
