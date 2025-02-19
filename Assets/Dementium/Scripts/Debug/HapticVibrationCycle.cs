using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

public class HapticVibrationCycle : MonoBehaviour
{
    [Tooltip("R�f�rence vers le composant HapticImpulsePlayer.")]
    public HapticImpulsePlayer hapticImpulsePlayer;

    [Tooltip("Dur�e de l'impulsion haptique (en secondes).")]
    public float impulseDuration = 0.5f;

    [Tooltip("Intervalle de temps entre deux impulsions (en secondes).")]
    public float interval = 2.0f;

    private void OnEnable()
    {
        StartCoroutine(VibrationRoutine());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator VibrationRoutine()
    {
        while (true)
        {
            if (hapticImpulsePlayer != null)
            {
                // Envoie une impulsion haptique � pleine amplitude (1.0) pour la dur�e d�finie.
                hapticImpulsePlayer.SendHapticImpulse(1.0f, impulseDuration);
            }
            yield return new WaitForSeconds(interval);
        }
    }
}
