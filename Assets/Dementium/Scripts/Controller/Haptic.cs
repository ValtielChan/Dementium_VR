using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

public struct VibrationType { }

public class Haptic : MonoBehaviour
{
    [Tooltip("Référence vers le composant HapticImpulsePlayer.")]
    public HapticImpulsePlayer hapticImpulsePlayer;


    public void StartHaptic(HapticConfig config)
    {
        hapticImpulsePlayer.SendHapticImpulse(config.Amplitude, config.Duration);
    }
}
