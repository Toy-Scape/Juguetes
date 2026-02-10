using UnityEngine;

namespace Core
{
    /// <summary>
    /// Contract for objects that can be detected by the NPC VisionSensor.
    /// Allows targets to define their own point of interest (e.g., chest/head instead of feet).
    /// </summary>
    public interface IVisibleTarget
    {
        /// <summary>
        /// The precise world position that should be checked for visibility.
        /// Useful to aim at the center of mass or head instead of the pivot point.
        /// </summary>
        Vector3 TargetPosition { get; }

        /// <summary>
        /// If false, this target is currently ignored (e.g. dead, invisible).
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// The main transform of the target.
        /// </summary>
        Transform Transform { get; }
    }
}
