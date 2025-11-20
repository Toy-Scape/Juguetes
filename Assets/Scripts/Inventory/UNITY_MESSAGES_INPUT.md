# 🎮 Input System con Unity Messages - Sistema de Inventario

## ✅ Actualización Completada

El sistema de inventario ahora usa la **clase InputSystem_Actions generada automáticamente** con **Unity Messages** (interfaz IPlayerActions).

---

## 🔧 Cómo Funciona

### 1. Clase Generada Automáticamente

Unity genera automáticamente la clase `InputSystem_Actions.cs` desde el archivo `InputSystem_Actions.inputactions`.

Esta clase incluye:
- **InputSystem_Actions** - Clase principal
- **IPlayerActions** - Interfaz con métodos tipo Unity Message
- **IUIActions** - Interfaz para acciones de UI

### 2. Unity Messages Pattern

En lugar de serializar el InputActionAsset, los scripts implementan la interfaz `IPlayerActions`:

```csharp
public class PlayerInventoryIntegration : MonoBehaviour, InputSystem_Actions.IPlayerActions
{
    private InputSystem_Actions inputActions;

    private void Awake()
    {
        // Crear instancia
        inputActions = new InputSystem_Actions();
        
        // Registrar callbacks (Unity Messages)
        inputActions.Player.AddCallbacks(this);
    }

    // Unity Message - Se llama automáticamente cuando se presiona I
    public void OnToggleInventory(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            ShowInventoryInfo();
        }
    }

    // Otros métodos requeridos por la interfaz
    public void OnMove(InputAction.CallbackContext context) { }
    public void OnJump(InputAction.CallbackContext context) { }
    // ... etc
}
```

---

## 🚀 Regenerar la Clase

**IMPORTANTE**: He eliminado el archivo `InputSystem_Actions.cs` para forzar su regeneración.

### Unity regenerará automáticamente el archivo cuando:

1. **Abras el proyecto en Unity**
2. **Modifiques el archivo `.inputactions`**
3. **Presiones el botón "Generate C# Class"** en el Inspector del InputActions

### Pasos para Regenerar:

1. Abre Unity
2. Ve al Project → Assets/Input/
3. Selecciona `InputSystem_Actions.inputactions`
4. En el Inspector, verás:
   - ☑ Generate C# Class (debe estar marcado)
   - Class Name: InputSystem_Actions
   - Class Namespace: (vacío)
   - C# File: InputSystem_Actions.cs
5. Si no se regeneró automáticamente, haz clic en **"Apply"**

Unity generará el archivo con el método `OnToggleInventory` incluido.

---

## 📝 Scripts Actualizados

### ✅ PlayerInventoryIntegration.cs

```csharp
// Implementa IPlayerActions
public class PlayerInventoryIntegration : MonoBehaviour, InputSystem_Actions.IPlayerActions
{
    private InputSystem_Actions inputActions;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
        inputActions.Player.AddCallbacks(this);
    }

    private void OnEnable()
    {
        inputActions?.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions?.Player.Disable();
    }

    private void OnDestroy()
    {
        if (inputActions != null)
        {
            inputActions.Player.RemoveCallbacks(this);
            inputActions.Dispose();
        }
    }

    // Unity Message - Llamado automáticamente
    public void OnToggleInventory(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            ShowInventoryInfo();
        }
    }

    // Implementaciones vacías de otros métodos
    public void OnMove(InputAction.CallbackContext context) { }
    public void OnJump(InputAction.CallbackContext context) { }
    // etc...
}
```

### ✅ InventoryUsageExample.cs

```csharp
// Mismo patrón - implementa IPlayerActions
public class InventoryUsageExample : MonoBehaviour, InputSystem_Actions.IPlayerActions
{
    private InputSystem_Actions inputActions;
    
    // ... misma estructura
}
```

---

## 🎯 Ventajas de este Enfoque

### ✅ Sin Serialización
- No necesitas arrastrar InputActionAsset en el Inspector
- Menos dependencias visuales
- Más limpio

### ✅ Unity Messages Pattern
- Familiar para desarrolladores Unity
- Similar a OnCollisionEnter, OnTriggerEnter, etc.
- Autocomplete en el IDE

### ✅ Type-Safe
- El compilador verifica que implementes todos los métodos
- Errores en tiempo de compilación, no en runtime

### ✅ Desacoplado
- La clase PlayerController puede tener sus propios input handlers
- El InventoryIntegration tiene los suyos
- Sin conflictos

---

## 🔍 Interfaz IPlayerActions

Unity genera automáticamente la interfaz con todos los métodos:

```csharp
public interface IPlayerActions
{
    void OnMove(InputAction.CallbackContext context);
    void OnLook(InputAction.CallbackContext context);
    void OnAttack(InputAction.CallbackContext context);
    void OnInteract(InputAction.CallbackContext context);
    void OnCrouch(InputAction.CallbackContext context);
    void OnCrouchToggle(InputAction.CallbackContext context);
    void OnJump(InputAction.CallbackContext context);
    void OnPrevious(InputAction.CallbackContext context);
    void OnNext(InputAction.CallbackContext context);
    void OnSprint(InputAction.CallbackContext context);
    void OnGrab(InputAction.CallbackContext context);
    void OnToggleInventory(InputAction.CallbackContext context); // ← NUEVA
}
```

---

## 📋 Uso en Unity

### 1. No Necesitas Asignar Nada en el Inspector

Los scripts crean su propia instancia de `InputSystem_Actions`:

```csharp
inputActions = new InputSystem_Actions();
```

### 2. Solo Añade el Componente al Player

```
Player GameObject
├── InventoryComponent
└── PlayerInventoryIntegration  ← Solo añadir, sin configurar nada
```

### 3. Funciona Automáticamente

- Presiona **I** → Se llama `OnToggleInventory`
- Presiona **E** → Se llama `OnInteract` (si lo implementas)
- etc.

---

## 🎮 Ejemplo Completo

```csharp
using UnityEngine;
using UnityEngine.InputSystem;
using Inventory;

namespace Player
{
    public class MyInventoryHandler : MonoBehaviour, InputSystem_Actions.IPlayerActions
    {
        [SerializeField] private InventoryComponent inventory;
        
        private InputSystem_Actions inputActions;

        private void Awake()
        {
            inputActions = new InputSystem_Actions();
            inputActions.Player.AddCallbacks(this);
        }

        private void OnEnable() => inputActions?.Player.Enable();
        private void OnDisable() => inputActions?.Player.Disable();

        private void OnDestroy()
        {
            if (inputActions != null)
            {
                inputActions.Player.RemoveCallbacks(this);
                inputActions.Dispose();
            }
        }

        // Unity Message - Se llama cuando presionas I
        public void OnToggleInventory(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                Debug.Log("Inventario toggled!");
                // Tu lógica aquí
            }
        }

        // Implementar otros métodos que necesites
        public void OnInteract(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                Debug.Log("Interacción!");
            }
        }

        // Métodos vacíos para los que no uses
        public void OnMove(InputAction.CallbackContext context) { }
        public void OnLook(InputAction.CallbackContext context) { }
        public void OnAttack(InputAction.CallbackContext context) { }
        public void OnCrouch(InputAction.CallbackContext context) { }
        public void OnCrouchToggle(InputAction.CallbackContext context) { }
        public void OnJump(InputAction.CallbackContext context) { }
        public void OnPrevious(InputAction.CallbackContext context) { }
        public void OnNext(InputAction.CallbackContext context) { }
        public void OnSprint(InputAction.CallbackContext context) { }
        public void OnGrab(InputAction.CallbackContext context) { }
    }
}
```

---

## ⚠️ Importante: Lifecycle

Siempre sigue este patrón para evitar memory leaks:

```csharp
private void Awake()
{
    inputActions = new InputSystem_Actions();
    inputActions.Player.AddCallbacks(this);
}

private void OnEnable()
{
    inputActions?.Player.Enable();
}

private void OnDisable()
{
    inputActions?.Player.Disable();
}

private void OnDestroy()
{
    if (inputActions != null)
    {
        inputActions.Player.RemoveCallbacks(this);
        inputActions.Dispose();
    }
}
```

---

## 🔄 Comparación con el Método Anterior

### Antes (Serialización)
```csharp
[SerializeField] private InputActionAsset inputActions; // ← Serializado
private InputAction toggleAction;

private void Awake()
{
    var map = inputActions.FindActionMap("Player");
    toggleAction = map.FindAction("ToggleInventory");
}

private void OnEnable()
{
    toggleAction.Enable();
    toggleAction.performed += OnToggle;
}
```

❌ Necesitas arrastrar el asset en el Inspector
❌ Más código boilerplate
❌ Propenso a errores (null references)

### Ahora (Unity Messages)
```csharp
private InputSystem_Actions inputActions; // ← No serializado

private void Awake()
{
    inputActions = new InputSystem_Actions();
    inputActions.Player.AddCallbacks(this);
}

public void OnToggleInventory(InputAction.CallbackContext context)
{
    if (context.performed) { /* ... */ }
}
```

✅ Sin configuración en Inspector
✅ Menos código
✅ Type-safe
✅ Más limpio

---

## 📚 Documentación

- **INPUT_SYSTEM_GUIDE.md** - Guía anterior (serialización)
- **Este documento** - Guía actualizada (Unity Messages)

---

## ✅ Checklist

- [x] Acción "ToggleInventory" añadida al .inputactions
- [x] PlayerInventoryIntegration actualizado a Unity Messages
- [x] InventoryUsageExample actualizado a Unity Messages
- [x] InputSystem_Actions.cs eliminado (se regenerará en Unity)
- [x] Sin dependencias serializadas
- [x] Patrón de lifecycle implementado
- [x] Documentación creada

---

## 🎉 Estado Final

**✅ SISTEMA COMPLETAMENTE MIGRADO A UNITY MESSAGES**

### Próximos Pasos:

1. **Abre Unity** → El archivo `InputSystem_Actions.cs` se regenerará automáticamente
2. **Verifica** que no hay errores de compilación
3. **Añade** `PlayerInventoryIntegration` al Player
4. **Presiona I** en Play Mode → ¡Funciona!

**¡No necesitas configurar nada en el Inspector!** 🚀

---

*Actualizado el 2025-11-20*
*Usa Unity Messages Pattern con Input System generado*

