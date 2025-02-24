using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

public struct VibrationType { }

public class Haptic : MonoBehaviour
{
    [Tooltip("R�f�rence vers le composant HapticImpulsePlayer.")]
    public HapticImpulsePlayer hapticImpulsePlayer;


    public void StartHaptic(HapticConfig config)
    {
        hapticImpulsePlayer.SendHapticImpulse(config.Amplitude, config.Duration);
    }
}
