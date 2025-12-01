using UnityEngine;

public class WalkableWallSurface : MonoBehaviour
{
    [Header("Wall Properties")]
    [Tooltip("Velocidad extra o penalización al andar en esta pared")]
    public float speedModifier = 1f;

    [Tooltip("¿Se puede escalar esta pared además de caminar?")]
    public bool climbable = false;

    [Tooltip("¿El jugador se pega automáticamente a la pared?")]
    public bool sticky = true;
}
