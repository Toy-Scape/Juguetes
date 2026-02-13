using UnityEngine;

public class PlayerFootstepSystem : MonoBehaviour
{
    public PlayerController player;
    public AudioSource footstepSource;

    public float rayDistance = 2f;
    public LayerMask groundLayer;

    public float walkStepInterval = 0.5f;
    public float runStepInterval = 0.3f;

    public AudioClip[] asphaltWalk;
    public AudioClip[] woodWalk;
    public AudioClip[] cardboardWalk;

    public AudioClip[] asphaltRun;
    public AudioClip[] woodRun;
    public AudioClip[] cardboardRun;


    private float stepTimer;
    private SurfaceType currentSurface;
    private bool wasMovingLastFrame;

    [Range(0f,2f)] public float asphaltVolume = 1f;
    [Range(0f,2f)] public float woodVolume = 0.3f;
    [Range(0f,2f)] public float cardboardVolume = 0.3f;


    void Update()
    {
        if (!player.CharacterController.isGrounded)
        {
            wasMovingLastFrame = false;
            StopFootstepSound();
            return;
        }

        DetectSurface();
        HandleFootsteps();
    }

    void DetectSurface()
    {
        RaycastHit hit;
        currentSurface = SurfaceType.Asphalt; 

        if (Physics.Raycast(transform.position + Vector3.up * 0.2f, Vector3.down, out hit, rayDistance))
        {
            var surface = hit.collider.GetComponent<SurfaceIdentifier>();

            if (surface != null && Vector3.Angle(hit.normal, Vector3.up) < 45f)
                currentSurface = surface.surfaceType;
        }
    }


    void HandleFootsteps()
{
    Vector3 vel = player.CharacterController.velocity;
    Vector3 horizontalVel = new Vector3(vel.x, 0, vel.z);
    float speed = horizontalVel.magnitude;

    bool isMoving = speed > 0.2f;

    if (!isMoving)
    {
        StopFootstepSound();
        return;
    }

    HandleLoopingFootsteps(speed);
}


    void HandleLoopingFootsteps(float speed)
    {
        AudioClip targetClip = GetClip();
        float targetVolume = GetSurfaceVolume();

        // Si no está sonando → iniciar
        if (!footstepSource.isPlaying)
        {
            footstepSource.clip = targetClip;
            footstepSource.volume = targetVolume;
            footstepSource.pitch = player.Context.IsSprinting ? 1.2f : 1f;
            footstepSource.Play();
            return;
        }

        // Si cambió superficie o tipo de movimiento → cambiar clip
        if (footstepSource.clip != targetClip)
        {
            footstepSource.clip = targetClip;
            footstepSource.volume = targetVolume;
            footstepSource.pitch = player.Context.IsSprinting ? 1.2f : 1f;
            footstepSource.Play();
        }
    }




    void StopFootstepSound()
    {
        if (footstepSource.isPlaying)
            footstepSource.Stop();
    }

    AudioClip GetClip()
{
    AudioClip clip = null;
    bool isRunning = player.Context.IsSprinting;


    switch (currentSurface)
    {
        case SurfaceType.Wood:
            clip = isRunning ? woodRun[Random.Range(0, woodRun.Length)]
                             : woodWalk[Random.Range(0, woodWalk.Length)];
            break;
        case SurfaceType.Cardboard:
            clip = isRunning ? cardboardRun[Random.Range(0, cardboardRun.Length)]
                             : cardboardWalk[Random.Range(0, cardboardWalk.Length)];
            break;
        default:
            clip = isRunning ? asphaltRun[Random.Range(0, asphaltRun.Length)]
                             : asphaltWalk[Random.Range(0, asphaltWalk.Length)];
            break;
    }

    Debug.Log($"Surface: {currentSurface}, Running: {isRunning}, Clip: {clip?.name}");
    return clip;
}



    float GetSurfaceVolume()
    {
        bool isRunning = player.Context.IsSprinting;
        float baseVolume;

        switch (currentSurface)
        {
            case SurfaceType.Wood: baseVolume = woodVolume; break;
            case SurfaceType.Cardboard: baseVolume = cardboardVolume; break;
            default: baseVolume = asphaltVolume; break;
        }

        if (isRunning) baseVolume *= 1.3f; 
        return baseVolume;
    }
}
