using UnityEngine;
using UnityEngine.Events;

public class HandPositioning : MonoBehaviour
{
    [Header("Hand Transform References")]
    [SerializeField] private Transform rightHandTransform;
    [SerializeField] private Transform leftHandTransform;

    [Header("Secondary Hand Transform References")]
    [SerializeField] private Transform secondaryRightHandTransform;
    [SerializeField] private Transform secondaryLeftHandTransform;

    [Header("Animation Parameters")]
    [SerializeField] private string grabAnimationTrigger = "Grab";
    [SerializeField] private string secondaryGrabAnimationTrigger = "SecondaryGrab";

    // Store original hand transforms to restore when released
    



    public void PositionHand(Transform handModel, Handedness handedness, bool isSecondaryGrab = false)
    {
        Transform targetTransform;
        
        if (isSecondaryGrab)
        {
            // Utiliser les positions secondaires
            targetTransform = handedness == Handedness.Right ? secondaryRightHandTransform : secondaryLeftHandTransform;
        }
        else
        {
            // Utiliser les positions principales
            targetTransform = handedness == Handedness.Right ? rightHandTransform : leftHandTransform;
        }

        if (handModel == null || targetTransform == null) return;

        Animator handAnimator = handModel.GetComponent<Animator>();
        if (handAnimator != null)
        {
            handAnimator.SetTrigger(isSecondaryGrab ? secondaryGrabAnimationTrigger : grabAnimationTrigger);
        }

        handModel.SetParent(targetTransform);
        handModel.localPosition = Vector3.zero;
        handModel.localRotation = Quaternion.identity;
    }

}
