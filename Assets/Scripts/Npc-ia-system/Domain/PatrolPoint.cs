using System;
using UnityEngine;

namespace Domain
{
    [System.Serializable]
    public class PatrolPoint
    {
        public Transform point;
        public float waitTime = 0f;
        public Action onReachPoint;
    }
}