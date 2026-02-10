# Sistema de NPC — Documentación

Este documento describe la arquitectura, componentes y uso del sistema de NPC incluido en este proyecto Unity.

Objetivo
- Proporcionar un sistema de NPC modular, desacoplado y limpio (Clean Code, arquitectura por capas) con patrulla y persecución, sensores de visión y adaptador para NavMeshAgent.

Estructura general
- Assets/Scripts/
  - Core/ : Interfaces y utilidades comunes (sensores y adaptadores).
  - Domain/ : Lógica del dominio (estados del agente, `NpcBrain`, fábrica de estados).
  - Infrastructure/ : Implementaciones dependientes de Unity (adaptador de `NavMeshAgent`, `NpcController`, managers).
  - SO/ : ScriptableObjects (por ejemplo `PatrolRouteSO`) que contienen datos de rutas.

Principales componentes y responsabilidades

1) Core
- `IAgentState` (interface)
  - Contrato para estados del agente con `Enter()`, `Update()` y `Exit()`.

- `INavigationAgent` (interface)
  - Abstracción de un agente de navegación. Propiedades:
    - `bool PathPending { get; }`
    - `float RemainingDistance { get; }`
    - `Vector3 Velocity { get; }`
  - Método:
    - `void SetDestination(Vector3 target)`
  - Motivo: Permite desacoplar la lógica del NPC de `NavMeshAgent` y facilita testing y sustitución.

- `ITargetDetector` (interface)
  - Contrato para detectores (sensores) que pueden devolver objetivos visibles y consultar si un target es visible.
  - Métodos típicos:
    - `List<Transform> GetVisibleTargets(Transform self)`
    - `bool CanSeeTarget(Transform self, Transform target)`

- `VisionSensor` (ahora `MonoBehaviour`, implementa `ITargetDetector`)
  - Componente que se añade al GameObject del NPC.
  - Propiedades exposables en el Inspector: `viewRadius`, `viewAngle`, `detectionMask`, `obstacleMask`.
  - Implementación optimizada con `Physics.OverlapSphereNonAlloc` (buffer interno) para reducir allocations.
  - Provee `GetVisibleTargets` y `CanSeeTarget`.
  - Provee método editor-only `DrawGizmosSelected()` para dibujar el arco de visión en Scene View.

2) Domain
- `NpcBrain` (`MonoBehaviour` principal del NPC)
  - Coordina estados (patrulla, persecución) y detección.
  - Campos serializables en el Inspector: `PatrolRouteSO[] patrolRoutes`, `VisionSensor vision`, `detectionTime`, `randomRouteOnStart`.
  - Internamente usa las interfaces `ITargetDetector` y `INavigationAgent` para desacoplar dependencias concretas.
  - Usa `StateFactory` para crear instancias de estados (`PatrolState`, `ChaseState`).
  - Eventos públicos (expuestos como `UnityEvent` para el Inspector):
    - `UnityEvent<string> OnStateChanged` — disparado al cambiar de estado (pasa el nombre del estado).
    - `UnityEvent<Transform> OnTargetDetected` — disparado la primera vez que se detecta un objetivo.
    - `UnityEvent OnTargetLost` — disparado cuando se pierde el objetivo.
  - Comportamiento:
    - Inicia en un `PatrolState` (elegido aleatorio o primero según configuración).
    - Cada frame consulta el detector: si encuentra objetivos calcula el más cercano y, tras `detectionTime`, cambia a `ChaseState`.
    - Si pierde objetivos vuelve a patrullar.

- `PatrolState` / `ChaseState`
  - Implementaciones de `IAgentState`.
  - `PatrolState` recorre `PatrolRouteSO` y respeta `waitTime` en puntos.
  - `ChaseState` actualiza destino hacia el `Transform` objetivo.

- `StateFactory` (fábrica)
  - Clase estática que centraliza la creación de estados. Facilita swap de implementaciones y testeo.

3) Infrastructure
- `NavMeshAgentAdapter` (`MonoBehaviour`) — implementa `INavigationAgent` delegando a `UnityEngine.AI.NavMeshAgent`.
  - Permite que `NpcBrain` y estados trabajen con la interfaz en lugar de `NavMeshAgent` directamente.

- `NpcController`, `GameManager` (si existen)
  - Ejemplos de infraestructura que pueden usar los eventos de `NpcBrain` para animaciones, UI u otras integraciones.

ScriptableObjects (SO)
- `PatrolRouteSO` — contiene una lista de `PatrolPointData` (posiciones, waitTime, callbacks) y opción `randomPatrol`.

Cómo usar el sistema (pasos rápidos)
1) Preparación de la escena
- Asegúrate de que hay un NavMesh en la escena (baked NavMesh) si vas a usar `NavMeshAgent`.

2) Crear un NPC en la escena
- Crear un GameObject vacío o con modelo/rig.
- Añadir componente `NavMeshAgent` o asegúrate de que el adaptador `NavMeshAgentAdapter` se añadirá en runtime.
- Añadir componente `VisionSensor` al mismo GameObject (Add Component -> VisionSensor). Configura:
  - `viewRadius` (p.ej. 10)
  - `viewAngle` (p.ej. 90)
  - `detectionMask` (LayerMask con las capas de objetivos — por ejemplo `Player`) 
  - `obstacleMask` (LayerMask con los colliders que bloquean la visión)
- Añadir componente `NpcBrain` al GameObject.
  - En `patrolRoutes` arrastra uno o varios `PatrolRouteSO` (ScriptableObjects)
  - Ajusta `detectionTime` y `randomRouteOnStart` según desees.

3) Probar en Play
- Selecciona el NPC y observa en Scene View el arco de visión cuando está seleccionado.
- Ejecuta la escena y observa: el NPC debe patrullar. Si un objetivo entra en visión y no está bloqueado, tras `detectionTime` cambiará a chase.
- Usa la consola o subscribirte a eventos para depurar transiciones.

Ejemplo: suscribirse a eventos desde un script (pequeño Snippet)
```csharp
using UnityEngine;
using Domain;

public class NpcEventLogger : MonoBehaviour
{
    void Start()
    {
        var brain = GetComponent<NpcBrain>();
        if (brain == null) return;

        // Con UnityEvent se usan AddListener / RemoveListener
        brain.onStateChanged.AddListener(stateName => Debug.Log($"State changed to: {stateName}"));
        brain.onTargetDetected.AddListener(t => Debug.Log($"Target detected: {t.name}"));
        brain.onTargetLost.AddListener(() => Debug.Log("Target lost"));
     }
 }
 ```

Notas importantes y buenas prácticas
- `VisionSensor` ahora es `MonoBehaviour`. Añádelo como componente al NPC y configura las máscaras en el Inspector.
- `NpcBrain` mantiene una referencia serializada a `VisionSensor` sólo para authoring; en runtime trabaja con `ITargetDetector` (si `vision` no está asignado intentará `GetComponent<ITargetDetector>()`).
- Usa `StateFactory` para obtener estados; si quieres modificar la lógica de creación (por ejemplo inyectar dependencias), actualiza la fábrica.
- Los eventos ya están expuestos como `UnityEvent` para que puedas asignar callbacks desde el Inspector. Si prefieres suscribirte por código usa `AddListener` / `RemoveListener` como en el ejemplo.

Testing y verificación
- Verificación estática: el proyecto actualmente pasa las comprobaciones internas (no hay errores en los scripts relevantes después de los refactors).
- Prueba en escena: crea un NPC con un `PatrolRouteSO` y mueve un objeto del layer configurado en `detectionMask` para verificar detección y persecución.

Extensiones recomendadas (próximos pasos)
- Añadir más detectores (p.ej. `HearingSensor`) implementando `ITargetDetector`.
- Implementar `StateFactory` con inyección para tests y configuraciones desde `ScriptableObject`.
- Exponer eventos como `UnityEvent` para uso desde el Inspector.
- Escribir tests unitarios para `VisionSensor` (CanSeeTarget) y para la lógica de selección de objetivo en `NpcBrain` (usando mocks de `ITargetDetector`).

Glosario rápido
- NPC: Non-player character (personaje controlado por el sistema).
- Detector: Componente que identifica posibles objetivos (visibles) para el NPC.
- Estado (State): Comportamiento actual del NPC (Patrol, Chase, etc.).

Registro de cambios (refactor actual)
- Se refactorizó el sistema para seguir principios de Clean Code:
  - Interfaces para desacoplar dependencias (`INavigationAgent`, `ITargetDetector`).
  - `VisionSensor` migrado a `MonoBehaviour` y optimizado para reducir allocations.
  - `StateFactory` introducida para centralizar creación de estados.
  - `NpcBrain` ahora expone eventos y consume interfaces.

Si quieres, puedo:
- Convertir los eventos `Action` en `UnityEvent` para exponerlos en el inspector.
- Añadir un ejemplo de `NpcEventLogger` al proyecto y enlazarlo automáticamente a un NPC de ejemplo.
- Implementar tests unitarios básicos.

---

Fin de la documentación.
