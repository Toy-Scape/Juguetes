//Vector3 finalPos = new Vector3(
//-17.2559967f,
//0.03654289f,
//-11.9039993f
//);

//Quaternion finalRot = new Quaternion(
//    -7.1477899e-8f,
//     0.26913732f,
//    -5.4033712e-8f,
//     0.96310186f
//);
//        //TriggerFall(finalPos, finalRot);

using UnityEngine;

public class BoxWobbleAndFall : MonoBehaviour
{
    public Transform pivot;
    public float wobbleAngle = 10f;
    public float wobbleSpeed = 2f;
    public Vector3 wobbleAxisLocal = Vector3.forward;
    public float fallDuration = 0.6f;
    public float settleDuration = 0.25f;
    public Vector3 fallAxisLocal = Vector3.right;
    public float fallAngle = 75f;
    public AudioClip fallSound;

    Vector3 finalWorldPosition;
    Quaternion finalWorldRotation;
    float rotated;
    float fallTime;
    float settleTime;
    Vector3 settleStartPos;
    Quaternion settleStartRot;
    Quaternion initialPivotRotation;
    AudioSource audioSource;

    enum State { Wobbling, Falling, Settling, Fallen }
    State currentState = State.Wobbling;

    void Start()
    {
        if (pivot == null) pivot = transform;
        initialPivotRotation = pivot.localRotation;

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.clip = fallSound;
    }

    void Update()
    {
        switch (currentState)
        {
            case State.Wobbling:
                float angle = Mathf.Sin(Time.time * wobbleSpeed) * wobbleAngle;
                pivot.localRotation = initialPivotRotation * Quaternion.AngleAxis(angle, wobbleAxisLocal);
                break;
            case State.Falling:
                UpdateFall();
                break;
            case State.Settling:
                UpdateSettle();
                break;
            case State.Fallen:
                break;
        }
    }

    void UpdateFall()
    {
        fallTime += Time.deltaTime;
        float t = Mathf.Clamp01(fallTime / fallDuration);
        float eased = Mathf.SmoothStep(0f, 1f, t);
        float targetAngle = fallAngle * eased;
        float delta = targetAngle - rotated;
        Vector3 axis = transform.TransformDirection(fallAxisLocal);
        transform.RotateAround(pivot.position, axis, delta);
        rotated = targetAngle;

        if (t >= 1f)
        {
            if (fallSound != null) audioSource.Play();
            settleStartPos = transform.position;
            settleStartRot = transform.rotation;
            settleTime = 0f;
            currentState = State.Settling;
        }
    }

    void UpdateSettle()
    {
        settleTime += Time.deltaTime;
        float t = Mathf.Clamp01(settleTime / settleDuration);
        float eased = Mathf.SmoothStep(0f, 1f, t);
        transform.position = Vector3.Lerp(settleStartPos, finalWorldPosition, eased);
        transform.rotation = Quaternion.Slerp(settleStartRot, finalWorldRotation, eased);
        if (t >= 1f)
        {
            currentState = State.Fallen;
            enabled = false;
        }
    }

    public void TriggerFall(Vector3 finalPos, Quaternion finalRot)
    {
        if (currentState != State.Wobbling) return;
        pivot.localRotation = initialPivotRotation;
        finalWorldPosition = finalPos;
        finalWorldRotation = finalRot;
        rotated = 0f;
        fallTime = 0f;
        currentState = State.Falling;
    }
}
