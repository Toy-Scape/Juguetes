using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance { get; private set; }

    [System.Serializable]
    public class CharacterEntry
    {
        public string id;
        public GameObject model;
    }

    public List<CharacterEntry> characters;

    private void Awake ()
    {
        Instance = this;
    }

    public GameObject GetModel (string id)
    {
        foreach (var entry in characters)
            if (entry.id == id)
                return entry.model;

        return null;
    }
}
