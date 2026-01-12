using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Dialogue", menuName = "Dialogue System/Dialogue")]
public class Dialogue : ScriptableObject
{
    public DialogueType Type;
    public List<Line> Lines = new();

    public enum DialogueType
    {
        Normal,
        Thought
    }

    public static Dialogue Load(string relativePath)
    {
        return Resources.Load<Dialogue>($"Dialogues/{relativePath}");
    }

    [System.Serializable]
    public struct Line
    {
        public DialogueCharacter Character;

        [Header("Localization")]
        [Tooltip("Key from the Localization Database")]
        public string Key;

        [TextArea] public string Text;
        public List<DialogueAction> Actions;

        public string GetCharacterName()
        {
            return Character != null ? Character.DisplayName : "Unknown";
        }

        public Color GetNameColor()
        {
            var color = Character != null ? Character.NameColor : Color.white;
            color.a = 1f;
            return color;
        }

        public Sprite GetPortrait()
        {
            return Character != null ? Character.Portrait : null;
        }
    }

    public void TriggerActions(int lineIndex, TriggerTiming timing, DialogueContext context)
    {
        if (lineIndex < 0 || lineIndex >= Lines.Count)
            return;

        foreach (var action in Lines[lineIndex].Actions)
        {
            if (action.Timing == timing)
                action.Execute(context);
        }
    }

}
