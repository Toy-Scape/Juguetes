# Arquitectura del Sistema de Inventario

## 🏛️ Principios de Diseño

### Clean Code
- **Nombres descriptivos**: Cada clase, método y variable tiene un nombre claro que describe su propósito
- **Responsabilidad única**: Cada clase tiene una única responsabilidad bien definida
- **Funciones cortas**: Los métodos son concisos y hacen una sola cosa
- **Comentarios mínimos**: El código es autoexplicativo, los comentarios solo documentan el "por qué"

### Arquitectura Desacoplada
- **Separación de concerns**: Lógica separada de presentación
- **Inyección de dependencias**: Sin referencias hardcodeadas
- **Bajo acoplamiento**: Los componentes son independientes
- **Alta cohesión**: Funcionalidades relacionadas están juntas

## 📊 Diagrama de Componentes

```
┌─────────────────────────────────────────────────────────┐
│                    UNITY SCENE                          │
│                                                         │
│  ┌──────────────┐         ┌──────────────────┐        │
│  │   Player     │         │   ItemPickup     │        │
│  │              │         │  (GameObject)    │        │
│  │  Components: │         │                  │        │
│  │  - Inventory │◄────────│  - IInteractable │        │
│  │    Component │ AddItem │  - Collider      │        │
│  │  - Player    │         │  - Outline       │        │
│  │    Interactor│─────────┤                  │        │
│  └──────────────┘ Interact└──────────────────┘        │
│                                                         │
└─────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│                  CÓDIGO C# (LÓGICA)                     │
│                                                         │
│  ┌──────────────────────────────────────┐              │
│  │    InventoryComponent                │              │
│  │    (MonoBehaviour)                   │              │
│  │  ┌────────────────────────────────┐  │              │
│  │  │  + AddItem()                   │  │              │
│  │  │  + RemoveItem()                │  │              │
│  │  │  + Contains()                  │  │              │
│  │  │  + GetItem()                   │  │              │
│  │  │  + DropItem()                  │  │              │
│  │  │                                │  │              │
│  │  │  Events:                       │  │              │
│  │  │  - onItemAdded                 │  │              │
│  │  │  - onItemRemoved               │  │              │
│  │  └────────────────────────────────┘  │              │
│  │              │                        │              │
│  │              │ owns                   │              │
│  │              ▼                        │              │
│  │  ┌────────────────────────────────┐  │              │
│  │  │    Inventory                   │  │              │
│  │  │    (Pure C# Class)             │  │              │
│  │  │  ┌──────────────────────────┐  │  │              │
│  │  │  │ - items: List            │  │  │              │
│  │  │  │ - limbs: List            │  │  │              │
│  │  │  │ - maxCapacity            │  │  │              │
│  │  │  │ - maxLimbCapacity        │  │  │              │
│  │  │  └──────────────────────────┘  │  │              │
│  │  │              │                  │  │              │
│  │  │              │ contains         │  │              │
│  │  │              ▼                  │  │              │
│  │  │  ┌──────────────────────────┐  │  │              │
│  │  │  │  InventoryItem           │  │  │              │
│  │  │  │  - data: ItemData        │  │  │              │
│  │  │  │  - quantity: int         │  │  │              │
│  │  │  └──────────────────────────┘  │  │              │
│  │  │              │                  │  │              │
│  │  │              │ references       │  │              │
│  │  │              ▼                  │  │              │
│  │  │  ┌──────────────────────────┐  │  │              │
│  │  │  │  ItemData                │  │  │              │
│  │  │  │  (ScriptableObject)      │  │  │              │
│  │  │  │  - itemName              │  │  │              │
│  │  │  │  - description           │  │  │              │
│  │  │  │  - icon                  │  │  │              │
│  │  │  │  - maxStackSize          │  │  │              │
│  │  │  │  - isLimb                │  │  │              │
│  │  │  └──────────────────────────┘  │  │              │
│  │  └────────────────────────────────┘  │              │
│  └──────────────────────────────────────┘              │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

## 🔍 Responsabilidades de Cada Componente

### ItemData (ScriptableObject)
**Responsabilidad**: Definir las propiedades inmutables de un tipo de ítem.

**Por qué ScriptableObject**:
- ✅ Fácil de crear en el editor
- ✅ Se puede compartir entre múltiples instancias
- ✅ Cambios en el asset se reflejan en todos los lugares que lo usen
- ✅ Eficiente en memoria (una instancia, múltiples referencias)

**No hace**:
- ❌ No guarda estado mutable (cantidad, posición, etc.)
- ❌ No contiene lógica de juego

### InventoryItem (Clase)
**Responsabilidad**: Representar una instancia de un ítem con su cantidad.

**Por qué una clase simple**:
- ✅ Combina ItemData (inmutable) con cantidad (mutable)
- ✅ Serializable para guardar/cargar partidas
- ✅ Ligera y eficiente

**No hace**:
- ❌ No conoce al inventario que la contiene
- ❌ No tiene lógica de UI

### Inventory (Clase pura)
**Responsabilidad**: Toda la lógica del inventario sin dependencias de Unity.

**Por qué NO es MonoBehaviour**:
- ✅ **Testeable**: Se puede testear sin Unity
- ✅ **Reutilizable**: Se puede usar en otros contextos (NPCs, cofres, etc.)
- ✅ **Serializable**: Fácil de guardar/cargar
- ✅ **Portable**: Se podría usar en servidor/cliente

**Ventajas**:
```csharp
// Se puede testear sin Unity
[Test]
public void AddItem_WhenInventoryFull_ReturnsFalse()
{
    var inventory = new Inventory(maxCapacity: 1, maxLimbCapacity: 1);
    var item1 = CreateTestItem();
    var item2 = CreateTestItem();
    
    inventory.AddItem(item1, 1);
    bool result = inventory.AddItem(item2, 1);
    
    Assert.IsFalse(result);
}
```

**No hace**:
- ❌ No usa componentes de Unity (Transform, GameObject, etc.)
- ❌ No dispara eventos de Unity
- ❌ No dibuja en pantalla

### InventoryComponent (MonoBehaviour)
**Responsabilidad**: Actuar como puente entre Unity y la lógica del inventario.

**Por qué es el único MonoBehaviour**:
- ✅ Necesita estar en un GameObject para funcionar en Unity
- ✅ Expone eventos de UnityEvent para el Inspector
- ✅ Puede ser encontrado con GetComponent
- ✅ Se serializa en escenas de Unity

**Patrón Facade**:
```csharp
// Simplifica el acceso al inventario interno
public bool AddItem(ItemData itemData, int quantity = 1)
{
    // Validaciones
    // Llamada al inventario
    // Eventos
    // Logging
}
```

**No hace**:
- ❌ No contiene la lógica del inventario (la delega)
- ❌ No implementa algoritmos complejos

### ItemPickup (MonoBehaviour + IInteractable)
**Responsabilidad**: Permitir que un GameObject sea recogible.

**Por qué implementa IInteractable**:
- ✅ Se integra con el sistema de interacción existente
- ✅ Polimorfismo: PlayerInteractor no necesita saber que es un ItemPickup

**Patrón Strategy**:
```csharp
// PlayerInteractor trata todo como IInteractable
IInteractable interactable = hitObject.GetComponent<IInteractable>();
interactable.Interact(); // Puede ser ItemPickup, Door, Button, etc.
```

**No hace**:
- ❌ No contiene referencias directas al Player
- ❌ No modifica directamente el inventario (usa InventoryComponent)

## 🎯 Patrones de Diseño Aplicados

### 1. **Separation of Concerns**
```
Presentación (MonoBehaviour) → Lógica (Pure C#) → Datos (ScriptableObject)
```

### 2. **Facade Pattern**
`InventoryComponent` simplifica el acceso a `Inventory` interno.

### 3. **Strategy Pattern**
`IInteractable` permite diferentes comportamientos de interacción.

### 4. **Observer Pattern**
Eventos `onItemAdded` y `onItemRemoved` para notificaciones.

### 5. **Single Responsibility Principle (SRP)**
Cada clase tiene una única razón para cambiar.

### 6. **Open/Closed Principle**
Extender `ItemData` sin modificar código existente:
```csharp
public class WeaponData : ItemData { }
```

### 7. **Dependency Inversion**
`ItemPickup` depende de la abstracción `InventoryComponent`, no de implementaciones concretas.

## 🔄 Flujo de Datos

### Recoger un Item
```
1. Player presiona E
   ↓
2. PlayerInteractor detecta ItemPickup (IInteractable)
   ↓
3. Llama a ItemPickup.Interact()
   ↓
4. ItemPickup busca InventoryComponent en el Player
   ↓
5. Llama a InventoryComponent.AddItem(itemData, quantity)
   ↓
6. InventoryComponent valida y llama a inventory.AddItem()
   ↓
7. Inventory añade el item a la lista correspondiente
   ↓
8. InventoryComponent dispara evento onItemAdded
   ↓
9. UI y otros sistemas reaccionan al evento
   ↓
10. ItemPickup destruye el GameObject (si destroyOnPickup)
```

### Usar un Item
```
1. Player presiona tecla de uso (ej: H)
   ↓
2. Script del Player llama a inventory.GetItem(itemData)
   ↓
3. Verifica que existe y tiene cantidad > 0
   ↓
4. Aplica efecto del item (curación, etc.)
   ↓
5. Llama a inventory.RemoveItem(itemData, 1)
   ↓
6. Se dispara evento onItemRemoved
   ↓
7. UI se actualiza automáticamente
```

## 📈 Escalabilidad

### Añadir Nuevos Tipos de Items
```csharp
// Crear un nuevo ScriptableObject derivado
[CreateAssetMenu(menuName = "Inventory/Consumable")]
public class ConsumableData : ItemData
{
    public float healAmount;
    public float duration;
}
```

### Añadir Nuevos Comportamientos de Pickup
```csharp
public class DelayedPickup : MonoBehaviour, IInteractable
{
    public float pickupDelay = 2f;
    
    public void Interact()
    {
        StartCoroutine(PickupAfterDelay());
    }
}
```

### Múltiples Inventarios
```csharp
// Cada entidad puede tener su propio inventario
public class NPCController : MonoBehaviour
{
    [SerializeField] private InventoryComponent inventory;
    
    public void Trade(ItemData item, int quantity)
    {
        // Lógica de comercio
    }
}
```

## 🧪 Testabilidad

### Ventajas del diseño actual:
1. **Inventory** es pura lógica → Fácil de testear
2. **InventoryItem** es una clase simple → Fácil de mockear
3. **ItemData** es ScriptableObject → Se puede crear en tests
4. **InventoryComponent** es delgado → Solo testear integración

### Ejemplo de Test
```csharp
[TestFixture]
public class InventoryTests
{
    [Test]
    public void AddItem_StacksCorrectly()
    {
        // Arrange
        var inventory = new Inventory();
        var item = CreateTestItemData(maxStack: 10);
        
        // Act
        inventory.AddItem(item, 5);
        inventory.AddItem(item, 3);
        
        // Assert
        Assert.AreEqual(8, inventory.GetItemCount(item));
    }
}
```

## 🔒 Encapsulación

### Propiedades de solo lectura
```csharp
public IReadOnlyList<InventoryItem> Items => items;
```
Evita modificaciones externas accidentales.

### Validaciones internas
```csharp
public bool AddItem(ItemData itemData, int quantity)
{
    if (itemData == null || quantity <= 0)
        return false;
    // ...
}
```
El inventario se protege de datos inválidos.

## 🎨 Extensibilidad Futura

### Fácil añadir:
- ✅ Sistema de crafteo
- ✅ Comercio entre NPCs
- ✅ Cofres y almacenamiento
- ✅ Items equipables
- ✅ Sistema de durabilidad
- ✅ Items únicos vs stackeables
- ✅ Peso y límites
- ✅ Categorías de items
- ✅ Filtros y búsqueda

### Sin romper código existente:
Todo se puede añadir extendiendo las clases base o creando nuevos componentes que usen el sistema actual.

---

**Conclusión**: Este diseño maximiza la flexibilidad, testabilidad y mantenibilidad siguiendo principios SOLID y Clean Code, mientras se mantiene simple y fácil de usar.

