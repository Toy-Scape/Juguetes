using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public interface ITargetDetector
    {
        List<Transform> GetVisibleTargets(Transform self);
        bool CanSeeTarget(Transform self, Transform target);
    }
}
