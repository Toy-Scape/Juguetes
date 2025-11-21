# 🎒 Sistema de Inventario - Guía Simple

## 🚀 Uso Rápido (2 Pasos)

### Paso 1: Añadir Inventario al Player

1. Selecciona tu GameObject **Player** en la jerarquía
2. **Add Component** → `PlayerInventory`
3. Configura en el Inspector:
   - **Max Capacity**: 20 (capacidad de items normales)
   - **Max Limb Capacity**: 4 (capacidad de extremidades)
   - **Show Debug Logs**: ✓ (para ver mensajes en consola)

**¡Listo!** El inventario ya funciona en el Player.

---

### Paso 2: Crear Items Recogibles

Para que cualquier objeto en el mundo sea recogible:

1. Selecciona el GameObject del objeto (ej: un cubo, una espada, etc.)
2. **Add Component** → `WorldInventoryItem`
3. Asigna el **ItemData** (ScriptableObject del item)
4. Configura:
   - **Quantity**: Cantidad a dar (default: 1)
   - **Destroy On Pickup**: ✓ (destruir al recogerlo)

**¡Listo!** El objeto ya se puede recoger con E (tu sistema IInteractable).

---

## 🎮 Controles

- **E** → Recoger item (cuando estés cerca de un WorldInventoryItem)
- **I** → Ver inventario en consola (debug)

---

## 📝 Crear ItemData

Para crear nuevos items:

1. **Click derecho** en Project
2. **Create → Inventory → Item Data**
3. Nombra el asset (ej: "Espada", "Brazo", etc.)
4. Configura en el Inspector:
   - **Item Name**: Nombre del item
   - **Description**: Descripción
   - **Icon**: Sprite del item (opcional)
   - **Max Stack Size**: Cantidad máxima apilable (1 = no apilable, 99 = muy apilable)
   - **Is Limb**: ✓ si es una extremidad (usa lista separada)

---

## 🎯 Arquitectura Simple

```
Player
└── PlayerInventory ← Maneja todo el inventario

Objeto del Mundo
└── WorldInventoryItem ← Se puede recoger con E
    └── ItemData ← ScriptableObject con info del item
```

---

## 💻 Uso desde Código (Opcional)

Si quieres usar el inventario desde otros scripts:

```csharp
using Inventory;

// Obtener el inventario del player
PlayerInventory playerInventory = player.GetComponent<PlayerInventory>();

// Añadir item
playerInventory.AddItem(itemData, 5);

// Verificar si tiene item
if (playerInventory.Contains(itemData))
{
    Debug.Log("Tiene el item!");
}

// Obtener cantidad
int count = playerInventory.GetItemCount(itemData);

// Eliminar item
playerInventory.RemoveItem(itemData, 1);
```

---

## 🎨 Eventos

`PlayerInventory` dispara eventos cuando cambia el inventario:

```csharp
PlayerInventory inventory = player.GetComponent<PlayerInventory>();

// Suscribirse a eventos
inventory.onItemAdded.AddListener(OnItemAdded);
inventory.onItemRemoved.AddListener(OnItemRemoved);

void OnItemAdded(ItemData item, int quantity)
{
    Debug.Log($"Añadido: {item.name} x{quantity}");
    // Actualizar UI, reproducir sonido, etc.
}

void OnItemRemoved(ItemData item, int quantity)
{
    Debug.Log($"Eliminado: {item.name} x{quantity}");
    // Actualizar UI
}
```

---

## ✨ Características

- ✅ Fácil de usar (solo añadir componentes)
- ✅ Se integra con tu IInteractable existente
- ✅ Usa Unity Messages (Input System automático)
- ✅ Dos categorías: Items normales y Extremidades
- ✅ Sistema de apilamiento automático
- ✅ Eventos para UI y feedback
- ✅ Sin configuración compleja

---

## 🔧 Configuración Avanzada

### Outline del Item

El script `WorldInventoryItem` añade automáticamente un componente `Outline` si no existe. Esto permite que el sistema de interacción resalte el objeto cuando el jugador lo mira.

### Gizmos

Los `WorldInventoryItem` muestran un gizmo en el editor:
- **Amarillo**: Item normal
- **Rojo**: Extremidad (limb)

---

## 🎯 Ejemplo Completo

### 1. Crear un Item de Espada

1. Create → Inventory → Item Data
2. Nombre: "Espada Legendaria"
3. Max Stack Size: 1 (no apilable)
4. Is Limb: ✗

### 2. Crear el Objeto en el Mundo

1. GameObject → 3D Object → Cube (o importa un modelo)
2. Add Component → WorldInventoryItem
3. Asigna "Espada Legendaria" en Item Data
4. El Outline se añade automáticamente

### 3. Configurar el Player

1. Selecciona tu Player
2. Add Component → PlayerInventory
3. ¡Listo!

### 4. Probar

1. Presiona Play
2. Acércate al cubo → Se resalta (outline amarillo)
3. Presiona E → Recoge la espada
4. Presiona I → Ve el inventario en consola

---

## 🚀 Próximos Pasos

El sistema backend está completo. Para añadir UI visual:

1. Crea un Canvas con UI de inventario
2. Suscríbete a los eventos `onItemAdded` y `onItemRemoved`
3. Actualiza el UI cuando cambien los eventos

**¡El sistema está listo para usar!** 🎉

