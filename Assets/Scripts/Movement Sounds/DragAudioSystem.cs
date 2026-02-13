using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Rigidbody))]
public class DragAudioSystem : MonoBehaviour
{
    public AudioClip[] dragClips;
    public float minVelocityToPlay = 0.1f;

    private AudioSource source;
    private Rigidbody rb;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();

        source.loop = true;
        source.spatialBlend = 1f; // 3D
        source.playOnAwake = false;
    }

    void Update()
    {
        float speed = rb.linearVelocity.magnitude;

        if (speed > minVelocityToPlay)
        {
            if (!source.isPlaying)
            {
                source.clip = dragClips[Random.Range(0, dragClips.Length)];
                source.Play();
            }

            source.volume = Mathf.Clamp01(speed / 2f);
            source.pitch = Mathf.Lerp(0.8f, 1.2f, speed / 3f);
        }
        else
        {
            if (source.isPlaying)
                source.Stop();
        }
    }
}
