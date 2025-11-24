# 📦 Sistema de Inventario - Documentación Completa

Sistema de inventario modular y desacoplado para Unity, diseñado siguiendo principios de Clean Code y arquitectura limpia.

---

## 📋 Índice

1. [Características Principales](#-características-principales)
2. [Arquitectura del Sistema](#-arquitectura-del-sistema)
3. [Instalación y Configuración](#-instalación-y-configuración)
4. [API Pública - PlayerInventory](#-api-pública---playerinventory)
5. [Integración con Otros Sistemas](#-integración-con-otros-sistemas)
6. [Sistema de UI](#-sistema-de-ui)
7. [Eventos del Inventario](#-eventos-del-inventario)
8. [Ejemplos de Uso](#-ejemplos-de-uso)
9. [Extensión del Sistema](#-extensión-del-sistema)
10. [Troubleshooting](#-troubleshooting)

---

## ✨ Características Principales

- ✅ **Arquitectura desacoplada**: Lógica separada de MonoBehaviour
- ✅ **Dos categorías de items**: Items normales y extremidades (limbs)
- ✅ **Sistema de stacking**: Items apilables con límite configurable
- ✅ **UI completa**: Inventario visual con tooltips y dos contenedores
- ✅ **Sistema de eventos**: UnityEvents para suscripciones reactivas
- ✅ **Nuevo Input System**: Integrado con Unity Input System (Send Messages)
- ✅ **Interacción con el mundo**: Sistema IInteractable para pickups
- ✅ **Fácil extensión**: ScriptableObject base para crear nuevos items
- ✅ **Sin dependencias innecesarias**: Cada componente es independiente

---

## 🏗️ Arquitectura del Sistema

### Componentes Principales

```
┌─────────────────────────────────────────────────────────────┐
│                    PlayerInventory                          │
│  (MonoBehaviour - API Pública - Punto de entrada)          │
└────────────┬────────────────────────────────────────────────┘
             │
             ├── Inventory (Lógica pura - Sin MonoBehaviour)
             │   ├── List<InventoryItem> items
             │   └── List<InventoryItem> limbs
             │
             ├── ItemData (ScriptableObject - Definición de items)
             │
             ├── InventoryItem (Instancia de item en inventario)
             │
             └── UnityEvents (onItemAdded, onItemRemoved)
                     │
                     └── InventoryUI (Suscriptor automático)
```

### Separación de Responsabilidades

| Componente | Responsabilidad |
|------------|-----------------|
| **PlayerInventory** | Punto de entrada público, gestión de eventos |
| **Inventory** | Lógica pura del inventario (añadir, quitar, buscar) |
| **ItemData** | Definición estática de items (ScriptableObject) |
| **InventoryItem** | Instancia con cantidad y estado |
| **InventoryUI** | Visualización y interacción con UI |
| **WorldInventoryItem** | Representación física en el mundo (pickup) |

---

## 🚀 Instalación y Configuración

### 1. Setup Básico del Player

1. **Añade `PlayerInventory` al GameObject del Player:**
   ```
   Player GameObject
   ├── PlayerInput (ya existente)
   ├── PlayerInventory ← Añade este componente
   └── ... otros componentes
   ```

2. **Configura PlayerInventory en el Inspector:**
   - `Max Capacity`: Capacidad máxima de items normales (default: 20)
   - `Max Limb Capacity`: Capacidad máxima de extremidades (default: 4)
   - `Show Debug Logs`: Muestra logs de debug (útil para desarrollo)

3. **Configurar PlayerInput:**
   - Asegúrate de que `Behavior` esté en **"Send Messages"**
   - Verifica que exista la acción **"ToggleInventory"** (ej: tecla Tab)

### 2. Crear Items (ScriptableObjects)

1. **Click derecho en Project** → Create → Inventory → Item Data
2. **Configura el ItemData:**
   ```
   Item Name: Nombre del item
   Description: Descripción que aparece en tooltip
   Icon: Sprite visual del item
   Max Stack Size: Cuántos items caben en un stack
   Is Limb: ¿Es una extremidad? (true/false)
   ```

### 3. Setup de la UI

1. **Crear el Canvas de Inventario:**
   ```
   Canvas
   ├── InventoryPanel (GameObject con InventoryUI component)
   │   ├── ItemsContainer (VerticalLayoutGroup o GridLayoutGroup)
   │   ├── LimbsContainer (VerticalLayoutGroup)
   │   └── TooltipPanel (Panel con CanvasGroup)
   │       ├── NameText (TextMeshProUGUI)
   │       └── DescriptionText (TextMeshProUGUI)
   └── EventSystem (necesario para eventos UI)
   ```

2. **Crear SlotPrefab:**
   - Prefab con componente `InventorySlotUI`
   - Debe tener: Image (icono), Text (nombre), Text (cantidad), Image (fondo)
   - Añadir componente `Button` para clicks

3. **Configurar TooltipPanel:**
   - Añadir `CanvasGroup`: Blocks Raycasts = OFF (evita parpadeo)
   - Añadir `VerticalLayoutGroup` y `ContentSizeFitter`

4. **Asignar referencias en InventoryUI Inspector:**
   - Arrastra el PlayerInventory
   - Asigna itemsContainer, limbsContainer, slotPrefab
   - Asigna tooltipPanel, tooltipNameText, tooltipDescriptionText
   - Ajusta `Tooltip Offset` (ej: 15, -15 para abajo-derecha)

### 4. Crear Items en el Mundo (Pickups)

1. **Crear GameObject en escena**
2. **Añadir componentes:**
   - `WorldInventoryItem` (script del sistema)
   - Tu implementación de `IInteractable`
3. **Configurar WorldInventoryItem:**
   - Asignar el `ItemData` correspondiente
   - Cantidad inicial
   - (Opcional) GameObject visual que se destruirá al recoger

---

## 📚 API Pública - PlayerInventory

### Métodos Principales

#### ✅ **AddItem** - Añadir items al inventario
```csharp
public bool AddItem(ItemData itemData, int quantity = 1)
```
**Parámetros:**
- `itemData`: ScriptableObject del item a añadir
- `quantity`: Cantidad a añadir (default: 1)

**Retorna:** `true` si se añadió exitosamente, `false` si el inventario está lleno

**Ejemplo:**
```csharp
PlayerInventory inventory = GetComponent<PlayerInventory>();
ItemData potion = ... // Tu ItemData
bool success = inventory.AddItem(potion, 3);
if (success) {
    Debug.Log("¡3 pociones añadidas!");
}
```

---

#### ❌ **RemoveItem** - Quitar items del inventario
```csharp
public bool RemoveItem(ItemData itemData, int quantity = 1)
```
**Parámetros:**
- `itemData`: ScriptableObject del item a quitar
- `quantity`: Cantidad a quitar (default: 1)

**Retorna:** `true` si se quitó exitosamente, `false` si no había suficientes

**Ejemplo:**
```csharp
bool consumed = inventory.RemoveItem(potion, 1);
if (consumed) {
    // Aplicar efecto de la poción
    health += 50;
}
```

---

#### 🔍 **Contains** - Verificar si existe un item
```csharp
public bool Contains(ItemData itemData)
```
**Parámetros:**
- `itemData`: ScriptableObject del item a buscar

**Retorna:** `true` si existe al menos 1, `false` si no

**Ejemplo:**
```csharp
ItemData key = ... // Tu llave
if (inventory.Contains(key)) {
    // Abrir puerta
    door.Unlock();
}
```

---

#### 🔢 **GetItemCount** - Obtener cantidad de un item
```csharp
public int GetItemCount(ItemData itemData)
```
**Parámetros:**
- `itemData`: ScriptableObject del item

**Retorna:** Cantidad total del item (suma de todos los stacks)

**Ejemplo:**
```csharp
int totalGold = inventory.GetItemCount(goldCoin);
Debug.Log($"Tienes {totalGold} monedas de oro");
```

---

#### 📦 **GetItem** - Obtener instancia del item
```csharp
public InventoryItem GetItem(ItemData itemData)
```
**Parámetros:**
- `itemData`: ScriptableObject del item

**Retorna:** `InventoryItem` (instancia con Data y Quantity), o `null` si no existe

**Ejemplo:**
```csharp
InventoryItem item = inventory.GetItem(potion);
if (item != null) {
    Debug.Log($"Tienes {item.Quantity} {item.Data.ItemName}");
}
```

---

#### 🗑️ **ClearInventory** - Vaciar completamente el inventario
```csharp
public void ClearInventory()
```
**Sin parámetros**

**Ejemplo:**
```csharp
// Al morir el jugador
inventory.ClearInventory();
```

---

#### 📊 **Acceso a la Lógica Interna**
```csharp
public Inventory Inventory { get; }
```
**Propiedad de solo lectura** que da acceso directo al objeto `Inventory` interno.

**Uso avanzado:**
```csharp
// Ver todos los items normales
IReadOnlyList<InventoryItem> items = inventory.Inventory.Items;
foreach (var item in items) {
    Debug.Log($"{item.Data.ItemName} x{item.Quantity}");
}

// Ver todas las extremidades
IReadOnlyList<InventoryItem> limbs = inventory.Inventory.Limbs;

// Capacidades máximas
int maxItems = inventory.Inventory.MaxCapacity;
int maxLimbs = inventory.Inventory.MaxLimbCapacity;
```

---

## 🔗 Integración con Otros Sistemas

### Sistema de Crafting

```csharp
using Inventory;

public class CraftingSystem : MonoBehaviour
{
    [SerializeField] private PlayerInventory playerInventory;
    
    // Definir receta
    [SerializeField] private ItemData ingrediente1;
    [SerializeField] private ItemData ingrediente2;
    [SerializeField] private ItemData resultado;
    
    public bool Trycraft()
    {
        // Verificar si tiene los ingredientes
        if (!playerInventory.Contains(ingrediente1) || 
            !playerInventory.Contains(ingrediente2))
        {
            Debug.Log("No tienes los ingredientes necesarios");
            return false;
        }
        
        // Consumir ingredientes
        playerInventory.RemoveItem(ingrediente1, 1);
        playerInventory.RemoveItem(ingrediente2, 1);
        
        // Dar resultado
        playerInventory.AddItem(resultado, 1);
        
        Debug.Log($"¡Crafteaste {resultado.ItemName}!");
        return true;
    }
    
    public bool CanCraft()
    {
        return playerInventory.Contains(ingrediente1) && 
               playerInventory.Contains(ingrediente2);
    }
}
```

---

### Sistema de Quests

```csharp
using Inventory;

public class Quest : MonoBehaviour
{
    [SerializeField] private ItemData itemRequerido;
    [SerializeField] private int cantidadRequerida = 5;
    private PlayerInventory _playerInventory;
    
    private void Start()
    {
        _playerInventory = FindObjectOfType<PlayerInventory>();
        
        // Suscribirse a eventos del inventario
        _playerInventory.onItemAdded.AddListener(OnItemAdded);
    }
    
    private void OnItemAdded(ItemData itemData, int quantity)
    {
        // Comprobar si es el item que necesitamos
        if (itemData == itemRequerido)
        {
            int cantidad = _playerInventory.GetItemCount(itemRequerido);
            
            if (cantidad >= cantidadRequerida)
            {
                CompleteQuest();
            }
            else
            {
                Debug.Log($"Progreso: {cantidad}/{cantidadRequerida}");
            }
        }
    }
    
    private void CompleteQuest()
    {
        Debug.Log("¡Quest completada!");
        // Consumir items
        _playerInventory.RemoveItem(itemRequerido, cantidadRequerida);
        // Dar recompensa...
    }
    
    private void OnDestroy()
    {
        if (_playerInventory != null)
            _playerInventory.onItemAdded.RemoveListener(OnItemAdded);
    }
}
```

---

### Sistema de Tienda/Comercio

```csharp
using Inventory;

public class ShopSystem : MonoBehaviour
{
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private ItemData moneda; // Ej: moneda de oro
    
    [System.Serializable]
    public class ShopItem
    {
        public ItemData item;
        public int precio;
    }
    
    [SerializeField] private List<ShopItem> itemsEnVenta;
    
    public bool TryBuy(ShopItem shopItem)
    {
        // Verificar si tiene suficiente dinero
        int dineroActual = playerInventory.GetItemCount(moneda);
        
        if (dineroActual < shopItem.precio)
        {
            Debug.Log("No tienes suficiente dinero");
            return false;
        }
        
        // Verificar si hay espacio en el inventario
        if (!TieneEspacio(shopItem.item))
        {
            Debug.Log("Inventario lleno");
            return false;
        }
        
        // Realizar compra
        playerInventory.RemoveItem(moneda, shopItem.precio);
        playerInventory.AddItem(shopItem.item, 1);
        
        Debug.Log($"Compraste {shopItem.item.ItemName} por {shopItem.precio} monedas");
        return true;
    }
    
    public bool TrySell(ItemData item, int precio)
    {
        if (!playerInventory.Contains(item))
        {
            Debug.Log("No tienes ese item");
            return false;
        }
        
        playerInventory.RemoveItem(item, 1);
        playerInventory.AddItem(moneda, precio);
        
        Debug.Log($"Vendiste {item.ItemName} por {precio} monedas");
        return true;
    }
    
    private bool TieneEspacio(ItemData item)
    {
        // Intentar añadir temporalmente para comprobar espacio
        // (esto es una simplificación, podrías hacer una verificación más compleja)
        return playerInventory.Inventory.Items.Count < 
               playerInventory.Inventory.MaxCapacity;
    }
}
```

---

### Sistema de Puertas/Cerraduras

```csharp
using Inventory;
using InteractionSystem.Core;

public class LockedDoor : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemData llaveRequerida;
    [SerializeField] private bool consumirLlave = false;
    private bool _abierta = false;
    
    public void Interact(GameObject interactor)
    {
        if (_abierta)
        {
            Debug.Log("La puerta ya está abierta");
            return;
        }
        
        PlayerInventory inventory = interactor.GetComponent<PlayerInventory>();
        if (inventory == null)
        {
            Debug.LogWarning("El interactor no tiene inventario");
            return;
        }
        
        if (inventory.Contains(llaveRequerida))
        {
            // Abrir puerta
            _abierta = true;
            Debug.Log("¡Puerta desbloqueada!");
            
            // Opcional: consumir llave
            if (consumirLlave)
            {
                inventory.RemoveItem(llaveRequerida, 1);
                Debug.Log("La llave se ha consumido");
            }
            
            // Animación/efecto de abrir puerta
            GetComponent<Animator>()?.SetTrigger("Open");
        }
        else
        {
            Debug.Log($"Necesitas: {llaveRequerida.ItemName}");
        }
    }
    
    public string GetInteractionPrompt()
    {
        return _abierta ? "Puerta Abierta" : $"Abrir (Requiere: {llaveRequerida.ItemName})";
    }
}
```

---

### Sistema de Condiciones Complejas

```csharp
using Inventory;

public class PuzzleDoor : MonoBehaviour
{
    [System.Serializable]
    public class ItemRequirement
    {
        public ItemData item;
        public int cantidad;
    }
    
    [SerializeField] private List<ItemRequirement> requisitos;
    [SerializeField] private PlayerInventory playerInventory;
    
    public bool CheckRequirements()
    {
        foreach (var req in requisitos)
        {
            int cantidad = playerInventory.GetItemCount(req.item);
            if (cantidad < req.cantidad)
            {
                Debug.Log($"Faltan {req.cantidad - cantidad} {req.item.ItemName}");
                return false;
            }
        }
        return true;
    }
    
    public void TryOpen()
    {
        if (!CheckRequirements())
        {
            Debug.Log("No cumples los requisitos");
            return;
        }
        
        // Consumir todos los items requeridos
        foreach (var req in requisitos)
        {
            playerInventory.RemoveItem(req.item, req.cantidad);
        }
        
        Debug.Log("¡Puzzle resuelto! Puerta abierta");
        // Abrir puerta...
    }
}
```

---

## 🎨 Sistema de UI

### InventoryUI - Componente Principal

El sistema incluye una UI completa y funcional:

**Características:**
- ✅ Dos contenedores: Items normales y Limbs (extremidades)
- ✅ Tooltip al pasar el ratón (configurable)
- ✅ Selección visual de slots
- ✅ Actualización automática al añadir/quitar items
- ✅ Toggle con Input System (Tab por defecto)
- ✅ Cursor visible/oculto automático

**Configuración en Inspector:**
```
[Referencias Principales]
- Player Inventory: Referencia al PlayerInventory
- Inventory Panel: Panel raíz de la UI

[Contenedores de Slots]
- Items Container: Transform donde se generan slots de items
- Limbs Container: Transform donde se generan slots de limbs

[Prefab de Slot]
- Slot Prefab: Prefab con InventorySlotUI component

[Tooltip]
- Tooltip Panel: GameObject del tooltip
- Tooltip Name Text: Texto del nombre
- Tooltip Description Text: Texto de la descripción
- Tooltip Offset: Vector2 (ej: 15, -15) posición relativa al cursor

[Configuración]
- Max Slots To Display: Cantidad máxima de slots de items (20)
- Hide On Start: Ocultar inventario al iniciar
```

### InventorySlotUI - Slot Individual

Representa un slot del inventario visualmente.

**Referencias UI requeridas:**
- Icon Image: Sprite del item
- Name Text: Nombre del item (opcional)
- Quantity Text: Cantidad del item
- Quantity Container: GameObject que contiene el texto de cantidad
- Background Image: Fondo del slot

**Colores configurables:**
- Empty Color: Color cuando está vacío
- Filled Color: Color cuando tiene item
- Selected Color: Color cuando está seleccionado

**Fallbacks:**
- Placeholder Icon: Sprite que se muestra si el item no tiene icono

### Registro de UI

El sistema usa un registro ligero (`InventoryUIRegistry`) para localizar la UI sin búsquedas costosas:

```csharp
// Obtener la UI activa
var ui = InventoryUIRegistry.Get();
if (ui != null)
{
    ui.ToggleInventory();
}
```

---

## 📡 Eventos del Inventario

### UnityEvents Disponibles

El `PlayerInventory` expone eventos públicos para reaccionar a cambios:

```csharp
public UnityEvent<ItemData, int> onItemAdded;
public UnityEvent<ItemData, int> onItemRemoved;
```

### Suscripción desde Código

```csharp
using Inventory;
using UnityEngine;

public class InventoryObserver : MonoBehaviour
{
    [SerializeField] private PlayerInventory playerInventory;
    
    private void OnEnable()
    {
        playerInventory.onItemAdded.AddListener(OnItemAdded);
        playerInventory.onItemRemoved.AddListener(OnItemRemoved);
    }
    
    private void OnDisable()
    {
        playerInventory.onItemAdded.RemoveListener(OnItemAdded);
        playerInventory.onItemRemoved.RemoveListener(OnItemRemoved);
    }
    
    private void OnItemAdded(ItemData itemData, int quantity)
    {
        Debug.Log($"Añadido: {itemData.ItemName} x{quantity}");
        
        // Reaccionar según el item
        if (itemData.ItemName == "Poción de Salud")
        {
            // Mostrar notificación
            ShowNotification($"+{quantity} Poción");
        }
    }
    
    private void OnItemRemoved(ItemData itemData, int quantity)
    {
        Debug.Log($"Quitado: {itemData.ItemName} x{quantity}");
    }
}
```

### Casos de Uso de Eventos

1. **Sistema de Notificaciones:**
   - Mostrar popup cuando se añade un item
   - Efecto visual/sonido

2. **Sistema de Logros:**
   - "Recolecta 100 monedas de oro"
   - "Obtén tu primera espada"

3. **Sistema de Estadísticas:**
   - Tracking de items recogidos
   - Analytics del jugador

4. **Sistema de Tutoriales:**
   - "Has recogido tu primera poción, úsala con clic derecho"

5. **Actualización de UI Personalizada:**
   - Actualizar displays personalizados
   - Mostrar contador específico

---

## 💡 Ejemplos de Uso

### Ejemplo 1: Pickup Simple

```csharp
using Inventory;
using InteractionSystem.Core;
using UnityEngine;

public class ItemPickup : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemData itemData;
    [SerializeField] private int quantity = 1;
    [SerializeField] private GameObject visualModel;
    
    public void Interact(GameObject interactor)
    {
        PlayerInventory inventory = interactor.GetComponent<PlayerInventory>();
        if (inventory == null)
        {
            Debug.LogWarning("El interactor no tiene inventario");
            return;
        }
        
        bool added = inventory.AddItem(itemData, quantity);
        
        if (added)
        {
            Debug.Log($"Recogido: {itemData.ItemName} x{quantity}");
            
            // Destruir el objeto visual
            if (visualModel != null)
                Destroy(visualModel);
            else
                Destroy(gameObject);
        }
        else
        {
            Debug.Log("Inventario lleno");
        }
    }
    
    public string GetInteractionPrompt()
    {
        return $"Recoger {itemData.ItemName} x{quantity}";
    }
}
```

---

### Ejemplo 2: Consumible (Poción de Salud)

```csharp
using Inventory;
using UnityEngine;

public class HealthPotion : MonoBehaviour
{
    [SerializeField] private ItemData healthPotionData;
    [SerializeField] private int healAmount = 50;
    private PlayerInventory _inventory;
    private PlayerHealth _health;
    
    private void Start()
    {
        _inventory = GetComponent<PlayerInventory>();
        _health = GetComponent<PlayerHealth>();
    }
    
    private void Update()
    {
        // Usar poción con tecla "H"
        if (Input.GetKeyDown(KeyCode.H))
        {
            TryUsePotion();
        }
    }
    
    private void TryUsePotion()
    {
        if (!_inventory.Contains(healthPotionData))
        {
            Debug.Log("No tienes pociones");
            return;
        }
        
        if (_health.IsFullHealth())
        {
            Debug.Log("Tu salud ya está llena");
            return;
        }
        
        // Consumir poción
        _inventory.RemoveItem(healthPotionData, 1);
        
        // Aplicar curación
        _health.Heal(healAmount);
        
        Debug.Log($"Usaste una poción y recuperaste {healAmount} HP");
    }
}
```

---

### Ejemplo 3: Sistema de Drop al Morir

```csharp
using Inventory;
using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private GameObject itemDropPrefab;
    [SerializeField] private float dropRadius = 2f;
    
    public void OnPlayerDeath()
    {
        // Dropear todos los items del inventario
        DropAllItems();
        
        // Limpiar inventario
        inventory.ClearInventory();
    }
    
    private void DropAllItems()
    {
        var allItems = inventory.Inventory.Items;
        
        foreach (var item in allItems)
        {
            // Crear drop en el mundo
            Vector3 dropPosition = transform.position + Random.insideUnitSphere * dropRadius;
            dropPosition.y = transform.position.y; // Mantener altura
            
            GameObject drop = Instantiate(itemDropPrefab, dropPosition, Quaternion.identity);
            
            // Configurar el drop con el item
            WorldInventoryItem worldItem = drop.GetComponent<WorldInventoryItem>();
            if (worldItem != null)
            {
                worldItem.SetItem(item.Data, item.Quantity);
            }
        }
        
        Debug.Log($"Dropeados {allItems.Count} items");
    }
}
```

---

### Ejemplo 4: Verificación de Múltiples Items

```csharp
using Inventory;
using UnityEngine;
using System.Collections.Generic;

public class RecipeChecker : MonoBehaviour
{
    [System.Serializable]
    public class Recipe
    {
        public string recipeName;
        public List<IngredientRequirement> ingredients;
        public ItemData result;
    }
    
    [System.Serializable]
    public class IngredientRequirement
    {
        public ItemData item;
        public int cantidad;
    }
    
    [SerializeField] private List<Recipe> recipes;
    [SerializeField] private PlayerInventory inventory;
    
    public bool CanCraftRecipe(Recipe recipe)
    {
        foreach (var ingredient in recipe.ingredients)
        {
            int currentAmount = inventory.GetItemCount(ingredient.item);
            if (currentAmount < ingredient.cantidad)
            {
                return false;
            }
        }
        return true;
    }
    
    public void CraftRecipe(Recipe recipe)
    {
        if (!CanCraftRecipe(recipe))
        {
            Debug.Log("No tienes todos los ingredientes");
            return;
        }
        
        // Consumir ingredientes
        foreach (var ingredient in recipe.ingredients)
        {
            inventory.RemoveItem(ingredient.item, ingredient.cantidad);
        }
        
        // Dar resultado
        inventory.AddItem(recipe.result, 1);
        
        Debug.Log($"¡Crafteaste {recipe.result.ItemName}!");
    }
    
    public List<Recipe> GetAvailableRecipes()
    {
        List<Recipe> available = new List<Recipe>();
        
        foreach (var recipe in recipes)
        {
            if (CanCraftRecipe(recipe))
            {
                available.Add(recipe);
            }
        }
        
        return available;
    }
}
```

---

## 🔧 Extensión del Sistema

### Crear Nuevos Tipos de Items

Puedes extender `ItemData` para crear tipos especializados:

```csharp
using UnityEngine;

namespace Inventory
{
    [CreateAssetMenu(fileName = "New Weapon", menuName = "Inventory/Weapon Data")]
    public class WeaponData : ItemData
    {
        [Header("Weapon Stats")]
        public int damage = 10;
        public float attackSpeed = 1.0f;
        public WeaponType weaponType;
        
        public enum WeaponType
        {
            Sword,
            Axe,
            Bow,
            Staff
        }
    }
}
```

Uso:
```csharp
WeaponData weapon = item.Data as WeaponData;
if (weapon != null)
{
    Debug.Log($"Daño: {weapon.damage}, Velocidad: {weapon.attackSpeed}");
}
```

---

### Añadir Funcionalidad Personalizada

Crea un wrapper o sistema que extienda la funcionalidad:

```csharp
using Inventory;
using UnityEngine;

public class InventoryExtensions : MonoBehaviour
{
    [SerializeField] private PlayerInventory inventory;
    
    // Obtener todos los items de un tipo específico
    public List<InventoryItem> GetItemsOfType<T>() where T : ItemData
    {
        List<InventoryItem> result = new List<InventoryItem>();
        
        foreach (var item in inventory.Inventory.Items)
        {
            if (item.Data is T)
            {
                result.Add(item);
            }
        }
        
        return result;
    }
    
    // Obtener el item más valioso (por ejemplo)
    public InventoryItem GetMostValuableItem()
    {
        InventoryItem mostValuable = null;
        int highestValue = 0;
        
        foreach (var item in inventory.Inventory.Items)
        {
            // Asumiendo que tienes una propiedad Value
            // int value = (item.Data as ValuableItemData)?.value ?? 0;
            // if (value > highestValue) { ... }
        }
        
        return mostValuable;
    }
    
    // Transferir items a otro inventario
    public bool TransferItem(PlayerInventory targetInventory, ItemData itemData, int quantity)
    {
        if (!inventory.Contains(itemData))
            return false;
        
        int available = inventory.GetItemCount(itemData);
        int toTransfer = Mathf.Min(available, quantity);
        
        bool added = targetInventory.AddItem(itemData, toTransfer);
        if (added)
        {
            inventory.RemoveItem(itemData, toTransfer);
            return true;
        }
        
        return false;
    }
}
```

---

## 🐛 Troubleshooting

### Problema: El inventario no se abre con Tab

**Solución:**
1. Verifica que `PlayerInput` tenga `Behavior = Send Messages`
2. Asegúrate de que la acción se llama exactamente "ToggleInventory"
3. Verifica que `PlayerInventory` esté en el mismo GameObject que `PlayerInput`
4. Comprueba que existe un `InventoryUI` en la escena

---

### Problema: Los items no se muestran en la UI

**Solución:**
1. Verifica que `InventoryUI` tenga las referencias asignadas:
   - `playerInventory`
   - `itemsContainer` y `limbsContainer`
   - `slotPrefab`
2. Comprueba que el `slotPrefab` tenga el componente `InventorySlotUI`
3. Revisa la consola por warnings sobre slots o referencias null

---

### Problema: El tooltip parpadea

**Solución:**
1. Añade `CanvasGroup` al `TooltipPanel`
2. Desactiva `Blocks Raycasts` en el CanvasGroup
3. Desactiva `Interactable` en el CanvasGroup

---

### Problema: Los limbs aparecen en el contenedor de items

**Solución:**
1. Verifica en el Inspector de `InventoryUI` que:
   - `itemsContainer` apunta al contenedor correcto
   - `limbsContainer` apunta al contenedor correcto
2. Asegúrate de que el `ItemData` tenga `Is Limb` marcado correctamente
3. Revisa la consola por warnings de containers cruzados

---

### Problema: El inventario está lleno pero parece vacío

**Solución:**
1. Verifica que la UI se esté actualizando:
   - Comprueba que `InventoryUI.OnEnable()` se suscribe a los eventos
2. Revisa que `RefreshUI()` se esté llamando correctamente
3. Comprueba en debug logs si los items se están añadiendo realmente

---

## 📝 Notas Finales

### Buenas Prácticas

1. **Siempre usa la API pública de `PlayerInventory`**, no modifiques directamente el objeto `Inventory` interno
2. **Suscríbete y desuscríbete de eventos** correctamente (OnEnable/OnDisable)
3. **Comprueba valores null** antes de acceder a propiedades
4. **Usa los eventos del inventario** para sistemas reactivos en lugar de polling constante
5. **Crea ItemData como ScriptableObjects** reutilizables en lugar de instanciarlos en código

### Consideraciones de Rendimiento

- El sistema usa listas para almacenar items, óptimo para inventarios de tamaño moderado (<100 items)
- Los eventos UnityEvent tienen overhead mínimo
- La UI se actualiza solo cuando cambia el inventario (reactivo, no por frame)
- `InventoryUIRegistry` evita búsquedas costosas con FindObjectOfType

### Compatibilidad

- ✅ Unity 2021.3+
- ✅ Nuevo Input System (Input System Package)
- ✅ TextMeshPro (incluido en Unity por defecto)
- ✅ URP/HDRP/Built-in

---

## 📞 Soporte

Para dudas o problemas:
1. Revisa la sección [Troubleshooting](#-troubleshooting)
2. Consulta los [Ejemplos de Uso](#-ejemplos-de-uso)
3. Revisa los comentarios en el código fuente

---

## 📄 Licencia y Créditos

Sistema de Inventario creado siguiendo principios de Clean Code y arquitectura desacoplada.

**Autor:** [Tu Nombre/Equipo]  
**Versión:** 1.0.0  
**Fecha:** 2025-01-24

---

¡Listo para usar! 🚀 Integra el sistema de inventario en tu proyecto y extiéndelo según tus necesidades.

