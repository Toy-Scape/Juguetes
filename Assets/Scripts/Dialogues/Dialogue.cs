using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Dialogue", menuName = "Dialogue System/Dialogue")]
public class Dialogue : ScriptableObject
{
    public List<Line> lines = new();

    public static Dialogue Load(string relativePath)
    {
        return Resources.Load<Dialogue>($"Dialogues/{relativePath}");
    }

    [System.Serializable]
    public struct Line
    {
        public Character character;
        public string text;

        public string GetCharacterName()
        {
            switch (character)
            {
                case Character.Robotin:
                    return "Robotín";
                case Character.JuanElCenas:
                    return "Juan el Cenas";
                default:
                    return "Unknown";
            }
        }

        public enum Character
        {
            Robotin,
            JuanElCenas
        }
    }
}
