using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Grabbable : MonoBehaviour
{
    // Events
    public UnityEvent onHoverEnter;
    public UnityEvent onHoverExit;

    public UnityEvent onGrabStart;
    public UnityEvent onGrabStop;

    public UnityEvent onActivateStart;
    public UnityEvent onActivateStop;

    public UnityEvent onPrimary;
    public UnityEvent onSecondary;

    protected bool grabbable = true;
    protected bool grabbed;

    protected Rigidbody rb;
    protected Transform target;

    protected Grabber currentGrabber; // Référence explicite au Grabber actif

    protected Vector3 previousLocalPosition;
    protected Vector3 previousLocalRotation;

    public bool Grabbed { get => grabbed; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

 

    protected void Update()
    {
        UpdateRigidbody();
        UpdateTransform();
    }

    protected virtual void UpdateRigidbody()
    {
        if (rb)
            rb.isKinematic = grabbed;
    }

    protected virtual void UpdateTransform()
    {
        if (grabbed && target)
        {
            transform.position = target.position;
            transform.rotation = target.rotation;
        }
    }

    protected void OnTriggerEnter(Collider other)
    {
        if (grabbable)
        {
            Grabber grabber = other.GetComponent<Grabber>();
            if (!grabbed && grabber && !grabber.Grabbing)
            {
                currentGrabber = grabber; // Assigner le Grabber actif
                onHoverEnter?.Invoke();
                grabber.grabAction.action.started += OnGrabActionStarted;
            }
        }
    }

    protected void OnTriggerExit(Collider other)
    {
        if (grabbable)
        {
            Grabber grabber = other.GetComponent<Grabber>();
            if (!grabbed && grabber && grabber == currentGrabber)
            {
                onHoverExit?.Invoke();
                grabber.grabAction.action.started -= OnGrabActionStarted;

                currentGrabber = null; // Nettoyer le Grabber actif
            }
        }
    }

    public virtual void Grab(Grabber grabber)
    {
        currentGrabber = grabber;

        grabber.grabAction.action.canceled += OnGrabActionCanceled;
        grabber.primaryAction.action.started += OnPrimaryStarted;
        grabber.triggerAction.action.started += OnActivateStarted;
        grabber.triggerAction.action.canceled += OnActivateCanceled;


        onGrabStart?.Invoke();
        grabbed = true;
        grabber.Grabbing = true;

        target = grabber.GrabTransform;
    }

    public virtual void Release(Grabber grabber)
    {
        Debug.Log("Release Parent");

        grabber.grabAction.action.canceled -= OnGrabActionCanceled;
        grabber.primaryAction.action.started -= OnPrimaryStarted;
        grabber.triggerAction.action.started -= OnActivateStarted;
        grabber.triggerAction.action.canceled -= OnActivateCanceled;

        onGrabStop?.Invoke();
        grabbed = false;
        grabber.Grabbing = false;
        target = null;
        currentGrabber = null; // Nettoyer le Grabber actif
    }

    // Méthodes de callback
    protected void OnGrabActionStarted(InputAction.CallbackContext context)
    {
        if (currentGrabber != null) // Vérifie si un Grabber est actif
        {
            Grab(currentGrabber);
        }
    }

    protected void OnGrabActionCanceled(InputAction.CallbackContext context)
    {
        if (currentGrabber != null) // Vérifie si un Grabber est actif
        {
            Release(currentGrabber);
        }
    }

    protected void OnActivateStarted(InputAction.CallbackContext context)
    {
        if (currentGrabber != null) // Vérifie si un Grabber est actif
        {
            onActivateStart?.Invoke();
        }
    }

    protected void OnActivateCanceled(InputAction.CallbackContext context)
    {
        if (currentGrabber != null) // Vérifie si un Grabber est actif
        {
            onActivateStop?.Invoke();
        }
    }

    protected void OnPrimaryStarted(InputAction.CallbackContext context)
    {
        if (currentGrabber != null) // Vérifie si un Grabber est actif
        {
            onPrimary?.Invoke();
        }
    }
}
