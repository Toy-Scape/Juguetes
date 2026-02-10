using UnityEngine;
using System;

namespace Core
{
    public interface INavigationAgent
    {
        bool PathPending { get; }
        float RemainingDistance { get; }
        Vector3 Velocity { get; }
        float Speed { get; set; }

        // Permite establecer el destino del agente de navegación
        void SetDestination(Vector3 target);
    }
}
