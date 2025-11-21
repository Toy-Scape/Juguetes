# 📦 GUÍA DE UI DEL INVENTARIO

Sistema completo de UI para mostrar el inventario con iconos, nombres, descripciones y cantidades.

---

## 📋 ESTRUCTURA DEL SISTEMA

### **Scripts Creados:**
1. **InventorySlotUI.cs** - Representa un slot individual
2. **InventoryUI.cs** - Manager principal de la UI del inventario

---

## 🎨 CONFIGURACIÓN EN UNITY

### **PASO 1: Crear el Prefab de Slot**

1. **Crear GameObject** `InventorySlot` en la escena
2. Añadir componentes:
   - **InventorySlotUI** (script)
   - **Button** (para hacer click)
   - **Image** (para el fondo/background)

3. **Estructura hija recomendada:**
   ```
   InventorySlot (InventorySlotUI + Button + Image background)
   ├── Icon (Image) - El icono del item
   ├── Name (TextMeshPro - Text) - Nombre del item
   └── QuantityContainer (GameObject)
       └── QuantityText (TextMeshPro - Text) - Cantidad "x5"
   ```

4. **Asignar referencias en InventorySlotUI:**
   - Icon Image → Arrastrar el hijo "Icon"
   - Name Text → Arrastrar el hijo "Name"
   - Quantity Text → Arrastrar "QuantityText"
   - Quantity Container → Arrastrar "QuantityContainer"
   - Background Image → Arrastrar el Image del mismo GameObject

5. **Configurar colores** (opcional):
   - Empty Color: Color cuando el slot está vacío
   - Filled Color: Color cuando tiene un item
   - Selected Color: Color cuando está seleccionado

6. **Guardar como Prefab** en `Assets/Prefabs/UI/InventorySlot`

---

### **PASO 2: Crear el Canvas del Inventario**

1. **Crear Canvas** `InventoryCanvas` (si no existe)
   - Render Mode: Screen Space - Overlay
   - Canvas Scaler: Scale With Screen Size (1920x1080)

2. **Estructura principal:**
   ```
   InventoryCanvas
   └── InventoryPanel (GameObject - Panel principal)
       ├── Background (Image oscura)
       ├── TitleText (TextMeshPro) - "INVENTARIO"
       ├── TabsContainer (GameObject)
       │   ├── ItemsTabButton (Button)
       │   └── LimbsTabButton (Button)
       ├── ContentContainer (GameObject)
       │   ├── ItemsTab (GameObject)
       │   │   └── ItemsGrid (GridLayoutGroup)
       │   └── LimbsTab (GameObject)
       │       └── LimbsGrid (GridLayoutGroup)
       └── DetailsPanel (GameObject)
           ├── DetailIcon (Image)
           ├── DetailName (TextMeshPro)
           ├── DetailDescription (TextMeshPro)
           └── DetailQuantity (TextMeshPro)
   ```

3. **Configurar GridLayoutGroup** (en ItemsGrid y LimbsGrid):
   - Cell Size: 100x100 (o el tamaño que prefieras)
   - Spacing: 10x10
   - Constraint: Fixed Column Count: 5 (ajustar según tu diseño)

---

### **PASO 3: Configurar InventoryUI**

1. **Añadir InventoryUI** al `InventoryPanel` (o a un manager separado)

2. **Asignar referencias:**
   - **Player Inventory**: Arrastrar el Player con PlayerInventory
   - **Inventory Panel**: El panel principal que se mostrará/ocultará
   - **Items Container**: El GridLayoutGroup de items
   - **Limbs Container**: El GridLayoutGroup de extremidades
   - **Slot Prefab**: El prefab del slot que creaste
   - **Items Tab**: El GameObject ItemsTab
   - **Limbs Tab**: El GameObject LimbsTab
   - **Items Tab Button**: El botón para cambiar a items
   - **Limbs Tab Button**: El botón para cambiar a limbs
   - **Details Panel**: El panel de detalles
   - **Detail Icon**: La imagen del icono en detalles
   - **Detail Name Text**: El texto del nombre
   - **Detail Description Text**: El texto de la descripción
   - **Detail Quantity Text**: El texto de la cantidad

3. **Configuración:**
   - Max Slots To Display: 20 (o cuántos quieras mostrar)
   - Hide On Start: ✓ (para que empiece oculto)

---

## 🎮 CONFIGURAR INPUT SYSTEM

### **Añadir acción en InputSystem_Actions**

1. Abrir `Assets/Input/InputSystem_Actions.inputactions`
2. Ir al Action Map "Player"
3. Click en el botón **+** para añadir nueva acción
4. Configurar la acción:
   - **Nombre**: `ToggleInventory`
   - **Action Type**: Button
   - **Control Type**: (dejar vacío o Button)
5. Click en la acción y añadir binding:
   - Click en **+** → **Add Binding**
   - Seleccionar **Keyboard → Tab** (o la tecla que prefieras: I, B, etc.)
6. **Guardar** (Ctrl+S)
7. Unity regenerará automáticamente el script C#

**Nota**: El PlayerInventory y el InventoryUI ya están configurados para recibir el mensaje `OnToggleInventory()` automáticamente gracias al Input System de Unity.

---

## 🔧 FUNCIONAMIENTO

### **Input**
- Presiona **Tab** (o la tecla configurada) para abrir/cerrar el inventario
- El `InventoryUI` recibe el mensaje `OnToggleInventory()` automáticamente

### **Navegación**
- Click en los botones de tabs para cambiar entre Items y Extremidades
- Click en un slot para ver sus detalles en el panel derecho

### **Actualización Automática**
- Cuando añades o quitas items, la UI se actualiza automáticamente gracias a los eventos `onItemAdded` y `onItemRemoved`

---

## 🎨 PERSONALIZACIÓN

### **Colores de Slots**
Configura en cada `InventorySlotUI`:
- **Empty Color**: Slot vacío (ej: gris oscuro transparente)
- **Filled Color**: Slot con item (ej: gris medio)
- **Selected Color**: Slot seleccionado (ej: azul claro)

### **Layout**
Ajusta el `GridLayoutGroup`:
- **Cell Size**: Tamaño de cada slot
- **Spacing**: Espacio entre slots
- **Constraint**: Número de columnas

### **Estilos de Texto**
Personaliza los TextMeshPro:
- Fuente, tamaño, color, alineación
- Outline, sombra, efectos

---

## 💡 EJEMPLO DE USO

### **Abrir/Cerrar desde código:**
```csharp
// Desde cualquier script
InventoryUI inventoryUI = FindObjectOfType<InventoryUI>();
inventoryUI.OpenInventory();
inventoryUI.CloseInventory();
inventoryUI.ToggleInventory();
```

### **Refrescar manualmente:**
```csharp
inventoryUI.RefreshUI();
```

---

## 📊 FLUJO DE DATOS

```
ItemData (ScriptableObject)
    ↓
InventoryItem (en Inventory)
    ↓
InventorySlotUI (muestra el item)
    ↓
InventoryUI (controla todo)
```

---

## ✅ CHECKLIST DE CONFIGURACIÓN

- [ ] Prefab de InventorySlot creado y configurado
- [ ] Canvas con estructura de UI creada
- [ ] GridLayoutGroups configurados
- [ ] InventoryUI añadido con todas las referencias asignadas
- [ ] Input Action "ToggleInventory" añadida en PlayerInput
- [ ] PlayerInventory en el Player
- [ ] Probado: Abrir inventario con Tab
- [ ] Probado: Añadir items y ver que aparecen en UI
- [ ] Probado: Click en slots para ver detalles
- [ ] Probado: Cambiar entre tabs

---

## 🎯 CARACTERÍSTICAS

✅ Muestra nombre, descripción, cantidad e icono de cada item
✅ Tabs separadas para items normales y extremidades
✅ Panel de detalles al seleccionar un item
✅ Se actualiza automáticamente al añadir/quitar items
✅ Sistema de input integrado con Unity Input System
✅ Diseño modular y fácil de personalizar
✅ Clean Code y arquitectura desacoplada

---

## 🐛 TROUBLESHOOTING

**No se abre el inventario al presionar Tab:**
- Verifica que la acción "ToggleInventory" exista en PlayerInput
- Asegúrate de que el Input System esté configurado correctamente
- Comprueba que el GameObject con InventoryUI esté activo

**Los items no aparecen en la UI:**
- Verifica que las referencias en InventoryUI estén asignadas
- Comprueba que el Slot Prefab tenga el componente InventorySlotUI
- Asegúrate de que los ItemData tengan iconos asignados

**Los detalles no se muestran:**
- Verifica que el Details Panel esté asignado
- Asegúrate de que los TextMeshPro estén configurados
- Comprueba que el slot tenga el componente Button

**El inventario aparece visible aunque "Hide On Start" esté marcado:**
- ✅ SOLUCIONADO: Ahora el panel se oculta correctamente en Start()
- El estado inicial siempre es cerrado (isInventoryOpen = false)

**La UI no se actualiza al añadir items:**
- ✅ SOLUCIONADO: La UI se refresca automáticamente cada vez que añades o quitas items
- Ya no necesitas tener el inventario abierto para que se actualice

---

¡Listo! Tu sistema de UI de inventario está completo y funcionando. 🎮

