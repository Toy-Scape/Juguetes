using InteractionSystem.Core;
using UnityEngine;
using InteractionSystem.Interfaces;

namespace Inventory
{
    /// <summary>
    /// Componente para objetos del mundo que pueden ser recogidos.
    /// Implementa IInteractable para integrarse con el sistema de interacción existente.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class WorldInventoryItem : InteractableBase
    {
        [Header("Item Configuration")]
        [SerializeField] private ItemData itemData;
        [SerializeField, Min(1)] private int quantity = 1;
        
        private void OnValidate()
        {
            if (itemData == null)
            {
                Debug.LogWarning($"[WorldInventoryItem] Asigna un ItemData en {gameObject.name}", this);
            }

            // Asegurar que tenga Collider
            if (GetComponent<Collider>() == null)
            {
                Debug.LogWarning($"[WorldInventoryItem] {gameObject.name} necesita un Collider", this);
            }
            
        }

        public override void Interact()
        {
            if (itemData == null)
            {
                Debug.LogWarning("[WorldInventoryItem] No hay ItemData asignado");
                return;
            }

            // Buscar el PlayerInventory en la escena
            PlayerInventory playerInventory = FindPlayerInventory();

            if (playerInventory == null)
            {
                Debug.LogWarning("[WorldInventoryItem] No se encontró PlayerInventory en el Player");
                return;
            }

            // Intentar añadir el item al inventario
            bool success = playerInventory.AddItem(itemData, quantity);

            if (success)
            {
                Destroy(gameObject);
            }
        }

        public override bool IsInteractable()
        {
            return itemData != null && quantity > 0;
        }

        private PlayerInventory FindPlayerInventory()
        {
            // Intentar encontrar por tag Player primero
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                PlayerInventory inventory = player.GetComponent<PlayerInventory>();
                if (inventory != null) return inventory;
            }

            // Fallback: buscar en toda la escena
            return FindFirstObjectByType<PlayerInventory>();
        }

        private void OnDrawGizmos()
        {
            if (itemData != null)
            {
                Gizmos.color = itemData.IsLimb ? Color.red : Color.yellow;
                Gizmos.DrawWireSphere(transform.position, 0.3f);
            }
        }
    }
}

