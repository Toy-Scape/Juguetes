# 🎮 Input System con Unity Messages - Patrón Correcto

## ✅ Implementación Final

El sistema de inventario ahora usa **Unity Messages automáticos**, exactamente igual que `PlayerController`.

---

## 🎯 Cómo Funciona

### Unity Messages Automáticos

Cuando tienes un **PlayerInput component** en el mismo GameObject (o padre) con el Input System configurado, Unity **llama automáticamente** a los métodos que coincidan con el nombre de las acciones.

### Patrón de PlayerController

```csharp
public class PlayerController : MonoBehaviour
{
    // Sin referencias a InputSystem_Actions
    // Sin instancias de InputActionAsset
    
    #region "Inputs"
    void OnMove(InputValue value)
    {
        playerContext.MoveInput = value.Get<Vector2>();
    }

    void OnJump(InputValue value)
    {
        playerContext.IsJumping = value.isPressed;
    }

    void OnInteract()
    {
        playerContext.IsInteracting = true;
    }
    #endregion
}
```

**Unity llama automáticamente estos métodos** cuando:
- Hay un `PlayerInput` component configurado
- El GameObject tiene el script
- El nombre del método coincide con el nombre de la acción (`OnNombreAccion`)

---

## 📝 PlayerInventoryIntegration (ACTUALIZADO)

```csharp
using UnityEngine;
using Inventory;

namespace Player
{
    public class PlayerInventoryIntegration : MonoBehaviour
    {
        [SerializeField] private InventoryComponent inventoryComponent;
        [SerializeField] private bool showDebugInfo = true;

        private void Start()
        {
            if (inventoryComponent == null)
                inventoryComponent = GetComponent<InventoryComponent>();

            inventoryComponent.onItemAdded.AddListener(OnItemAdded);
            inventoryComponent.onItemRemoved.AddListener(OnItemRemoved);
        }

        private void OnDestroy()
        {
            if (inventoryComponent != null)
            {
                inventoryComponent.onItemAdded.RemoveListener(OnItemAdded);
                inventoryComponent.onItemRemoved.RemoveListener(OnItemRemoved);
            }
        }

        #region Input Actions (Unity Messages)

        void OnToggleInventory()  // ← Unity llama esto automáticamente
        {
            if (showDebugInfo)
            {
                ShowInventoryInfo();
            }
        }

        #endregion

        // ... resto del código
    }
}
```

---

## 🔧 Configuración en Unity

### 1. El Player ya tiene PlayerInput

Tu Player ya tiene configurado:
- **PlayerInput component**
- **Actions**: `InputSystem_Actions` asset
- **Behavior**: Send Messages (o Invoke Unity Events)

### 2. Solo Añade el Script

```
Player GameObject
├── PlayerInput (ya existe)
├── PlayerController (ya existe)
├── InventoryComponent
└── PlayerInventoryIntegration  ← Añadir
```

### 3. Funciona Automáticamente

Cuando presionas `I`:
1. PlayerInput detecta la acción "ToggleInventory"
2. Unity busca el método `OnToggleInventory()` en todos los scripts del GameObject
3. Llama automáticamente al método

**¡Sin configuración adicional!**

---

## 🆚 Comparación

### ❌ Antes (Incorrecto)

```csharp
public class PlayerInventoryIntegration : MonoBehaviour, InputSystem_Actions.IPlayerActions
{
    private InputSystem_Actions inputActions;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
        inputActions.Player.AddCallbacks(this);
    }

    public void OnToggleInventory(InputAction.CallbackContext context)
    {
        if (context.performed) { /* ... */ }
    }

    // Implementar TODOS los métodos de IPlayerActions
    public void OnMove(...) { }
    public void OnJump(...) { }
    // ... etc (12+ métodos vacíos)
}
```

**Problemas:**
- ❌ Instanciaba InputSystem_Actions innecesariamente
- ❌ Implementaba interfaz completa
- ❌ Muchos métodos vacíos
- ❌ Código duplicado con PlayerController

### ✅ Ahora (Correcto - Igual que PlayerController)

```csharp
public class PlayerInventoryIntegration : MonoBehaviour
{
    // Sin InputSystem_Actions
    // Sin interfaces
    
    #region Input Actions

    void OnToggleInventory()  // ← Simple, limpio
    {
        ShowInventoryInfo();
    }

    #endregion
}
```

**Ventajas:**
- ✅ Usa el PlayerInput existente del Player
- ✅ Sin código duplicado
- ✅ Solo implementa lo que necesita
- ✅ Patrón consistente con PlayerController
- ✅ Más simple y limpio

---

## 📋 Estructura Final

```
┌──────────────────────────────────────┐
│   InputSystem_Actions.inputactions   │
│   (Asset)                            │
└────────────┬─────────────────────────┘
             │
             ▼
┌──────────────────────────────────────┐
│   PlayerInput Component              │
│   (en el GameObject Player)          │
│   - Actions: InputSystem_Actions     │
│   - Behavior: Send Messages          │
└────────────┬─────────────────────────┘
             │ busca métodos OnNombreAccion
             │
             ├─────────────────────┐
             │                     │
             ▼                     ▼
┌─────────────────────┐  ┌────────────────────────┐
│  PlayerController   │  │ PlayerInventoryInteg.. │
│  - OnMove()         │  │ - OnToggleInventory()  │
│  - OnJump()         │  │                        │
│  - OnInteract()     │  │                        │
└─────────────────────┘  └────────────────────────┘
```

Unity llama automáticamente a `OnToggleInventory()` en **todos** los MonoBehaviours del GameObject que tengan ese método.

---

## 🎮 Uso

### En el Inspector

Solo verifica que el Player tenga:
- ✅ `PlayerInput` component
- ✅ `InventoryComponent`
- ✅ `PlayerInventoryIntegration`

### En el Juego

- Presiona **I** → `OnToggleInventory()` se llama automáticamente
- Sin configuración
- Sin serialización
- Sin instancias

---

## 💡 Ventajas del Patrón

### 1. Consistencia
Todos los scripts del Player usan el mismo patrón:
- `PlayerController` → `OnMove()`, `OnJump()`, etc.
- `PlayerInventoryIntegration` → `OnToggleInventory()`

### 2. Simplicidad
```csharp
void OnToggleInventory()  // ← 1 línea de declaración
{
    ShowInventoryInfo();   // ← Tu lógica
}
```

### 3. Sin Duplicación
Un solo `PlayerInput` en el Player maneja todo el input para todos los scripts.

### 4. Fácil de Extender
Quieres añadir otra acción? Solo añade el método:
```csharp
void OnUseItem()
{
    // Tu lógica
}
```

---

## 📚 Ejemplos

### PlayerInventoryIntegration

```csharp
public class PlayerInventoryIntegration : MonoBehaviour
{
    [SerializeField] private InventoryComponent inventory;

    void OnToggleInventory()
    {
        Debug.Log("Inventario toggled!");
    }
}
```

### InventoryUsageExample

```csharp
public class InventoryUsageExample : MonoBehaviour
{
    [SerializeField] private InventoryComponent inventory;

    void OnToggleInventory()
    {
        CheckInventory();
    }
}
```

### Custom Script

```csharp
public class MyCustomScript : MonoBehaviour
{
    void OnToggleInventory()
    {
        // Tu lógica personalizada
    }
}
```

Todos funcionan automáticamente si están en el mismo GameObject que tiene `PlayerInput`.

---

## ⚠️ Importante

### Firma del Método

Puedes usar cualquiera de estas firmas:

```csharp
// Opción 1: Sin parámetros (más simple)
void OnToggleInventory()
{
    // ...
}

// Opción 2: Con InputValue (si necesitas el valor)
void OnToggleInventory(InputValue value)
{
    bool pressed = value.isPressed;
}

// Opción 3: Con InputAction.CallbackContext (control completo)
void OnToggleInventory(InputAction.CallbackContext context)
{
    if (context.performed) { /* ... */ }
}
```

**PlayerController usa Opción 2** (`InputValue`), así que es consistente usar esa cuando necesites valores.

Para acciones tipo Button (como ToggleInventory), **Opción 1 es suficiente**.

---

## ✅ Checklist Final

- [x] Eliminado `InputSystem_Actions` instanciación
- [x] Eliminada interfaz `IPlayerActions`
- [x] Eliminados métodos vacíos (OnMove, OnJump, etc.)
- [x] Simplificado a solo `OnToggleInventory()`
- [x] Patrón consistente con PlayerController
- [x] Sin errores de compilación
- [x] Código limpio y simple

---

## 🎉 Estado Final

**✅ SISTEMA COMPLETAMENTE CORRECTO**

- ✓ Usa Unity Messages automáticos
- ✓ Patrón idéntico a PlayerController
- ✓ Sin código innecesario
- ✓ Simple, limpio, funcional

**¡Solo añade el script al Player y funciona!** 🚀

---

*Patrón Unity Messages - La forma correcta*
*Simple > Complejo*

