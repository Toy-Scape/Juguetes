using System.Collections.Generic;
using UnityEngine;

namespace CinematicSystem.Core
{
    [CreateAssetMenu(fileName = "NewCinematic", menuName = "Cinematic System/Cinematic Asset")]
    public class CinematicAsset : ScriptableObject
    {
        [Header("Settings")]
        public bool blockPlayerInput = true;
        public bool hideHUD = true;

        [Header("Sequence")]
        public List<CinematicAction> actions = new List<CinematicAction>();

        public void AddAction(CinematicAction action)
        {
            actions.Add(action);
        }
    }
}
