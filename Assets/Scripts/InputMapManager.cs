using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Pequeño servicio para cambiar de action map con un pequeño retardo (evita asserts del InputSystem)
/// Centraliza cambios de action map para que otros scripts no lo hagan directamente.
/// </summary>
[DefaultExecutionOrder(-100)]
public class InputMapManager : MonoBehaviour
{
    public static InputMapManager Instance { get; private set; }

    [SerializeField] private PlayerInput playerInput;

    private void Awake ()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (playerInput == null)
        {
            playerInput = GetComponent<PlayerInput>();
        }
    }

    /// <summary>
    /// Cambia el action map, pero espera al menos un frame para evitar problemas de timing
    /// relacionados con el Input System (assertions al activar/desactivar acciones inmediatamente).
    /// </summary>
    public void SwitchToActionMap(string mapName)
    {
        if (playerInput == null)
        {
            Debug.LogWarning("InputMapManager: playerInput no asignado.");
            return;
        }

        // Cambio inmediato (para entrar en diálogos)
        playerInput.SwitchCurrentActionMap(mapName);
    }

    public void SwitchToActionMapSafe(string mapName)
    {
        if (playerInput == null)
        {
            Debug.LogWarning("InputMapManager: playerInput no asignado.");
            return;
        }

        StartCoroutine(SwitchCoroutine(mapName));
    }

    private IEnumerator SwitchCoroutine(string mapName)
    {
        yield return new WaitForEndOfFrame();
        playerInput.SwitchCurrentActionMap(mapName);
    }
}