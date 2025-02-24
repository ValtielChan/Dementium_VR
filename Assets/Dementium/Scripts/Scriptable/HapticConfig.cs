using UnityEngine;

[CreateAssetMenu(fileName = "NewHapticConfig", menuName = "DementiumVR/Haptic/HapticConfig")]
public class HapticConfig : ScriptableObject
{
    [SerializeField, Range(0f, 1f)] private float amplitude;
    [SerializeField] private float duration;

    public float Amplitude => amplitude;
    public float Duration => duration;
}