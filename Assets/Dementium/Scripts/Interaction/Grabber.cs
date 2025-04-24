using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public enum Handedness
{
    Left,
    Right
}

public class Grabber : MonoBehaviour
{

    public InputActionReference grabAction;
    public InputActionReference triggerAction;
    public InputActionReference primaryAction;
    public InputActionReference secondaryAction;

    private bool busy;

    private bool grabbing;

    [SerializeField]
    private Transform grabTransform;

    [SerializeField]
    private GameObject hand;

    [SerializeField]
    private Handedness handedness;

    private Vector3 originalHandPosition;
    private Quaternion originalHandRotation;
    private Transform originalParent;

    public Transform GrabTransform
    {
        get
        {
            if (grabTransform == null)
            {
                return transform;
            }
            else
            {
                return grabTransform;
            }
        }
    }

    public bool Grabbing { get => grabbing; set => grabbing = value; }
    public bool Busy { get => busy; set => busy = value; }
    public GameObject Hand { get => hand; set => hand = value; }
    public Handedness Handedness { get => handedness; set => handedness = value; }

    void Start()
    {
        originalHandPosition = hand.transform.localPosition;
        originalHandRotation = hand.transform.localRotation;
        originalParent = hand.transform.parent;
    }

    public void ReleaseHand()
    {

        if (hand != null)
        {

            Animator handAnimator = hand.GetComponent<Animator>();
            if (handAnimator != null)
            {
                handAnimator.SetTrigger("Release");
            }

            hand.transform.parent = originalParent;
            hand.transform.localPosition = originalHandPosition;
            hand.transform.localRotation = originalHandRotation;
        }
    }

}
