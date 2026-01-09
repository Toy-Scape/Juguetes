using Core;
using UnityEngine;

namespace Domain
{
    /// <summary>
    /// Fábrica simple para crear estados del NPC. Centraliza la construcción
    /// y permite sustituir implementaciones futuras o introducir inyección.
    /// </summary>
    public static class StateFactory
    {
        public static PatrolState CreatePatrol(NpcBrain npc, INavigationAgent agent, SO.PatrolRouteSO route, float speed)
        {
            return new PatrolState(npc, agent, route, speed);
        }

        public static ChaseState CreateChase(NpcBrain npc, INavigationAgent agent, Transform target, float speed)
        {
            return new ChaseState(npc, agent, target, speed);
        }

        public static SearchingState CreateSearching(INavigationAgent agent, Vector3 origin, float radius, float duration, float speed, float approachSpeed)
        {
            return new SearchingState(agent, origin, radius, duration, speed, approachSpeed);
        }
    }
}
