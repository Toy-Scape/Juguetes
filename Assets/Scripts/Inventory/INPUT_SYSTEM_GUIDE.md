# 🎮 Guía del Input System - Sistema de Inventario

## ✅ Sistema Actualizado al Nuevo Input System

El sistema de inventario ahora usa el **nuevo Input System de Unity** en lugar del Input Manager antiguo.

---

## 🔧 Configuración

### 1. Acción Añadida al InputActions

Se ha añadido una nueva acción al archivo `InputSystem_Actions.inputactions`:

**Acción: `ToggleInventory`**
- **Tipo**: Button
- **Keyboard**: Tecla `I`
- **Gamepad**: Button West (X en Xbox, Square en PlayStation)

### 2. Scripts Actualizados

✅ **PlayerInventoryIntegration.cs**
- Usa `InputAction` en lugar de `Input.GetKeyDown()`
- Auto-encuentra la acción del InputActionAsset
- Gestiona Enable/Disable correctamente

✅ **InventoryUsageExample.cs**
- Actualizado para usar Input System
- Tecla `I` usa el nuevo sistema
- Teclas numéricas (1-5) mantienen Input Manager como fallback

---

## 📋 Uso en Unity

### Paso 1: Asignar InputActionAsset

Cuando añadas `PlayerInventoryIntegration` o `InventoryUsageExample` al Player:

1. En el Inspector, verás un campo **"Input Actions"**
2. Arrastra el asset `InputSystem_Actions` desde la carpeta Assets/Input/
3. ¡Listo!

```
Player GameObject
├── InventoryComponent
└── PlayerInventoryIntegration
    ├─ Inventory Component: (auto-asignado)
    └─ Input Actions: InputSystem_Actions ← Arrastrar aquí
```

### Paso 2: Usar en Juego

**Presiona `I`** para mostrar el inventario en la consola (si Debug Mode está activo)

---

## 💻 Código de Ejemplo

### Usar en tus propios scripts

```csharp
using UnityEngine;
using UnityEngine.InputSystem;
using Inventory;

public class MiScript : MonoBehaviour
{
    [SerializeField] private InventoryComponent inventory;
    [SerializeField] private InputActionAsset inputActions;
    
    private InputAction toggleInventoryAction;

    private void Awake()
    {
        // Obtener la acción del InputActionAsset
        var playerMap = inputActions.FindActionMap("Player");
        toggleInventoryAction = playerMap.FindAction("ToggleInventory");
    }

    private void OnEnable()
    {
        // Habilitar y suscribirse
        toggleInventoryAction.Enable();
        toggleInventoryAction.performed += OnToggleInventory;
    }

    private void OnDisable()
    {
        // Desuscribirse y deshabilitar
        toggleInventoryAction.performed -= OnToggleInventory;
        toggleInventoryAction.Disable();
    }

    private void OnToggleInventory(InputAction.CallbackContext context)
    {
        // Tu lógica aquí
        Debug.Log("¡Inventario toggled!");
    }
}
```

---

## 🎯 Ventajas del Nuevo Input System

✅ **Multi-plataforma**: Funciona con keyboard, gamepad, touch, etc.
✅ **Rebinding**: Los jugadores pueden cambiar teclas
✅ **Control Schemes**: Cambia entre diferentes esquemas de control
✅ **Acciones desacopladas**: El código no depende de teclas específicas
✅ **Mejor rendimiento**: Más eficiente que el Input Manager antiguo

---

## 🔍 Estructura del Input System

```
InputSystem_Actions.inputactions
└── Player (Action Map)
    ├── Move
    ├── Look
    ├── Jump
    ├── Crouch
    ├── Sprint
    ├── Interact
    ├── Grab
    └── ToggleInventory ← Nueva acción
        ├── Keyboard: I
        └── Gamepad: Button West
```

---

## 📝 Bindings Configurados

| Acción | Keyboard | Gamepad |
|--------|----------|---------|
| **ToggleInventory** | `I` | Button West (X/Square) |

Puedes añadir más bindings editando el asset `InputSystem_Actions` en Unity:
1. Selecciona el asset en el Project
2. Abre el Input Actions Editor
3. Añade nuevos bindings para ToggleInventory

---

## 🚀 Migración Completa

### Scripts Actualizados

**PlayerInventoryIntegration.cs**
```csharp
// ANTES (Input Manager)
if (Input.GetKeyDown(KeyCode.I))
{
    ShowInventoryInfo();
}

// AHORA (Input System)
private void OnToggleInventory(InputAction.CallbackContext context)
{
    ShowInventoryInfo();
}
```

**InventoryUsageExample.cs**
- Tecla `I` → Input System
- Teclas 1-5 → Input Manager (como fallback para el ejemplo)

---

## 🛠️ Personalización

### Cambiar la Tecla

1. Abre `InputSystem_Actions.inputactions`
2. Encuentra la acción "ToggleInventory"
3. Edita el binding de Keyboard
4. Cambia `<Keyboard>/i` por la tecla deseada

### Añadir Más Acciones de Inventario

Puedes añadir acciones adicionales al InputActions:

```json
{
    "name": "UseItem",
    "type": "Button",
    "id": "...",
    "expectedControlType": "Button"
}
```

Luego úsala en tu código:

```csharp
var useItemAction = playerMap.FindAction("UseItem");
useItemAction.performed += OnUseItem;
```

---

## ⚠️ Importante

### Lifecycle del Input Action

Siempre sigue este patrón:

```csharp
private void OnEnable()
{
    action.Enable();
    action.performed += Callback;
}

private void OnDisable()
{
    action.performed -= Callback;
    action.Disable();
}
```

Esto evita memory leaks y errores cuando el objeto se desactiva.

---

## 🎮 Testing

### En el Editor
1. Presiona Play
2. Presiona `I` → Debería mostrar el inventario
3. Si tienes gamepad conectado, presiona Button West

### Debug
Activa "Debug Mode" en `InventoryComponent` para ver logs cuando presiones `I`.

---

## 📚 Referencias

- [Unity Input System Documentation](https://docs.unity3d.com/Packages/com.unity.inputsystem@latest)
- [Input Actions Asset](https://docs.unity3d.com/Packages/com.unity.inputsystem@latest/manual/ActionAssets.html)
- [Callbacks en Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@latest/manual/ActionCallbacks.html)

---

## ✨ Resumen

✅ Acción "ToggleInventory" añadida al InputActions
✅ Binding: Tecla I (keyboard) + Button West (gamepad)
✅ PlayerInventoryIntegration actualizado
✅ InventoryUsageExample actualizado
✅ Sin errores de compilación
✅ Listo para usar

**¡El sistema de inventario ahora usa el Input System moderno de Unity!** 🎉

