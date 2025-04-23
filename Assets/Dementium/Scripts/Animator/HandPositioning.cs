using UnityEngine;
using UnityEngine.Events;

public class HandPositioning : MonoBehaviour
{
    [Header("Hand Transform References")]
    [SerializeField] private Transform rightHandTransform;
    [SerializeField] private Transform leftHandTransform;

    [Header("Animation Parameters")]
    [SerializeField] private string grabAnimationTrigger = "Grab";

    // Store original hand transforms to restore when released
    



    public void PositionHand(Transform handModel, Handedness handedness)
    {
        Transform targetTransform = rightHandTransform;
        if (handedness == Handedness.Left) {
            targetTransform = leftHandTransform;
        }

        if (handModel == null || targetTransform == null) return;

        Animator handAnimator = handModel.GetComponent<Animator>();
        if (handAnimator != null)
        {
            handAnimator.SetTrigger(grabAnimationTrigger);
        }

        handModel.SetParent(targetTransform);
        handModel.localPosition = Vector3.zero;
        handModel.localRotation = Quaternion.identity;
    }

}
