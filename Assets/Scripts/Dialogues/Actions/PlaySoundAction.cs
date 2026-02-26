using UnityEngine;

[CreateAssetMenu(fileName = "PlaySoundAction", menuName = "Dialogue System/Actions/Play Sound")]
public class PlaySoundAction : ActionBase
{
    public AudioClip clip;

    [Range(0f, 3f)] 
    public float volume = 1f;

    public override void Execute(DialogueContext context)
    {
        if (clip == null) return;

        GameObject tempGO = new GameObject("TempAudio");
        AudioSource audioSource = tempGO.AddComponent<AudioSource>();

        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.Play();

        Object.Destroy(tempGO, clip.length);
    }
}