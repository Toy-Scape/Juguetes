using System;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// Global system to manage noise propagation.
    /// Objects call MakeNoise(), and HearingSensors listening will react.
    /// </summary>
    public static class NoiseManager
    {
        // Event payload: Position, Range/Loudness
        public static event Action<Vector3, float> OnNoiseEmitted;

        public static void MakeNoise(Vector3 position, float range)
        {
            if (range <= 0) return;
            OnNoiseEmitted?.Invoke(position, range);
        }
    }
}
