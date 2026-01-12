using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Dialogue System/Character Database")]
public class CharacterDatabase : ScriptableObject
{
    public List<string> CharacterIds = new List<string>();
}
