using UnityEngine;
using InteractionSystem.Interfaces;
using Inventory;

namespace Interaction_System.Interactables
{
    /// <summary>
    /// Componente que convierte un GameObject en un pickup de ítem.
    /// Implementa IInteractable para integrarse con el sistema de interacción existente.
    /// Solo maneja la lógica de recogida, sin dependencias fuertes con Player.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class ItemPickup : MonoBehaviour, IInteractable
    {
        [SerializeField] private ItemData item;
        [SerializeField, Min(1)] private int quantity = 1;
        [SerializeField] private bool destroyOnPickup = true;

        public void Interact()
        {
            var inventoryComponent = FindInventoryInScene();
            if (inventoryComponent == null)
            {
                Debug.LogWarning("No InventoryComponent encontrado en la escena. Añadir InventoryComponent al jugador para permitir recoger ítems.");
                return;
            }

            bool added = inventoryComponent.AddItem(item, quantity);
            if (added)
            {
                if (destroyOnPickup)
                    Destroy(gameObject);
            }
            else
            {
                Debug.Log("No se pudo añadir el ítem al inventario (posible capacidad llena)");
            }
        }

        public bool IsInteractable()
        {
            return item != null && quantity > 0;
        }

        private InventoryComponent FindInventoryInScene()
        {
            // Preferir objeto con tag "Player" si existe
            const string playerTag = "Player";
            var player = GameObject.FindWithTag(playerTag);
            if (player != null)
            {
                var ic = player.GetComponent<InventoryComponent>();
                if (ic != null) return ic;
            }

            // Usar FindAnyObjectByType si está disponible (Unity 2023+), si no, fallback a FindObjectOfType.
#if UNITY_2023_1_OR_NEWER
            return GameObject.FindAnyObjectByType<InventoryComponent>();
#else
            return FindObjectOfType<InventoryComponent>();
#endif
        }
    }
}
