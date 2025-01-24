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

    protected bool grabbed;
    protected Rigidbody rb;
    protected Transform target;

    private Grabber currentGrabber; // Référence explicite au Grabber actif

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
        if (rb)
            rb.isKinematic = grabbed;

        if (grabbed && target)
        {
            transform.position = target.position;
            transform.rotation = target.rotation;
        }
    }

    protected void OnTriggerEnter(Collider other)
    {
        Grabber grabber = other.GetComponent<Grabber>();
        if (!grabbed && grabber)
        {
            currentGrabber = grabber; // Assigner le Grabber actif
            onHoverEnter?.Invoke();
            grabber.grabAction.action.started += OnGrabActionStarted;
        }
    }

    protected void OnTriggerExit(Collider other)
    {
        Grabber grabber = other.GetComponent<Grabber>();
        if (!grabbed && grabber && grabber == currentGrabber)
        {
            onHoverExit?.Invoke();
            grabber.grabAction.action.started -= OnGrabActionStarted;
            currentGrabber = null; // Nettoyer le Grabber actif
        }
    }

    public virtual void Grab(Grabber grabber)
    {
        grabber.grabAction.action.canceled += OnGrabActionCanceled;

        onGrabStart?.Invoke();
        grabbed = true;

        target = grabber.GrabTransform;
    }

    public virtual void Release(Grabber grabber)
    {
        Debug.Log("Release Parent");

        grabber.grabAction.action.canceled -= OnGrabActionCanceled;

        onGrabStop?.Invoke();
        grabbed = false;
        target = null;
        currentGrabber = null; // Nettoyer le Grabber actif
    }

    // Méthodes de callback
    private void OnGrabActionStarted(InputAction.CallbackContext context)
    {
        if (currentGrabber != null) // Vérifie si un Grabber est actif
        {
            Grab(currentGrabber);
        }
    }

    private void OnGrabActionCanceled(InputAction.CallbackContext context)
    {
        if (currentGrabber != null) // Vérifie si un Grabber est actif
        {
            Release(currentGrabber);
        }
    }
}
