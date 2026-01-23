# Sistema de Cinemáticas (Data-Driven Cinematic System)

Este sistema permite crear y reproducir cinemáticas de forma modular y orientada a datos, utilizando `ScriptableObjects` para definir secuencias de acciones.

## 1. Arquitectura

El sistema se divide en tres capas:
- **Core**: Define las interfaces y los datos (`CinematicAsset`, `CinematicAction`).
- **Infrastructure**: Implementaciones concretas (ej. `CinemachineCameraController` para controlar la cámara, `SceneReferenceResolver` para encontrar objetos).
- **Application**: Lógica de ejecución (`CinematicPlayer`).

## 2. Configuración de la Escena

Para usar el sistema en una escena, necesitas configurar un "Cinematic Manager":

1.  Crea un **Empty GameObject** llamado `CinematicManager` (o similar).
2.  Añade los siguientes componentes:
    *   `CinematicPlayer`: El cerebro que reproduce la cinemática.
    *   `SceneReferenceResolver`: Permite mapear IDs (strings) a objetos reales de la escena (`Transform`).
    *   `CinemachineCameraController`: Controla la cámara usando Cinemachine.
3.  **Configuración del Camera Controller**:
    *   El `CinemachineCameraController` necesita una referencia a una **Cinemachine Camera** (Virtual Camera) dedicada para cinemáticas.
    *   Crea una Virtual Camera en la escena (Unity.Cinemachine) y asígnala al campo `Cinematic Camera` del script.
    *   Esta cámara debe tener prioridad 0 inicialmente; el sistema subirá su prioridad automáticamente al reproducir.
4.  **Vincular Referencias**:
    *   Asegúrate de que el `CinematicPlayer` tenga asignados el `Camera Controller Obj` y `Resolver Obj` (normalmente es el mismo GameObject).

## 3. Resolución de Referencias (SceneReferenceResolver)

Las acciones NO guardan referencias directas a objetos de la escena (para que sean reutilizables). En su lugar, usan **IDs** (texto).

1.  En el componente `SceneReferenceResolver`:
2.  Añade elementos a la lista `References`.
3.  Asigna un **ID** (ej: "Player", "BossSpawn", "CameraPos1") y arrastra el **Transform** correspondiente de la escena.
4.  Ahora, en tus acciones, solo necesitas escribir ese ID.

## 4. Creación de Cinemáticas

Las cinemáticas son archivos `.asset` que contienen una lista de acciones.

1.  **Crear Acciones**:
    *   Click derecho en Project -> `Create` -> `Cinematic System` -> `Actions`.
    *   Elige el tipo de acción (ej: `Move Camera`, `Wait`, `Look At`).
    *   Configura sus parámetros (ej: `Target Id` = "CameraPos1", `Duration` = 2).
2.  **Crear la Cinemática**:
    *   Click derecho en Project -> `Create` -> `Cinematic System` -> `Cinematic Asset`.
    *   Selecciona el asset creado.
    *   En la lista `Actions`, añade las acciones que creaste en el paso anterior.
    *   Puedes reordenarlas arrastrando.

## 5. Reproducir una Cinemática

Para disparar la cinemática desde código (ej. al entrar en un Trigger):

```csharp
using CinematicSystem.Application;
using CinematicSystem.Core;

public class CinematicTrigger : MonoBehaviour
{
    public CinematicAsset cinematicToPlay;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Buscar el reproductor en la escena
            var player = FindFirstObjectByType<CinematicPlayer>();
            if (player != null)
            {
                player.Play(cinematicToPlay);
            }
        }
    }
}
```

## 6. Crear Nuevas Acciones (Programador)

Para añadir nuevas funcionalidades (ej. reproducir sonido, animar UI, mover NPC):

1.  Crea un nuevo script C# en `Assets/Scripts/CinematicSystem/Actions`.
2.  Hereda de `CinematicAction`.
3.  Implementa el método `Execute`:

```csharp
using CinematicSystem.Core;
using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Cinematic System/Actions/My Custom Action")]
public class MyCustomAction : CinematicAction
{
    public string message;

    public override IEnumerator Execute(ICinematicContext context)
    {
        Debug.Log(message);
        yield return null; // Esperar un frame (o usar WaitForSeconds)
    }
}
```
