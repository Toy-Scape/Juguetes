# Guía Rápida de Uso - Sistema de Inventario

## 🎯 Configuración Inicial (3 pasos)

### 1️⃣ Añadir Inventario al Player
```
GameObject Player → Add Component → InventoryComponent
```
¡Listo! El inventario ya está funcional.

### 2️⃣ Crear un Item Data
```
Project → Click derecho → Create → Inventory → Item Data
```
Configurar:
- Item Name: "Poción de Vida"
- Max Stack Size: 10
- Is Limb: ☐ (dejar sin marcar)

### 3️⃣ Crear un Item Pickup en el Mundo
```
GameObject → 3D Object → Cube (o cualquier objeto)
Add Component → ItemPickup
Add Component → Outline (requerido por sistema de interacción)
```
En el Inspector del ItemPickup:
- Item: Arrastrar el ItemData creado
- Quantity: 1
- Destroy On Pickup: ✓

## 📝 Ejemplos de Código

### Desde el PlayerController

```csharp
using UnityEngine;
using Inventory;

public class PlayerController : MonoBehaviour
{
    private InventoryComponent inventory;

    void Start()
    {
        // Obtener referencia al inventario
        inventory = GetComponent<InventoryComponent>();
    }

    void Update()
    {
        // Ejemplo: Usar una poción con tecla H
        if (Input.GetKeyDown(KeyCode.H))
        {
            UseHealthPotion();
        }
    }

    void UseHealthPotion()
    {
        // Verificar si tiene el item
        if (inventory.Contains(healthPotionData))
        {
            // Usar la poción (tu lógica de curación aquí)
            // ...
            
            // Eliminar del inventario
            inventory.RemoveItem(healthPotionData, 1);
            Debug.Log("Poción usada!");
        }
        else
        {
            Debug.Log("No tienes pociones");
        }
    }
}
```

### Eventos

```csharp
void Start()
{
    inventory = GetComponent<InventoryComponent>();
    
    // Suscribirse a eventos
    inventory.onItemAdded.AddListener(OnItemCollected);
    inventory.onItemRemoved.AddListener(OnItemUsed);
}

void OnItemCollected(ItemData item, int quantity)
{
    // Mostrar notificación en UI
    ShowNotification($"Recogido: {item.name} x{quantity}");
}

void OnItemUsed(ItemData item, int quantity)
{
    // Actualizar UI
    UpdateInventoryUI();
}
```

## 🎮 Sistema de Interacción

El sistema ya está integrado con tu PlayerInteractor existente:

1. **El jugador se acerca a un item** → Se muestra outline amarillo
2. **Presiona E** → PlayerInteractor llama a ItemPickup.Interact()
3. **ItemPickup** → Busca el InventoryComponent y llama AddItem()
4. **InventoryComponent** → Añade el item al inventario y dispara eventos
5. **Item desaparece** (si destroyOnPickup está activo)

## 📦 Dos Tipos de Inventario

### Items Normales
```csharp
ItemData item;
item.IsLimb = false;  // Se guarda en lista principal
```

### Extremidades (Limbs)
```csharp
ItemData limb;
limb.IsLimb = true;  // Se guarda en lista separada
```

Ambos tipos se manejan automáticamente. Solo marca el checkbox "Is Limb" en el ItemData.

## 🔍 Métodos Disponibles

```csharp
// Añadir item
bool success = inventory.AddItem(itemData, quantity);

// Verificar si tiene item
bool hasItem = inventory.Contains(itemData);

// Obtener cantidad total
int count = inventory.GetItemCount(itemData);

// Obtener item completo (con datos de cantidad)
InventoryItem item = inventory.GetItem(itemData);
if (item != null)
{
    Debug.Log($"Tienes {item.Quantity} de {item.Data.name}");
}

// Eliminar item
inventory.RemoveItem(itemData, quantity);

// Dropear item (lo elimina del inventario)
inventory.DropItem(itemData, quantity);

// Limpiar todo el inventario
inventory.ClearInventory();
```

## 🐛 Debug Mode

Activa el checkbox "Debug Mode" en el InventoryComponent para ver logs detallados:
- Cuando se añaden items
- Cuando se eliminan items
- Cuando el inventario está lleno

## ✨ Características Automáticas

✅ **Apilamiento inteligente**: Los items se apilan automáticamente hasta MaxStackSize
✅ **Separación automática**: Items normales y extremidades en listas separadas
✅ **Búsqueda automática**: ItemPickup encuentra el inventario del jugador automáticamente
✅ **Sistema de eventos**: Notificaciones cuando cambia el inventario
✅ **Validaciones**: Verifica nulls, cantidades negativas, etc.

## 🚀 Extender el Sistema

### Crear un tipo de item personalizado

```csharp
using UnityEngine;

namespace Inventory
{
    [CreateAssetMenu(fileName = "New Weapon", menuName = "Inventory/Weapon")]
    public class WeaponData : ItemData
    {
        [SerializeField] private int damage;
        [SerializeField] private float attackSpeed;
        
        public int Damage => damage;
        public float AttackSpeed => attackSpeed;
    }
}
```

Ahora puedes crear "Weapon Data" desde el menú Create y tendrá todas las propiedades base más las nuevas.

## 📊 Acceder a Listas Completas

```csharp
// Obtener referencia al inventario interno
Inventory inv = inventoryComponent.Inventory;

// Recorrer todos los items normales
foreach (var item in inv.Items)
{
    Debug.Log($"{item.Data.name}: {item.Quantity}");
}

// Recorrer todas las extremidades
foreach (var limb in inv.Limbs)
{
    Debug.Log($"{limb.Data.name}: {limb.Quantity}");
}

// Ver capacidades
Debug.Log($"Capacidad items: {inv.Items.Count}/{inv.MaxCapacity}");
Debug.Log($"Capacidad limbs: {inv.Limbs.Count}/{inv.MaxLimbCapacity}");
```

## 🎨 Crear UI de Inventario

```csharp
public class InventoryUI : MonoBehaviour
{
    [SerializeField] private InventoryComponent inventory;
    [SerializeField] private GameObject itemSlotPrefab;
    [SerializeField] private Transform itemsContainer;

    void Start()
    {
        inventory.onItemAdded.AddListener((item, qty) => RefreshUI());
        inventory.onItemRemoved.AddListener((item, qty) => RefreshUI());
        RefreshUI();
    }

    void RefreshUI()
    {
        // Limpiar UI existente
        foreach (Transform child in itemsContainer)
            Destroy(child.gameObject);

        // Crear slots para cada item
        foreach (var item in inventory.Inventory.Items)
        {
            GameObject slot = Instantiate(itemSlotPrefab, itemsContainer);
            // Configurar el slot con item.Data.Icon, item.Quantity, etc.
        }
    }
}
```

