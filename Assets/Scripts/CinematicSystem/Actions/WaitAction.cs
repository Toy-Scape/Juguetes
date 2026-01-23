using System.Collections;
using CinematicSystem.Core;
using UnityEngine;

namespace CinematicSystem.Actions
{
    [CreateAssetMenu(fileName = "WaitAction", menuName = "Cinematic System/Actions/Wait")]
    public class WaitAction : CinematicAction
    {
        public float duration = 1f;

        public override IEnumerator Execute(ICinematicContext context)
        {
            if (duration > 0)
            {
                yield return new WaitForSeconds(duration);
            }
        }
    }
}
