# Sistema de Inventario – Documentación Mejorada

Sistema de inventario modular y desacoplado para Unity.  
El objetivo principal es ofrecer una **API clara**, **estable**, **fácil de integrar con cualquier sistema** y **extensible**, manteniendo una arquitectura limpia y sin dependencias innecesarias.

---

## Índice

1. Características principales  
2. Arquitectura del sistema  
3. API Pública – PlayerInventory (lo esencial)  
4. Integración con otros sistemas  
5. Eventos  
6. Extensión del sistema  
7. Troubleshooting

---

# 1. Características principales

- Arquitectura completamente desacoplada (UI, lógica y mundo separados).  
- Dos categorías de items: **items normales** y **extremidades** (limbs).  
- Sistema de stacking configurable.  
- Eventos de inventario para reactividad.  
- Fácil integración con interactuables, quests, tiendas, crafting, etc.  
- Extensible mediante ScriptableObjects.  
- UI modular que puede reemplazarse o eliminarse sin afectar a la lógica.

---

# 2. Arquitectura del sistema

```
PlayerInventory (MonoBehaviour)
│
├── Inventory (lógica pura, sin MonoBehaviour)
│   ├── List<InventoryItem> Items
│   └── List<InventoryItem> Limbs
│
├── ItemData (ScriptableObject)
├── InventoryItem (instancia con Data y Quantity)
└── UnityEvents (onItemAdded, onItemRemoved)
    └── InventoryUI (suscriptor opcional)
```

**Responsabilidades principales:**

| Componente | Responsabilidad |
|------------|----------------|
| PlayerInventory | API pública y gestión de eventos |
| Inventory | Lógica pura del inventario |
| ItemData | Definición de items (ScriptableObject) |
| InventoryItem | Instancia de item con cantidad |
| InventoryUI | Visualización y UI |
| WorldInventoryItem | Representación física de item en el mundo |

---

# 3. API Pública – PlayerInventory

## Métodos principales

### AddItem
```csharp
public bool AddItem(ItemData itemData, int quantity = 1)
```
Añade items al inventario.  
**Retorna:** true si se añadió, false si no hay espacio.

### RemoveItem
```csharp
public bool RemoveItem(ItemData itemData, int quantity = 1)
```
Quita items del inventario.  
**Retorna:** true si se quitó, false si no había suficientes.

### Contains
```csharp
public bool Contains(ItemData itemData)
```
Verifica si el inventario contiene al menos una unidad de un item.

### GetItemCount
```csharp
public int GetItemCount(ItemData itemData)
```
Obtiene la cantidad total de un item.

### GetItem
```csharp
public InventoryItem GetItem(ItemData itemData)
```
Obtiene la instancia `InventoryItem` de un item, o null si no existe.

### ClearInventory
```csharp
public void ClearInventory()
```
Vacía completamente el inventario.

### Acceso a la lógica interna
```csharp
public Inventory Inventory { get; }
```
Permite acceso avanzado a los items, extremidades y capacidades máximas.

---

# 4. Integración con otros sistemas

El inventario puede integrarse fácilmente con:

- **Crafting:** Verificar ingredientes y consumir items.  
- **Quests:** Suscribirse a eventos `onItemAdded` para tracking.  
- **Tiendas/Comercio:** Comprar y vender items con facilidad.  
- **Doors / Locks:** Validar llaves u items requeridos para interactuar.  
- **Condiciones complejas:** Sistemas de puzzle o requisitos de items múltiples.

---

# 5. Eventos

`PlayerInventory` expone los siguientes eventos:

```csharp
public UnityEvent<ItemData, int> onItemAdded;
public UnityEvent<ItemData, int> onItemRemoved;
```

**Suscripción desde código:**
```csharp
playerInventory.onItemAdded.AddListener(OnItemAdded);
playerInventory.onItemRemoved.AddListener(OnItemRemoved);
```

Los eventos permiten sistemas reactivos, como UI personalizada, logros, notificaciones o estadísticas de jugador.

---

# 6. Extensión del sistema

- Extender `ItemData` para crear nuevos tipos de items.  
- Crear wrappers o helpers para filtrar, buscar, transferir o manipular items fácilmente.  
- El sistema permite añadir funcionalidades personalizadas sin tocar la lógica interna.

---

# 7. Troubleshooting

**Problemas comunes:**

- Inventario no se abre: verificar `PlayerInput` y acción `ToggleInventory`.  
- Items no aparecen en UI: revisar referencias en `InventoryUI`.  
- Tooltip parpadea: usar `CanvasGroup` y desactivar `Blocks Raycasts`.  
- Limbs mal ubicados: comprobar `Is Limb` y contenedores asignados.  
- Inventario lleno pero parece vacío: verificar suscripciones a eventos y llamadas a `RefreshUI()`.

---

**Notas finales:**

- Siempre usar la **API pública de PlayerInventory**.  
- Suscribirse y desuscribirse correctamente de eventos.  
- Evitar acceso directo a `Inventory` salvo para casos avanzados.  
- Compatibilidad: Unity 2021.3+, Nuevo Input System, TextMeshPro, URP/HDRP/Built-in.

