using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue System/Character")]
public class DialogueCharacter : ScriptableObject
{
    public string DisplayName;
    public Sprite Portrait;
    public Color NameColor;
    public AudioClip Voice;
}
