using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Interactable : MonoBehaviour
{
    [Header("Interaction Events")]
    public UnityEvent onHoverEnter;
    public UnityEvent onHoverExit;
    public UnityEvent onInteractStart;
    public UnityEvent onInteractStop;


    protected Grabber currentInteractor;

    protected virtual void OnTriggerEnter(Collider other)
    {
        Grabber grabber = other.GetComponent<Grabber>();
        if (grabber)
        {
            currentInteractor = grabber;
            onHoverEnter?.Invoke();

            // Abonnement aux actions
            grabber.grabAction.action.started += OnInteractionStarted;
            grabber.grabAction.action.canceled += OnInteractionCanceled;
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        Grabber grabber = other.GetComponent<Grabber>();
        if (grabber && grabber == currentInteractor)
        {
            onHoverExit?.Invoke();

            // Désabonnement des actions
            grabber.grabAction.action.started -= OnInteractionStarted;
            grabber.grabAction.action.canceled -= OnInteractionCanceled;

            currentInteractor = null;
        }
    }

    protected virtual void OnInteractionStarted(InputAction.CallbackContext context)
    {

        onInteractStart?.Invoke();
    }

    protected virtual void OnInteractionCanceled(InputAction.CallbackContext context)
    {

        onInteractStop?.Invoke();
    }
}