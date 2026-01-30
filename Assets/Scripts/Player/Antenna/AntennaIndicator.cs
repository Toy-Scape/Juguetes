using UnityEngine;

public class AntennaIndicator : MonoBehaviour
{
    private Material ballMat;

    public enum AlertState { Safe, Warning, Detected }
    public AlertState currentState;

    private float blinkSpeed;
    private Color emissionColor;

    public Domain.NpcBrain npcBrain;
    public float warningDistance = 15f;

    void Start()
    {
        // Cogemos el material SOLO de la bola
        ballMat = GetComponent<Renderer>().material;

        if (npcBrain == null)
        {
             npcBrain = FindFirstObjectByType<Domain.NpcBrain>();
        }
    }

    void Update()
    {
        CheckNpcStatus();
        UpdateStateValues();
        Blink();
    }

    void CheckNpcStatus()
    {
        if (npcBrain == null) return;

        // 1. Priority: Detected
        // If the NPC is looking at THAT player object
        if (npcBrain.CurrentTarget == this.transform || npcBrain.CurrentTarget == this.transform.parent || (npcBrain.CurrentTarget != null && npcBrain.CurrentTarget.root == this.transform.root))
        {
            SetState(AlertState.Detected);
            return;
        }

        // 2. Distance Check
        float distance = Vector3.Distance(transform.position, npcBrain.transform.position);
        if (distance <= warningDistance)
        {
             SetState(AlertState.Warning);
        }
        else
        {
             SetState(AlertState.Safe);
        }
    }

    void UpdateStateValues()
    {
        switch (currentState)
        {
            case AlertState.Safe:
                emissionColor = Color.white;
                blinkSpeed = 1f;   // Lento
                break;

            case AlertState.Warning:
                emissionColor = Color.yellow;
                blinkSpeed = 3f;   // Medio
                break;

            case AlertState.Detected:
                emissionColor = Color.red;
                blinkSpeed = 6f;   // Rápido
                break;
        }
    }

    void Blink()
    {
        float intensity = Mathf.PingPong(Time.time * blinkSpeed, 1f);

        // Multiplicador alto para que active el Bloom
        Color finalColor = emissionColor * Mathf.LinearToGammaSpace(intensity * 30f);

        ballMat.SetColor("_EmissionColor", finalColor);
    }

    public void SetState(AlertState newState)
    {
        currentState = newState;
    }
}
