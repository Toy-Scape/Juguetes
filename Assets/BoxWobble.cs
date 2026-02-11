using UnityEngine;

public class BoxWobbleAndFall : MonoBehaviour
{
    public Transform pivot;
    public float wobbleAngle = 10f;
    public float wobbleSpeed = 2f;

    public float fallRotateSpeed = 180f;
    public float fallMoveSpeed = 2f;

    private bool falling = false;
    private float rotated = 0f;
    private Quaternion initialPivotRotation;

    void Start()
    {
        if (pivot == null)
            pivot = transform;

        initialPivotRotation = pivot.localRotation;

    }

    void Update()
    {
        if (!falling)
        {
            float angle = Mathf.Sin(Time.time * wobbleSpeed) * wobbleAngle;
            pivot.localRotation = initialPivotRotation * Quaternion.Euler(0f, 0f, angle);
        }
        else
        {
            float step = fallRotateSpeed * Time.deltaTime;
            float remaining = 90f - rotated;
            float actualStep = Mathf.Min(step, remaining);

            transform.Rotate(Vector3.right, actualStep, Space.World);
            transform.position += Vector3.down * fallMoveSpeed * Time.deltaTime;

            rotated += actualStep;

            if (rotated >= 90f)
            {
                falling = false;
            }
        }
    }

    public void TriggerFall()
    {
        if (falling) return;

        pivot.localRotation = initialPivotRotation;
        falling = true;
        rotated = 0f;
    }
}
