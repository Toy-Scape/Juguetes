using System;
using UnityEngine;

namespace MissionSystem.Utils
{
    [RequireComponent(typeof(Collider))]
    public class MissionLocationTrigger : MonoBehaviour
    {
        [SerializeField] private string locationID;
        [SerializeField] private bool oneShot = true;

        public static event Action<string> OnLocationReached;

        private bool _hasTriggered = false;

        private void OnTriggerEnter(Collider other)
        {
            if (_hasTriggered && oneShot) return;

            // Simple check: Assume player has tag "Player" or similar. 
            // Better: Check for specific component like PlayerController.
            if (other.CompareTag("Player"))
            {
                _hasTriggered = true;
                OnLocationReached?.Invoke(locationID);
                Debug.Log($"[MissionLocationTrigger] Reached: {locationID}");
            }
        }

        private void OnValidate()
        {
            var col = GetComponent<Collider>();
            if (col != null && !col.isTrigger)
            {
                col.isTrigger = true;
                Debug.LogWarning("Collider on MissionLocationTrigger must be a Trigger. Setting it to Trigger now.");
            }
        }
    }
}
