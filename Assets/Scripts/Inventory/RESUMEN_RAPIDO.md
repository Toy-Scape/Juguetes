# 📋 Resumen Rápido - API de Inventario

## ¿Qué he creado?

He documentado completamente la API pública del sistema de inventario y creado un script de ejemplo completo con múltiples casos de uso.

### Archivos Creados:

1. **`InventoryExample.cs`** - Script de ejemplo con casos de uso prácticos
2. **`INVENTORY_API_DOCUMENTATION.md`** - Documentación completa de la API
3. **Documentación XML en `InventoryUI.cs`** - Comments IntelliSense para todos los métodos públicos

---

## 🚀 Inicio Rápido

### Para usar el script de ejemplo:

1. **Crear un GameObject vacío** en tu escena (Ej: "InventoryTester")
2. **Adjuntar el script** `InventoryExample.cs`
3. **Asignar los ItemData** desde el Inspector:
   - `Item To Check` - El item que quieres verificar
   - `Item To Add` - El item que quieres añadir
   - `Quantity To Add` - Cantidad a añadir

4. **Ejecutar el juego** - Si `Run Tests On Start` está activado, se ejecutarán pruebas automáticamente

### Métodos disponibles en el script de ejemplo:

Puedes ejecutar estos métodos desde el menú contextual (clic derecho en el componente):

- ✓ **Ejecutar Pruebas de Inventario** - Ejecuta todas las pruebas
- ✓ **Verificar si Existe Item** - Comprueba si tienes un item
- ✓ **Contar Cantidad de Item** - Cuenta cuántos items tienes
- ✓ **Añadir Item de Prueba** - Añade items al inventario
- ✓ **Eliminar Item de Prueba** - Elimina items del inventario
- ✓ **Obtener Información del Item** - Muestra detalles completos
- ✓ **Abrir Inventario** - Abre la UI del inventario
- ✓ **Cerrar Inventario** - Cierra la UI del inventario
- ✓ **Alternar Inventario** - Alterna entre abierto/cerrado
- ✓ **Intentar Craftear Item** - Prueba el sistema de crafting
- ✓ **Verificar Quest Completada** - Comprueba si tienes todos los items de una quest

---

## 📚 Métodos Principales del Inventario

### PlayerInventory - Métodos Más Usados

```csharp
// Obtener referencia
PlayerInventory playerInv = FindFirstObjectByType<PlayerInventory>();

// Verificar si tiene un item
bool hasKey = playerInv.Contains(keyItemData);

// Contar cantidad
int goldCount = playerInv.GetItemCount(goldItemData);

// Añadir item
bool success = playerInv.AddItem(itemData, quantity);

// Eliminar item
bool removed = playerInv.RemoveItem(itemData, quantity);

// Obtener información del item
InventoryItem item = playerInv.GetItem(itemData);
```

### InventoryUI - Control de UI

```csharp
// Obtener referencia a la UI
var inventoryUI = InventoryUIRegistry.GetActiveUI();

// Abrir/Cerrar
inventoryUI.OpenInventory();
inventoryUI.CloseInventory();
inventoryUI.ToggleInventory();

// Refrescar
inventoryUI.RefreshUI();
```

---

## 💡 Ejemplos de Uso Comunes

### Ejemplo 1: Puerta con Llave

```csharp
public class Door : MonoBehaviour
{
    [SerializeField] private ItemData requiredKey;
    private PlayerInventory playerInventory;

    void Start()
    {
        playerInventory = FindFirstObjectByType<PlayerInventory>();
    }

    public void TryOpen()
    {
        if (playerInventory.Contains(requiredKey))
        {
            Debug.Log("Puerta abierta!");
            // Abrir puerta
        }
        else
        {
            Debug.Log("Necesitas una llave");
        }
    }
}
```

### Ejemplo 2: Recolectar Item del Mundo

```csharp
public class WorldItem : MonoBehaviour
{
    [SerializeField] private ItemData itemData;
    [SerializeField] private int quantity = 1;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerInventory inv = other.GetComponent<PlayerInventory>();
            
            if (inv.AddItem(itemData, quantity))
            {
                Debug.Log($"Recogido: {itemData.ItemName}");
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("Inventario lleno");
            }
        }
    }
}
```

### Ejemplo 3: Verificar Materiales para Crafting

```csharp
public void TryCraft()
{
    // Verificar materiales
    if (playerInventory.GetItemCount(wood) >= 5 &&
        playerInventory.GetItemCount(iron) >= 2)
    {
        // Remover materiales
        playerInventory.RemoveItem(wood, 5);
        playerInventory.RemoveItem(iron, 2);
        
        // Añadir resultado
        playerInventory.AddItem(sword, 1);
        
        Debug.Log("¡Espada crafteada!");
    }
    else
    {
        Debug.Log("Materiales insuficientes");
    }
}
```

---

## 🎯 Verificar si Existe un Objeto (Tu Pregunta Principal)

### Patrón Básico:

```csharp
using UnityEngine;
using Inventory;

public class CheckItemExample : MonoBehaviour
{
    [SerializeField] private ItemData itemToCheck;
    private PlayerInventory playerInventory;

    void Start()
    {
        playerInventory = FindFirstObjectByType<PlayerInventory>();
    }

    void CheckItem()
    {
        // VERIFICAR SI EXISTE
        if (playerInventory.Contains(itemToCheck))
        {
            // SÍ EXISTE - Hacer una cosa
            Debug.Log("¡El jugador TIENE el item!");
            OnHasItem();
        }
        else
        {
            // NO EXISTE - Hacer otra cosa
            Debug.Log("El jugador NO TIENE el item");
            OnDoesNotHaveItem();
        }
    }

    void OnHasItem()
    {
        // Acciones cuando SÍ tiene el item:
        // - Abrir puerta
        // - Completar quest
        // - Activar diálogo
        // - Desbloquear área
    }

    void OnDoesNotHaveItem()
    {
        // Acciones cuando NO tiene el item:
        // - Mostrar mensaje de error
        // - Dar pista
        // - Bloquear acceso
    }
}
```

### Verificar Cantidad Específica:

```csharp
void CheckQuantity()
{
    int count = playerInventory.GetItemCount(itemToCheck);
    
    if (count >= 5)
    {
        Debug.Log($"Tienes suficientes items: {count}");
        // Hacer algo
    }
    else
    {
        Debug.Log($"No tienes suficientes. Tienes: {count}, necesitas: 5");
    }
}
```

---

## 📖 Documentación Completa

Para ver la documentación completa de todos los métodos, propiedades y ejemplos avanzados:

👉 **`INVENTORY_API_DOCUMENTATION.md`**

Este archivo incluye:
- Descripción detallada de todas las clases
- Todos los métodos con parámetros y retornos
- Ejemplos de uso para cada método
- Sistemas completos (Crafting, Quests, Tiendas, etc.)
- Mejores prácticas
- Solución de problemas comunes

---

## 🔍 Cómo Usar IntelliSense

Todos los métodos públicos ahora tienen documentación XML completa. Cuando escribas código:

1. Escribe `playerInventory.`
2. IntelliSense mostrará todos los métodos con descripciones
3. Al escribir `playerInventory.AddItem(` verás los parámetros y qué hace el método

---

## ✅ Checklist de Implementación

- [x] Documentación XML en InventoryUI.cs
- [x] Script de ejemplo completo (InventoryExample.cs)
- [x] Documentación completa en Markdown
- [x] Ejemplos de verificación de items
- [x] Ejemplos de crafting
- [x] Ejemplos de quests
- [x] Ejemplos de tiendas
- [x] Ejemplos de recolección de items
- [x] Mejores prácticas y antipatrones

---

## 🎓 Próximos Pasos

1. **Revisar** el archivo `InventoryExample.cs` para ver todos los casos de uso
2. **Leer** la documentación completa en `INVENTORY_API_DOCUMENTATION.md`
3. **Probar** los métodos desde el menú contextual en Unity
4. **Implementar** tus propios sistemas usando los ejemplos como referencia

---

**¿Necesitas ayuda?** Todos los métodos tienen comentarios XML completos y ejemplos de uso en el código. ¡Explora los archivos creados! 🚀

