using UnityEngine;
using InteractionSystem.Core;

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
        [SerializeField] private ItemData itemData = default;
        [SerializeField, Min(1)] private int quantity = 1;

        void OnValidate ()
        {
            if (itemData == null)
                Debug.LogWarning($"[WorldInventoryItem] Asigna un ItemData en {gameObject.name}", this);

            if (GetComponent<Collider>() == null)
                Debug.LogWarning($"[WorldInventoryItem] {gameObject.name} necesita un Collider", this);
        }

        void OnDrawGizmos ()
        {
            if (itemData == null) return;

            Gizmos.color = itemData.IsLimb ? Color.red : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.3f);
        }

        public override void Interact ()
        {
            if (itemData == null)
            {
                Debug.LogWarning("[WorldInventoryItem] No hay ItemData asignado");
                return;
            }

            PlayerInventory playerInventory = FindPlayerInventory();
            if (playerInventory == null)
            {
                Debug.LogWarning("[WorldInventoryItem] No se encontró PlayerInventory en el Player");
                return;
            }

            bool success = playerInventory.AddItem(itemData, quantity);
            if (success)
                Destroy(gameObject);
        }

        public override bool IsInteractable ()
        {
            return itemData != null && quantity > 0;
        }

        private PlayerInventory FindPlayerInventory ()
        {
            try
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                PlayerInventory inventory = player.GetComponent<PlayerInventory>();
                if (inventory != null)
                    return inventory;
            }
            catch (UnityException)
            {
                // No hay objeto con tag Player
            }

            return UnityEngine.Object.FindFirstObjectByType<PlayerInventory>();
        }
    }
}

