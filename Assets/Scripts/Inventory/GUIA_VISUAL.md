# 🎮 Guía Visual del Sistema de Inventario

## 📦 Estructura de Clases

```
┌─────────────────────────────────────────────────────────┐
│                    ItemData.cs                           │
│              (ScriptableObject)                          │
│  ┌─────────────────────────────────────────────────┐   │
│  │ • ItemName: string                              │   │
│  │ • Description: string                           │   │
│  │ • Icon: Sprite                                  │   │
│  │ • MaxStackSize: int                             │   │
│  │ • IsLimb: bool                                  │   │
│  └─────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│                  InventoryItem.cs                        │
│                  (Clase Serializable)                    │
│  ┌─────────────────────────────────────────────────┐   │
│  │ • Data: ItemData (referencia)                   │   │
│  │ • Quantity: int                                 │   │
│  │ + CanStack(ItemData): bool                      │   │
│  │ + AddQuantity(int): int                         │   │
│  └─────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│                    Inventory.cs                          │
│              (Lógica pura, NO MonoBehaviour)             │
│  ┌─────────────────────────────────────────────────┐   │
│  │ • Items: List<InventoryItem>                    │   │
│  │ • Limbs: List<InventoryItem>                    │   │
│  │ + AddItem(ItemData, int): bool                  │   │
│  │ + RemoveItem(ItemData, int): bool               │   │
│  │ + Contains(ItemData): bool                      │   │
│  │ + GetItemCount(ItemData): int                   │   │
│  │ + GetItem(ItemData): InventoryItem              │   │
│  │ + Clear()                                       │   │
│  └─────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│                 PlayerInventory.cs                       │
│                   (MonoBehaviour)                        │
│  ┌─────────────────────────────────────────────────┐   │
│  │ • Inventory: Inventory                          │   │
│  │ • onItemAdded: UnityEvent                       │   │
│  │ • onItemRemoved: UnityEvent                     │   │
│  │ + AddItem(ItemData, int): bool                  │   │
│  │ + RemoveItem(ItemData, int): bool               │   │
│  │ + Contains(ItemData): bool                      │   │
│  │ + GetItemCount(ItemData): int                   │   │
│  │ + GetItem(ItemData): InventoryItem              │   │
│  │ + ClearInventory()                              │   │
│  └─────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│                   InventoryUI.cs                         │
│              (MonoBehaviour - Presentación)              │
│  ┌─────────────────────────────────────────────────┐   │
│  │ + OpenInventory()                               │   │
│  │ + CloseInventory()                              │   │
│  │ + ToggleInventory()                             │   │
│  │ + RefreshUI()                                   │   │
│  │ + ShowTooltip(InventoryItem, Vector2)          │   │
│  │ + HideTooltip()                                 │   │
│  └─────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│              InventoryUIRegistry.cs                      │
│                   (Static Class)                         │
│  ┌─────────────────────────────────────────────────┐   │
│  │ + Register(InventoryUI)                         │   │
│  │ + Unregister(InventoryUI)                       │   │
│  │ + Get(): InventoryUI                            │   │
│  │ + GetActiveUI(): InventoryUI                    │   │
│  └─────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────┘
```

---

## 🔄 Flujo de Uso Típico

```
┌─────────────────────────────────────────────────────────┐
│                    INICIO DEL JUEGO                      │
└─────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│  1. Crear ItemData (ScriptableObjects)                  │
│     • Poción de Salud                                   │
│     • Espada                                            │
│     • Llave                                             │
│     etc...                                              │
└─────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│  2. Configurar PlayerInventory en el jugador             │
│     • maxCapacity = 20                                  │
│     • maxLimbCapacity = 4                               │
└─────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│  3. Configurar InventoryUI en el Canvas                 │
│     • Asignar referencias                               │
│     • Configurar slots                                  │
└─────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│              DURANTE EL GAMEPLAY                         │
└─────────────────────────────────────────────────────────┘
                          ↓
       ┌──────────────────┴──────────────────┐
       ↓                                      ↓
┌─────────────────┐              ┌─────────────────────┐
│  Recolectar     │              │  Usar Items         │
│  Items          │              │                     │
│                 │              │                     │
│ AddItem() ────→ │              │ ← RemoveItem()      │
│                 │              │                     │
└─────────────────┘              └─────────────────────┘
       ↓                                      ↓
┌─────────────────────────────────────────────────────────┐
│                    VERIFICACIONES                        │
│                                                          │
│  • Contains(item) - ¿Tiene el item?                     │
│  • GetItemCount(item) - ¿Cuántos tiene?                 │
│  • GetItem(item) - ¿Información completa?               │
└─────────────────────────────────────────────────────────┘
       ↓
┌─────────────────────────────────────────────────────────┐
│                  ACCIONES BASADAS EN                     │
│                   VERIFICACIONES                         │
│                                                          │
│  • Abrir puertas                                        │
│  • Completar quests                                     │
│  • Craftear items                                       │
│  • Activar diálogos                                     │
└─────────────────────────────────────────────────────────┘
```

---

## 🎯 Casos de Uso Visuales

### Caso 1: Verificar si Existe Item

```
┌────────────────────────────────────────────────────┐
│  PlayerInventory.Contains(itemData)                │
└────────────────────────────────────────────────────┘
                    ↓
        ┌──────────┴──────────┐
        ↓                     ↓
    ┌──────┐            ┌──────────┐
    │ true │            │  false   │
    └──────┘            └──────────┘
        ↓                     ↓
┌──────────────┐      ┌─────────────────┐
│ TIENE EL     │      │ NO TIENE EL     │
│ ITEM         │      │ ITEM            │
│              │      │                 │
│ • Abrir      │      │ • Mostrar msg   │
│   puerta     │      │   "Necesitas    │
│ • Completar  │      │   llave"        │
│   quest      │      │ • Bloquear      │
│ • Activar    │      │   acceso        │
│   diálogo    │      │                 │
└──────────────┘      └─────────────────┘
```

### Caso 2: Añadir Item

```
┌────────────────────────────────────────────────────┐
│  PlayerInventory.AddItem(itemData, quantity)       │
└────────────────────────────────────────────────────┘
                    ↓
        ┌──────────┴──────────┐
        ↓                     ↓
    ┌──────┐            ┌──────────┐
    │ true │            │  false   │
    └──────┘            └──────────┘
        ↓                     ↓
┌──────────────┐      ┌─────────────────┐
│ AÑADIDO      │      │ INVENTARIO      │
│ EXITOSAMENTE │      │ LLENO           │
│              │      │                 │
│ • Mostrar    │      │ • Mostrar msg   │
│   "+1 Item"  │      │   "Inv lleno"   │
│ • Sonido     │      │ • Sugerir       │
│ • Animación  │      │   eliminar      │
│ • Actualizar │      │   items         │
│   UI         │      │                 │
└──────────────┘      └─────────────────┘
```

### Caso 3: Sistema de Crafting

```
┌────────────────────────────────────────────────────┐
│     VERIFICAR MATERIALES                           │
│                                                    │
│  GetItemCount(madera) >= 5                         │
│  GetItemCount(hierro) >= 2                         │
└────────────────────────────────────────────────────┘
                    ↓
        ┌──────────┴──────────┐
        ↓                     ↓
┌──────────────┐      ┌─────────────────┐
│ SUFICIENTES  │      │ INSUFICIENTES   │
│ MATERIALES   │      │ MATERIALES      │
└──────────────┘      └─────────────────┘
        ↓                     ↓
┌──────────────┐      ┌─────────────────┐
│ 1. Remover   │      │ • Mostrar msg   │
│    madera(5) │      │   "Necesitas    │
│    hierro(2) │      │   más X"        │
│              │      │ • Marcar en     │
│ 2. Añadir    │      │   UI los        │
│    espada(1) │      │   faltantes     │
│              │      │                 │
│ 3. Mostrar   │      └─────────────────┘
│    "Crafted!"│      
└──────────────┘      
```

---

## 📊 Tabla de Métodos Rápidos

| **Método** | **Retorna** | **Cuándo Usar** |
|------------|-------------|-----------------|
| `Contains(item)` | `bool` | ¿Tiene el item? |
| `GetItemCount(item)` | `int` | ¿Cuántos tiene? |
| `AddItem(item, qty)` | `bool` | Recolectar items |
| `RemoveItem(item, qty)` | `bool` | Consumir/usar items |
| `GetItem(item)` | `InventoryItem` | Info detallada |
| `ClearInventory()` | `void` | Limpiar todo |

---

## 🔑 Atajos de Código

### Obtener Referencia al Inventario
```csharp
PlayerInventory inv = FindFirstObjectByType<PlayerInventory>();
```

### Verificar Item
```csharp
if (inv.Contains(myItem)) { /* hacer algo */ }
```

### Contar Items
```csharp
int count = inv.GetItemCount(myItem);
```

### Añadir Item
```csharp
bool success = inv.AddItem(myItem, 1);
```

### Eliminar Item
```csharp
bool removed = inv.RemoveItem(myItem, 1);
```

### Obtener UI
```csharp
var ui = InventoryUIRegistry.GetActiveUI();
```

### Abrir/Cerrar Inventario
```csharp
ui.OpenInventory();
ui.CloseInventory();
ui.ToggleInventory();
```

---

## 🎨 Ejemplo Completo Visual

```
┌─────────────────────────────────────────────────────────┐
│                JUGADOR SE ACERCA A PUERTA                │
└─────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│              JUGADOR INTERACTÚA (E)                      │
└─────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────┐
│      Script Door: TryOpen()                              │
│      playerInventory.Contains(llave)                     │
└─────────────────────────────────────────────────────────┘
                          ↓
              ┌──────────┴──────────┐
              ↓                     ↓
        ┌─────────┐           ┌──────────┐
        │  TRUE   │           │  FALSE   │
        └─────────┘           └──────────┘
              ↓                     ↓
┌─────────────────────────┐  ┌──────────────────────┐
│  • Reproducir sonido    │  │  • ShowMessage()     │
│    de puerta            │  │    "Necesitas llave" │
│  • Animación abrir      │  │  • Sonido error      │
│  • playerInventory      │  │  • Puerta no abre    │
│    .RemoveItem(llave)   │  │                      │
│    (si consume)         │  │                      │
│  • Puerta abierta       │  │                      │
└─────────────────────────┘  └──────────────────────┘
```

---

## 🎯 Checklist de Implementación

```
┌─────────────────────────────────────────────┐
│  CONFIGURACIÓN INICIAL                      │
└─────────────────────────────────────────────┘
  ☐ Crear ItemData para cada item del juego
  ☐ Añadir PlayerInventory al jugador
  ☐ Configurar InventoryUI en el Canvas
  ☐ Asignar slots y referencias en Inspector

┌─────────────────────────────────────────────┐
│  IMPLEMENTACIÓN DE GAMEPLAY                 │
└─────────────────────────────────────────────┘
  ☐ Scripts para recolección de items
  ☐ Scripts para verificación de items
  ☐ Sistema de puertas/puzzles
  ☐ Sistema de crafting (si aplica)
  ☐ Sistema de quests (si aplica)
  ☐ Sistema de tienda (si aplica)

┌─────────────────────────────────────────────┐
│  PRUEBAS                                    │
└─────────────────────────────────────────────┘
  ☐ Probar añadir items
  ☐ Probar eliminar items
  ☐ Probar verificaciones
  ☐ Probar inventario lleno
  ☐ Probar UI (abrir/cerrar)
  ☐ Probar tooltips
```

---

## 📁 Archivos de Referencia

1. **`RESUMEN_RAPIDO.md`** ← Esta guía visual
2. **`INVENTORY_API_DOCUMENTATION.md`** ← Documentación completa
3. **`InventoryExample.cs`** ← Script de ejemplo con todos los casos de uso

---

**¡Listo para usar!** 🚀

