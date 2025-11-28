# 📦 API de Sistema de Inventario - Documentación Completa

## Índice
1. [Introducción](#introducción)
2. [Clases Principales](#clases-principales)
3. [PlayerInventory](#playerinventory)
4. [Inventory](#inventory)
5. [InventoryUI](#inventoryui)
6. [InventoryUIRegistry](#inventoryuiregistry)
7. [ItemData](#itemdata)
8. [InventoryItem](#inventoryitem)
9. [Ejemplos de Uso](#ejemplos-de-uso)

---

## Introducción

El sistema de inventario está diseñado con una arquitectura modular que separa la lógica del modelo de datos de la presentación visual. Esto permite una fácil extensión y testeo.

### Arquitectura del Sistema

```
ItemData (ScriptableObject)
    ↓
InventoryItem (Instancia de item con cantidad)
    ↓
Inventory (Lógica pura, sin MonoBehaviour)
    ↓
PlayerInventory (MonoBehaviour, controlador)
    ↓
InventoryUI (Presentación visual)
```

---

## Clases Principales

### PlayerInventory

**Namespace:** `Inventory`  
**Hereda de:** `MonoBehaviour`

#### Descripción
Componente principal del inventario del jugador. Actúa como controlador entre la lógica del inventario y la capa de presentación.

#### Propiedades Públicas

| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| `Inventory` | `Inventory` | Acceso de solo lectura al modelo de inventario |

#### Eventos

| Evento | Tipo | Descripción |
|--------|------|-------------|
| `onItemAdded` | `UnityEvent<ItemData, int>` | Se dispara cuando se añade un item. Parámetros: ItemData, cantidad |
| `onItemRemoved` | `UnityEvent<ItemData, int>` | Se dispara cuando se elimina un item. Parámetros: ItemData, cantidad |

#### Métodos Públicos

##### `AddItem(ItemData itemData, int quantity = 1)`
```csharp
public bool AddItem(ItemData itemData, int quantity = 1)
```
**Descripción:** Añade un ítem al inventario.

**Parámetros:**
- `itemData` (ItemData): Los datos del ítem a añadir
- `quantity` (int, opcional): Cantidad a añadir. Por defecto: 1

**Retorna:** `bool` - `true` si se añadió correctamente, `false` si el inventario está lleno

**Ejemplo:**
```csharp
PlayerInventory playerInv = GetComponent<PlayerInventory>();
ItemData potion = // ... referencia a tu ScriptableObject
bool success = playerInv.AddItem(potion, 3);

if (success)
{
    Debug.Log("Se añadieron 3 pociones al inventario");
}
else
{
    Debug.Log("Inventario lleno");
}
```

---

##### `RemoveItem(ItemData itemData, int quantity = 1)`
```csharp
public bool RemoveItem(ItemData itemData, int quantity = 1)
```
**Descripción:** Elimina un ítem del inventario.

**Parámetros:**
- `itemData` (ItemData): Los datos del ítem a eliminar
- `quantity` (int, opcional): Cantidad a eliminar. Por defecto: 1

**Retorna:** `bool` - `true` si se eliminó correctamente, `false` si no hay suficientes ítems

**Ejemplo:**
```csharp
PlayerInventory playerInv = GetComponent<PlayerInventory>();
ItemData potion = // ... referencia a tu ScriptableObject
bool consumed = playerInv.RemoveItem(potion, 1);

if (consumed)
{
    // Aplicar efecto de la poción
    player.health += 50;
}
```

---

##### `Contains(ItemData itemData)`
```csharp
public bool Contains(ItemData itemData)
```
**Descripción:** Verifica si el inventario contiene un ítem específico.

**Parámetros:**
- `itemData` (ItemData): El ítem a buscar

**Retorna:** `bool` - `true` si el inventario contiene al menos 1 unidad del ítem

**Ejemplo:**
```csharp
PlayerInventory playerInv = GetComponent<PlayerInventory>();
ItemData key = // ... referencia a llave
if (playerInv.Contains(key))
{
    door.Open();
}
else
{
    ShowMessage("Necesitas una llave para abrir esta puerta");
}
```

---

##### `GetItemCount(ItemData itemData)`
```csharp
public int GetItemCount(ItemData itemData)
```
**Descripción:** Obtiene la cantidad total de un ítem en el inventario (suma de todos los stacks).

**Parámetros:**
- `itemData` (ItemData): El ítem a contar

**Retorna:** `int` - Cantidad total del ítem en el inventario (0 si no existe)

**Ejemplo:**
```csharp
PlayerInventory playerInv = GetComponent<PlayerInventory>();
ItemData wood = // ... referencia a madera
int woodCount = playerInv.GetItemCount(wood);

if (woodCount >= 10)
{
    Debug.Log("Tienes suficiente madera para construir");
}
```

---

##### `GetItem(ItemData itemData)`
```csharp
public InventoryItem GetItem(ItemData itemData)
```
**Descripción:** Obtiene la primera instancia de un ítem del inventario.

**Parámetros:**
- `itemData` (ItemData): El ítem a obtener

**Retorna:** `InventoryItem` - Primera instancia del ítem, o `null` si no existe

**Ejemplo:**
```csharp
PlayerInventory playerInv = GetComponent<PlayerInventory>();
ItemData sword = // ... referencia a espada
InventoryItem swordItem = playerInv.GetItem(sword);

if (swordItem != null)
{
    Debug.Log($"Tienes {swordItem.Quantity} espadas");
    Debug.Log($"Nombre: {swordItem.Data.ItemName}");
}
```

---

##### `ClearInventory()`
```csharp
public void ClearInventory()
```
**Descripción:** Limpia completamente el inventario, eliminando todos los items.

**Ejemplo:**
```csharp
PlayerInventory playerInv = GetComponent<PlayerInventory>();
playerInv.ClearInventory(); // Útil al empezar un nuevo juego
```

---

### Inventory

**Namespace:** `Inventory`  
**Tipo:** `class` (Serializable, NO es MonoBehaviour)

#### Descripción
Lógica pura del inventario sin dependencias de Unity. Maneja dos listas separadas: items normales y extremidades (limbs).

#### Propiedades Públicas

| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| `Items` | `IReadOnlyList<InventoryItem>` | Lista de items normales (solo lectura) |
| `Limbs` | `IReadOnlyList<InventoryItem>` | Lista de extremidades (solo lectura) |
| `MaxCapacity` | `int` | Capacidad máxima de items normales |
| `MaxLimbCapacity` | `int` | Capacidad máxima de extremidades |

#### Constructor

```csharp
public Inventory(int maxCapacity = 20, int maxLimbCapacity = 10)
```

**Ejemplo:**
```csharp
// Crear un inventario personalizado
Inventory customInventory = new Inventory(maxCapacity: 50, maxLimbCapacity: 8);
```

#### Métodos Públicos

Los métodos son similares a `PlayerInventory`, pero operan directamente sobre la lógica sin eventos de Unity:
- `AddItem(ItemData itemData, int quantity)`
- `RemoveItem(ItemData itemData, int quantity)`
- `Contains(ItemData itemData)`
- `GetItemCount(ItemData itemData)`
- `GetItem(ItemData itemData)`
- `Clear()`

---

### InventoryUI

**Namespace:** `Inventory.UI`  
**Hereda de:** `MonoBehaviour`

#### Descripción
UI principal del inventario. Se encarga de mostrar todos los items, gestionar la navegación y mostrar detalles del item seleccionado.

#### Métodos Públicos

##### `ToggleInventory()`
```csharp
public void ToggleInventory()
```
**Descripción:** Alterna la visibilidad del inventario entre abierto y cerrado.

**Uso:** Este método es llamado automáticamente por el sistema de Input cuando se presiona TAB (configurable).

**Ejemplo:**
```csharp
InventoryUI inventoryUI = InventoryUIRegistry.GetActiveUI();
if (inventoryUI != null)
{
    inventoryUI.ToggleInventory();
}
```

---

##### `OpenInventory()`
```csharp
public void OpenInventory()
```
**Descripción:** Abre el panel del inventario y configura el estado del juego para la UI.

**Efectos:**
- Activa el panel de UI
- Refresca todos los slots
- Muestra y desbloquea el cursor
- Cambia el mapa de acciones del PlayerInput a "UI"

**Ejemplo:**
```csharp
// Abrir inventario al completar una misión
void OnQuestComplete()
{
    var inventoryUI = InventoryUIRegistry.GetActiveUI();
    if (inventoryUI != null)
    {
        inventoryUI.OpenInventory();
    }
}
```

---

##### `CloseInventory()`
```csharp
public void CloseInventory()
```
**Descripción:** Cierra el panel del inventario y restaura el estado del juego normal.

**Efectos:**
- Desactiva el panel de UI
- Deselecciona slots
- Oculta y bloquea el cursor
- Cambia el mapa de acciones del PlayerInput a "Player"

**Ejemplo:**
```csharp
// Cerrar inventario al iniciar una cinemática
void StartCutscene()
{
    var inventoryUI = InventoryUIRegistry.GetActiveUI();
    if (inventoryUI != null)
    {
        inventoryUI.CloseInventory();
    }
}
```

---

##### `RefreshUI()`
```csharp
public void RefreshUI()
```
**Descripción:** Refresca toda la interfaz del inventario para mostrar los items actuales.

**Cuándo se llama automáticamente:**
- Al añadir un item
- Al eliminar un item
- Al abrir el panel del inventario

**Ejemplo:**
```csharp
// Refrescar después de cargar datos guardados
void LoadInventory()
{
    // ... cargar datos ...
    var inventoryUI = InventoryUIRegistry.GetActiveUI();
    if (inventoryUI != null)
    {
        inventoryUI.RefreshUI();
    }
}
```

---

##### `ShowTooltip(InventoryItem item, Vector2 screenPosition)`
```csharp
public void ShowTooltip(InventoryItem item, Vector2 screenPosition)
```
**Descripción:** Muestra el tooltip con la información del item en la posición especificada.

**Parámetros:**
- `item` (InventoryItem): El item a mostrar en el tooltip
- `screenPosition` (Vector2): Posición en pantalla donde mostrar el tooltip

**Nota:** Este método es llamado automáticamente por `InventorySlotUI` al hacer hover.

**Ejemplo:**
```csharp
var inventoryUI = InventoryUIRegistry.GetActiveUI();
if (inventoryUI != null)
{
    Vector2 mousePos = Mouse.current.position.ReadValue();
    inventoryUI.ShowTooltip(myItem, mousePos);
}
```

---

##### `HideTooltip()`
```csharp
public void HideTooltip()
```
**Descripción:** Oculta el tooltip del inventario.

---

### InventoryUIRegistry

**Namespace:** `Inventory.UI`  
**Tipo:** `static class`

#### Descripción
Registro centralizado para acceder a la instancia activa de InventoryUI sin usar singletons.

#### Métodos Públicos

##### `GetActiveUI()`
```csharp
public static InventoryUI GetActiveUI()
```
**Descripción:** Obtiene la instancia activa de InventoryUI.

**Retorna:** `InventoryUI` - La instancia activa, o `null` si no hay ninguna

**Ejemplo:**
```csharp
var inventoryUI = InventoryUIRegistry.GetActiveUI();
if (inventoryUI != null)
{
    inventoryUI.OpenInventory();
}
```

---

### ItemData

**Namespace:** `Inventory`  
**Hereda de:** `ScriptableObject`

#### Descripción
ScriptableObject base para todos los items del juego. Define las propiedades compartidas de cualquier ítem.

#### Propiedades Públicas

| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| `ItemName` | `string` | Nombre del ítem que se muestra en la UI |
| `Description` | `string` | Descripción detallada del ítem |
| `Icon` | `Sprite` | Icono visual del ítem para la UI |
| `MaxStackSize` | `int` | Cantidad máxima que puede apilarse en un slot |
| `IsLimb` | `bool` | Indica si el ítem es una extremidad (limb) |

#### Cómo Crear un ItemData

1. En Unity, clic derecho en la carpeta Project
2. Create → Inventory → Item Data
3. Configura las propiedades en el Inspector

**Ejemplo:**
```
Nombre: Poción de Salud
Descripción: Restaura 50 puntos de salud
Icon: [Asignar sprite]
Max Stack Size: 99
Is Limb: false
```

---

### InventoryItem

**Namespace:** `Inventory`  
**Tipo:** `class` (Serializable)

#### Descripción
Representa una instancia de un ítem dentro del inventario. Contiene la referencia al ItemData y la cantidad actual.

#### Propiedades Públicas

| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| `Data` | `ItemData` | Referencia al ScriptableObject del ítem |
| `Quantity` | `int` | Cantidad actual de este stack |

#### Métodos Públicos

##### `CanStack(ItemData otherData)`
```csharp
public bool CanStack(ItemData otherData)
```
**Descripción:** Verifica si este ítem puede apilarse con otro ItemData.

**Retorna:** `bool` - `true` si puede apilarse y hay espacio

---

##### `AddQuantity(int amount)`
```csharp
public int AddQuantity(int amount)
```
**Descripción:** Añade cantidad al ítem respetando el límite de stack.

**Retorna:** `int` - La cantidad que no pudo ser añadida (overflow)

---

## Ejemplos de Uso

### Ejemplo 1: Sistema de Puertas con Llave

```csharp
using UnityEngine;
using Inventory;

public class LockedDoor : MonoBehaviour
{
    [SerializeField] private ItemData requiredKey;
    [SerializeField] private bool consumeKey = true;
    private PlayerInventory playerInventory;

    void Start()
    {
        playerInventory = FindFirstObjectByType<PlayerInventory>();
    }

    public void TryOpen()
    {
        if (playerInventory.Contains(requiredKey))
        {
            OpenDoor();
            
            if (consumeKey)
            {
                playerInventory.RemoveItem(requiredKey, 1);
                Debug.Log("Llave consumida");
            }
        }
        else
        {
            Debug.Log("Necesitas una llave para abrir esta puerta");
        }
    }

    void OpenDoor()
    {
        // Animación de apertura, etc.
        Debug.Log("¡Puerta abierta!");
    }
}
```

---

### Ejemplo 2: Sistema de Crafting

```csharp
using UnityEngine;
using Inventory;

public class CraftingStation : MonoBehaviour
{
    [System.Serializable]
    public class Recipe
    {
        public ItemData material1;
        public int material1Amount;
        public ItemData material2;
        public int material2Amount;
        public ItemData result;
    }

    [SerializeField] private Recipe[] recipes;
    private PlayerInventory playerInventory;

    void Start()
    {
        playerInventory = FindFirstObjectByType<PlayerInventory>();
    }

    public bool TryCraft(Recipe recipe)
    {
        // Verificar materiales
        if (playerInventory.GetItemCount(recipe.material1) >= recipe.material1Amount &&
            playerInventory.GetItemCount(recipe.material2) >= recipe.material2Amount)
        {
            // Remover materiales
            playerInventory.RemoveItem(recipe.material1, recipe.material1Amount);
            playerInventory.RemoveItem(recipe.material2, recipe.material2Amount);

            // Añadir resultado
            bool success = playerInventory.AddItem(recipe.result, 1);

            if (!success)
            {
                // Inventario lleno, devolver materiales
                playerInventory.AddItem(recipe.material1, recipe.material1Amount);
                playerInventory.AddItem(recipe.material2, recipe.material2Amount);
                return false;
            }

            Debug.Log($"Crafteado: {recipe.result.ItemName}");
            return true;
        }

        Debug.Log("Materiales insuficientes");
        return false;
    }
}
```

---

### Ejemplo 3: Sistema de Quests

```csharp
using UnityEngine;
using Inventory;

public class QuestManager : MonoBehaviour
{
    [System.Serializable]
    public class Quest
    {
        public string questName;
        public ItemData[] requiredItems;
        public bool removeItemsOnComplete = false;
    }

    [SerializeField] private Quest currentQuest;
    private PlayerInventory playerInventory;

    void Start()
    {
        playerInventory = FindFirstObjectByType<PlayerInventory>();
    }

    public bool CheckQuestCompletion()
    {
        foreach (var item in currentQuest.requiredItems)
        {
            if (!playerInventory.Contains(item))
            {
                return false;
            }
        }

        // Quest completada
        CompleteQuest();
        return true;
    }

    void CompleteQuest()
    {
        Debug.Log($"¡Quest '{currentQuest.questName}' completada!");

        if (currentQuest.removeItemsOnComplete)
        {
            foreach (var item in currentQuest.requiredItems)
            {
                playerInventory.RemoveItem(item, 1);
            }
        }

        // Dar recompensa, avanzar historia, etc.
    }
}
```

---

### Ejemplo 4: Recolección de Items del Mundo

```csharp
using UnityEngine;
using Inventory;

public class WorldItem : MonoBehaviour
{
    [SerializeField] private ItemData itemData;
    [SerializeField] private int quantity = 1;
    [SerializeField] private GameObject pickupEffect;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerInventory inventory = other.GetComponent<PlayerInventory>();
            
            if (inventory != null)
            {
                bool success = inventory.AddItem(itemData, quantity);

                if (success)
                {
                    Debug.Log($"Recogido: {itemData.ItemName} x{quantity}");
                    
                    if (pickupEffect != null)
                    {
                        Instantiate(pickupEffect, transform.position, Quaternion.identity);
                    }

                    Destroy(gameObject);
                }
                else
                {
                    Debug.Log("Inventario lleno");
                }
            }
        }
    }
}
```

---

### Ejemplo 5: Sistema de Tienda

```csharp
using UnityEngine;
using Inventory;

public class ShopKeeper : MonoBehaviour
{
    [System.Serializable]
    public class ShopItem
    {
        public ItemData item;
        public int price;
    }

    [SerializeField] private ShopItem[] shopInventory;
    [SerializeField] private ItemData currency; // Moneda del juego
    private PlayerInventory playerInventory;

    void Start()
    {
        playerInventory = FindFirstObjectByType<PlayerInventory>();
    }

    public bool TryBuyItem(ShopItem shopItem)
    {
        int playerMoney = playerInventory.GetItemCount(currency);

        if (playerMoney >= shopItem.price)
        {
            // Verificar si hay espacio en el inventario
            bool canAdd = playerInventory.AddItem(shopItem.item, 1);

            if (canAdd)
            {
                // Cobrar
                playerInventory.RemoveItem(currency, shopItem.price);
                Debug.Log($"Comprado: {shopItem.item.ItemName} por {shopItem.price} monedas");
                return true;
            }
            else
            {
                Debug.Log("Inventario lleno");
                return false;
            }
        }

        Debug.Log("Dinero insuficiente");
        return false;
    }

    public bool TrySellItem(ItemData item, int sellPrice)
    {
        if (playerInventory.Contains(item))
        {
            playerInventory.RemoveItem(item, 1);
            playerInventory.AddItem(currency, sellPrice);
            Debug.Log($"Vendido: {item.ItemName} por {sellPrice} monedas");
            return true;
        }

        Debug.Log("No tienes ese item");
        return false;
    }
}
```

---

## Mejores Prácticas

### ✅ DO (Hacer)

1. **Verificar siempre antes de usar:**
```csharp
var inventory = FindFirstObjectByType<PlayerInventory>();
if (inventory != null)
{
    // Usar el inventario
}
```

2. **Usar Contains antes de acciones condicionales:**
```csharp
if (playerInventory.Contains(key))
{
    door.Open();
}
```

3. **Verificar el resultado de AddItem:**
```csharp
bool success = playerInventory.AddItem(item, 1);
if (!success)
{
    ShowMessage("Inventario lleno");
}
```

4. **Usar GetItemCount para cantidades específicas:**
```csharp
if (playerInventory.GetItemCount(wood) >= 10)
{
    // Suficiente madera
}
```

### ❌ DON'T (No hacer)

1. **No asumir que AddItem siempre funciona:**
```csharp
// ❌ MAL
playerInventory.AddItem(item, 1);
// Continuar sin verificar

// ✅ BIEN
if (playerInventory.AddItem(item, 1))
{
    // Continuar
}
```

2. **No acceder directamente a Instance sin verificar:**
```csharp
// ❌ MAL (si usaras singleton)
InventoryUI.Instance.OpenInventory(); // Puede ser null

// ✅ BIEN
var ui = InventoryUIRegistry.GetActiveUI();
if (ui != null)
{
    ui.OpenInventory();
}
```

3. **No olvidar devolver materiales en caso de fallo:**
```csharp
// En crafting, si AddItem falla (inventario lleno)
if (!playerInventory.AddItem(craftedItem, 1))
{
    // Devolver materiales gastados
    playerInventory.AddItem(material1, amount1);
    playerInventory.AddItem(material2, amount2);
}
```

---

## Eventos y Callbacks

### Suscribirse a Eventos del Inventario

```csharp
using UnityEngine;
using Inventory;

public class InventoryObserver : MonoBehaviour
{
    [SerializeField] private PlayerInventory playerInventory;

    void OnEnable()
    {
        if (playerInventory != null)
        {
            playerInventory.onItemAdded.AddListener(OnItemAdded);
            playerInventory.onItemRemoved.AddListener(OnItemRemoved);
        }
    }

    void OnDisable()
    {
        if (playerInventory != null)
        {
            playerInventory.onItemAdded.RemoveListener(OnItemAdded);
            playerInventory.onItemRemoved.RemoveListener(OnItemRemoved);
        }
    }

    void OnItemAdded(ItemData item, int quantity)
    {
        Debug.Log($"Item añadido: {item.ItemName} x{quantity}");
        // Actualizar UI, mostrar notificación, etc.
    }

    void OnItemRemoved(ItemData item, int quantity)
    {
        Debug.Log($"Item eliminado: {item.ItemName} x{quantity}");
        // Actualizar estadísticas, etc.
    }
}
```

---

## Preguntas Frecuentes

### ¿Cómo verifico si el jugador tiene un item específico?
```csharp
bool hasKey = playerInventory.Contains(keyItemData);
```

### ¿Cómo obtengo la cantidad de un item?
```csharp
int goldCount = playerInventory.GetItemCount(goldItemData);
```

### ¿Cómo añado un item al inventario?
```csharp
bool success = playerInventory.AddItem(itemData, quantity);
```

### ¿Cómo abro/cierro el inventario desde código?
```csharp
var ui = InventoryUIRegistry.GetActiveUI();
if (ui != null)
{
    ui.OpenInventory();  // Abrir
    ui.CloseInventory(); // Cerrar
    ui.ToggleInventory(); // Alternar
}
```

### ¿Cómo creo un nuevo ItemData?
1. Clic derecho en Project
2. Create → Inventory → Item Data
3. Configurar propiedades en Inspector

### ¿Cómo implemento un sistema de crafting?
Ver [Ejemplo 2: Sistema de Crafting](#ejemplo-2-sistema-de-crafting)

---

## Solución de Problemas

### El inventario no se abre
- Verificar que InventoryUI esté en la escena
- Verificar que el InputSystem esté configurado correctamente
- Verificar que no haya errores en la consola

### Los items no se muestran en la UI
- Llamar a `RefreshUI()` después de modificar el inventario
- Verificar que itemsContainer y limbsContainer estén asignados
- Verificar que slotPrefab esté asignado

### AddItem retorna false siempre
- El inventario está lleno (verificar MaxCapacity)
- ItemData es null
- Verificar que la cantidad sea mayor a 0

---

## Resumen de Flujo de Trabajo Típico

1. **Crear ItemData** (ScriptableObjects)
2. **Asignar PlayerInventory** a un GameObject del jugador
3. **Configurar InventoryUI** en el Canvas
4. **Añadir items** con `AddItem()`
5. **Verificar items** con `Contains()` o `GetItemCount()`
6. **Eliminar items** con `RemoveItem()`
7. **Suscribirse a eventos** si necesitas reaccionar a cambios

---

**Versión:** 1.0  
**Última actualización:** 2025-01-26

