﻿# Sistema de Inventario - Unity

Sistema de inventario desacoplado siguiendo principios de Clean Code y arquitectura limpia.

**🎮 Usa el nuevo Input System con Unity Messages** - Ver [UNITY_MESSAGES_INPUT.md](UNITY_MESSAGES_INPUT.md) para detalles.
- Sin serialización de InputActionAsset
- Patrón Unity Messages (IPlayerActions)
- Sin configuración en el Inspector

## 📦 Componentes

### 1. **ItemData** (ScriptableObject)
Define las propiedades de cualquier ítem del juego.

**Propiedades:**
- `ItemName`: Nombre del ítem
- `Description`: Descripción del ítem
- `Icon`: Sprite del ítem
- `MaxStackSize`: Cantidad máxima que se puede apilar
- `IsLimb`: Indica si el ítem es una extremidad (usa lista separada)

**Crear nuevo ítem:**
1. Click derecho en Project → Create → Inventory → Item Data
2. Configurar propiedades en el Inspector

### 2. **InventoryItem** (Clase)
Representa una instancia de un ítem dentro del inventario.
- Contiene la referencia al `ItemData`
- Gestiona la cantidad actual
- Maneja lógica de apilamiento

### 3. **Inventory** (Clase pura - NO MonoBehaviour)
Lógica pura del inventario sin dependencias de Unity.

**Características:**
- Dos listas internas: items normales y extremidades (limbs)
- Capacidad configurable para cada lista
- Sistema de apilamiento automático
- Fácil de testear

**Métodos principales:**
```csharp
bool AddItem(ItemData itemData, int quantity)
bool Contains(ItemData itemData)
InventoryItem GetItem(ItemData itemData)
bool RemoveItem(ItemData itemData, int quantity)
int GetItemCount(ItemData itemData)
void Clear()
```

### 4. **InventoryComponent** (MonoBehaviour)
Único componente MonoBehaviour del sistema. Se añade al Player.

**Métodos públicos:**
```csharp
bool AddItem(ItemData itemData, int quantity = 1)
bool Contains(ItemData itemData)
InventoryItem GetItem(ItemData itemData)
int GetItemCount(ItemData itemData)
bool RemoveItem(ItemData itemData, int quantity = 1)
bool DropItem(ItemData itemData, int quantity = 1)
void ClearInventory()
```

**Eventos:**
- `onItemAdded`: Se dispara cuando se añade un ítem
- `onItemRemoved`: Se dispara cuando se elimina un ítem

### 5. **ItemPickup** (MonoBehaviour)
Implementa `IInteractable` para recoger objetos usando el sistema de interacción existente.

**Configuración:**
- `item`: Referencia al ItemData
- `quantity`: Cantidad a recoger
- `destroyOnPickup`: Si se destruye el objeto al recogerlo

## 🚀 Uso Rápido

### Configurar el Player
1. Añadir el componente `InventoryComponent` al GameObject del Player
2. Listo - el sistema está configurado

### Crear un ítem recogible en el mundo
1. Crear un GameObject (ej: un cubo)
2. Añadir componente `ItemPickup`
3. Añadir componente `Outline` (requerido por el sistema de interacción)
4. Asignar un `ItemData` en el Inspector
5. Configurar cantidad y opciones

### Usar desde código

```csharp
using Inventory;

public class PlayerController : MonoBehaviour
{
    private InventoryComponent inventory;

    void Start()
    {
        inventory = GetComponent<InventoryComponent>();
        
        // Suscribirse a eventos
        inventory.onItemAdded.AddListener(OnItemAdded);
    }

    void Update()
    {
        // Verificar si tiene un ítem específico
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (inventory.Contains(someItemData))
            {
                Debug.Log("Tienes el ítem!");
            }
        }
    }

    void OnItemAdded(ItemData item, int quantity)
    {
        Debug.Log($"Añadido: {item.name} x{quantity}");
    }
}
```

### Añadir ítems programáticamente

```csharp
// Añadir ítem
bool success = inventory.AddItem(itemData, 5);

// Obtener cantidad
int count = inventory.GetItemCount(itemData);

// Eliminar ítem
inventory.RemoveItem(itemData, 2);

// Dropear ítem (elimina del inventario)
inventory.DropItem(itemData, 1);
```

## 🏗️ Arquitectura

```
┌─────────────────────────────────────────┐
│         PlayerInteractor                │
│    (Sistema de Interacción)             │
└─────────────────┬───────────────────────┘
                  │ Interact()
                  ▼
┌─────────────────────────────────────────┐
│          ItemPickup                     │
│       (IInteractable)                   │
│   - Detecta InventoryComponent          │
│   - Llama AddItem()                     │
└─────────────────┬───────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────┐
│      InventoryComponent                 │
│       (MonoBehaviour)                   │
│   - Puente con Unity                    │
│   - Eventos                             │
│   - Validaciones                        │
└─────────────────┬───────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────┐
│          Inventory                      │
│       (Lógica Pura)                     │
│   - Lista de items normales             │
│   - Lista de extremidades               │
│   - Sin dependencias de Unity           │
└─────────────────┬───────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────┐
│       InventoryItem                     │
│   - ItemData + Quantity                 │
│   - Lógica de apilamiento               │
└─────────────────────────────────────────┘
```

## ✨ Características

- **Clean Code**: Nombres claros, responsabilidades separadas
- **Desacoplado**: Lógica separada de MonoBehaviour
- **Extensible**: Fácil añadir nuevas funcionalidades
- **Testeable**: Lógica pura sin dependencias de Unity
- **Integrado**: Funciona con el sistema de interacción existente
- **Dos inventarios**: Separación automática entre items normales y extremidades
- **Sistema de apilamiento**: Gestión automática de stacks

## 🔧 Personalización

### Cambiar capacidad del inventario
Editar valores en el componente `InventoryComponent` en el Inspector:
- `Max Capacity`: Capacidad para items normales
- `Max Limb Capacity`: Capacidad para extremidades

### Extender ItemData
```csharp
[CreateAssetMenu(fileName = "New Weapon", menuName = "Inventory/Weapon")]
public class WeaponData : ItemData
{
    [SerializeField] private int damage;
    [SerializeField] private float attackSpeed;
    
    public int Damage => damage;
    public float AttackSpeed => attackSpeed;
}
```

### Debug Mode
Activar el checkbox `Debug Mode` en `InventoryComponent` para ver logs detallados.

## 📝 Notas

- El sistema busca automáticamente el `InventoryComponent` en el GameObject con tag "Player"
- Los items con `IsLimb = true` se guardan en una lista separada
- El sistema soporta apilamiento automático según `MaxStackSize`
- Compatible con Unity 2021.3+


