using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Subtitle Line")]
public class SubtitleLine : ScriptableObject
{
    public AudioClip audioClip;
    [TextArea(2, 5)]
    public string subtitleText;
}
