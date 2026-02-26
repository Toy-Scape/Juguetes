using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(Collider))]
public class CinemachineTrigger : MonoBehaviour
{
    [Header("Configuración de la Cámara")]
    [Tooltip("Arrastra aquí la cámara de Cinemachine a la que quieres cambiar.")]
    public CinemachineCamera targetCamera;

    [Header("Configuración del Trigger")]
    [Tooltip("La capa (Layer) del objeto que activará la cámara. Asegúrate de que el Player esté en esta capa.")]
    public LayerMask triggerLayer;

    [Tooltip("La prioridad que tendrá la cámara al entrar en el trigger.")]
    public int activePriority = 20;

    private int originalPriority;
    private static System.Collections.Generic.List<CinemachineTrigger> allTriggers = new System.Collections.Generic.List<CinemachineTrigger>();

    private void Start()
    {
        // Guardamos la prioridad original para restaurarla al salir del trigger
        if (targetCamera != null)
        {
            originalPriority = targetCamera.Priority;
        }

        if (!allTriggers.Contains(this))
        {
            allTriggers.Add(this);
        }

        // Asegurarse de que el collider funciona como un trigger
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Comprobamos si el objeto pertenece a la capa (LayerMask) configurada
        if (IsHitInLayer(other.gameObject, triggerLayer) && targetCamera != null)
        {
            targetCamera.Priority = activePriority;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsHitInLayer(other.gameObject, triggerLayer) && targetCamera != null)
        {
            targetCamera.Priority = originalPriority;
        }
    }

    // Helper para comprobar la máscara de capa de forma limpia
    private bool IsHitInLayer(GameObject obj, LayerMask layerMask)
    {
        return (layerMask.value & (1 << obj.layer)) > 0;
    }

    private void OnDestroy()
    {
        if (allTriggers.Contains(this))
        {
            allTriggers.Remove(this);
        }
    }

    public static void ResetAllTriggers()
    {
        foreach (var trigger in allTriggers)
        {
            if (trigger != null && trigger.targetCamera != null)
            {
                trigger.targetCamera.Priority = trigger.originalPriority;
            }
        }
    }
}
