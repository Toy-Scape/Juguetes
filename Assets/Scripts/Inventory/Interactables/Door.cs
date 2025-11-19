using InteractionSystem.Interfaces;
using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    public bool isOpen = false;


    public void Interact()
    {
        // Lógica simple de abrir/cerrar puerta
        isOpen = !isOpen;
        // Aquí podrías animar la puerta, etc.
        Debug.Log(isOpen ? "Puerta abierta" : "Puerta cerrada");    }

    public bool IsInteractable()
    {
        return true;
    }
}
