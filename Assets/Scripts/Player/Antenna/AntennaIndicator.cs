using UnityEngine;
using Domain.StaticNpc;

public class AntennaIndicator : MonoBehaviour
{
    private Material ballMat;

    public enum AlertState { Safe, Warning, Detected }
    public AlertState currentState;

    private float blinkSpeed;
    private Color emissionColor;

    public StaticNpcBrain[] npcBrains;
    public float warningDistance = 15f;

    private Transform playerRoot;

    void Start()
    {
        ballMat = GetComponent<Renderer>().material;

        npcBrains = FindObjectsByType<StaticNpcBrain>(FindObjectsSortMode.None);

        playerRoot = transform.root;
    }

    void Update()
    {
        CheckNpcStatus();
        UpdateStateValues();
        Blink();
    }

    void CheckNpcStatus()
    {
        if (npcBrains == null || npcBrains.Length == 0) return;

        bool warning = false;

        foreach (var brain in npcBrains)
        {
            if (brain == null) continue;

            Transform target = brain.CurrentTarget;

            // 🔴 DETECTED
            if (brain.IsFullyDetected && target != null && target.root == playerRoot)
            {
                SetState(AlertState.Detected);
                return;
            }

            // 🟡 DETECTING
            if (brain.IsDetecting && target != null && target.root == playerRoot)
            {
                warning = true;
            }

            // 🟡 DISTANCE
            float distance = Vector3.Distance(transform.position, brain.transform.position);

            if (distance <= warningDistance)
            {
                warning = true;
            }
        }

        if (warning)
            SetState(AlertState.Warning);
        else
            SetState(AlertState.Safe);
    }

    void UpdateStateValues()
    {
        switch (currentState)
        {
            case AlertState.Safe:
                emissionColor = Color.white;
                blinkSpeed = 1f;
                break;

            case AlertState.Warning:
                emissionColor = Color.yellow;
                blinkSpeed = 3f;
                break;

            case AlertState.Detected:
                emissionColor = Color.red;
                blinkSpeed = 6f;
                break;
        }
    }

    void Blink()
    {
        float pulse = (Mathf.Sin(Time.time * blinkSpeed * Mathf.PI * 2f) + 1f) * 0.5f;

        float minIntensity = 2f;
        float maxIntensity = 25f;

        if (currentState == AlertState.Warning)
        {
            minIntensity = 1f;
            maxIntensity = 35f;
        }
        else if (currentState == AlertState.Detected)
        {
            minIntensity = 0.5f;
            maxIntensity = 50f;
        }

        float intensity = Mathf.Lerp(minIntensity, maxIntensity, pulse);

        Color finalColor = emissionColor * intensity;

        ballMat.SetColor("_EmissionColor", finalColor);
    }

    public void SetState(AlertState newState)
    {
        currentState = newState;
    }
}