using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.XR;

public class Grabbable : MonoBehaviour
{
    public bool usePhysics;

    [SerializeField]
    private float throwForce = 2.0f;

    [SerializeField]
    private InventoryCallback inventoryCallback;

    [SerializeField]
    private HapticConfig hoverHaptics;

    [SerializeField]
    private HandPositioning handPositioning;

    // Events
    public UnityEvent onHoverEnter;
    public UnityEvent onHoverExit;

    public UnityEvent onGrabStart;
    public UnityEvent onGrabStop;

    public UnityEvent onActivateStart;
    public UnityEvent onActivateStop;

    public UnityEvent onPrimary;
    public UnityEvent onSecondary;

    // Secondary Grab Events
    public UnityEvent onSecondaryGrabStart;
    public UnityEvent onSecondaryGrabStop;

    protected bool grabbable = true;
    protected bool grabbed;
    protected bool secondaryGrabbed;

    protected Rigidbody rb;
    protected Transform target;

    protected Grabber currentGrabber; // Référence explicite au Grabber actif
    protected Grabber secondaryGrabber; // Référence au grabber secondaire

    protected Vector3 previousLocalPosition;
    protected Vector3 previousLocalRotation;

    protected Vector3 previousPosition;
    protected Vector3 previousRotation;

    protected bool storedMomentum;
    protected Vector3 velocityMomentum;
    protected Vector3 angularMomentum;

    // **** Nouvel ajout : le pivot personnalisé ****
    [Tooltip("Si défini, ce pivot interne sera utilisé pour le suivi lors du grab (la position/rotation de ce pivot sera alignée sur celle de la cible).")]
    [SerializeField]
    protected Transform customPivot;


    public bool Grabbed { get => grabbed; }
    public bool SecondaryGrabbed { get => secondaryGrabbed; }

    public Transform CustomPivot { get => customPivot; set => customPivot = value; }

    public bool StoredUsePhysics { 
        get => inventoryCallback && inventoryCallback.Stored ? false : usePhysics; 
    }

    protected void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    protected void Update()
    {
        UpdateRigidbody();
        UpdateTransform();

        if ((transform.position - previousPosition).magnitude > 0)
            velocityMomentum = transform.position - previousPosition;

        if ((transform.eulerAngles - previousRotation).magnitude > 0)
            angularMomentum = transform.eulerAngles - previousRotation;


        previousPosition = transform.position;
        previousRotation = transform.eulerAngles;
    }

    protected virtual void UpdateRigidbody()
    {
        if (rb)
        {
                if (inventoryCallback)
                {
                    rb.isKinematic = grabbed || inventoryCallback.Stored;
                }
                else
                {
                    rb.isKinematic = grabbed;
                }

            
            if (usePhysics && grabbed)
            {
                rb.useGravity = false;
                rb.constraints = RigidbodyConstraints.FreezeAll;
            }
            else if (usePhysics && !grabbed)
            {
                rb.useGravity = true;
                rb.constraints = RigidbodyConstraints.None;
            }

            if (storedMomentum && !rb.isKinematic)
            {


                rb.linearVelocity = velocityMomentum / Time.deltaTime * throwForce;
                rb.angularVelocity = angularMomentum / Time.deltaTime;

                storedMomentum = false;
            }
        }
    }

    protected virtual void UpdateTransform()
    {
        if (grabbed && target && !usePhysics)
        {
            // Si un pivot personnalisé est défini, on l'utilise pour calculer la transformation du grabbable
            if (customPivot != null)
            {
                // On calcule la rotation de l'objet de manière à ce que customPivot (dont la rotation locale est fixe)
                // se retrouve aligné avec la rotation de la cible.
                transform.rotation = target.rotation * Quaternion.Inverse(customPivot.localRotation);
                // On calcule la position de l'objet pour que customPivot se retrouve exactement à target.position.
                transform.position = target.position - transform.rotation * customPivot.localPosition;
            }
            else
            {
                transform.position = target.position;
                transform.rotation = target.rotation;
            }
        }
    }

    protected void OnTriggerEnter(Collider other)
    {
        if (grabbable)
        {
            Grabber grabber = other.GetComponent<Grabber>();
            if (grabber && !grabber.Grabbing && !grabber.Busy)
            {
                if (!grabbed)
                {
                    // Premier grab potentiel
                    currentGrabber = grabber; // Assigner le Grabber actif
                    SendHaptic(hoverHaptics);
                    onHoverEnter?.Invoke();
                    grabber.grabAction.action.started += OnGrabActionStarted;
                    grabber.Busy = true;
                }
                else if (grabbed && !secondaryGrabbed && grabber != currentGrabber)
                {
                    // Si déjà grabbé par une main, mais pas encore par une seconde
                    secondaryGrabber = grabber;
                    SendHaptic(hoverHaptics);
                    onHoverEnter?.Invoke();
                    grabber.grabAction.action.started += OnSecondaryGrabActionStarted;
                    grabber.Busy = true;
                }
            }
        }
    }

    protected void OnTriggerExit(Collider other)
    {
        if (grabbable)
        {
            Grabber grabber = other.GetComponent<Grabber>();
            if (grabber)
            {
                if (!grabbed && grabber == currentGrabber)
                {
                    onHoverExit?.Invoke();
                    grabber.grabAction.action.started -= OnGrabActionStarted;
                    grabber.Busy = false;
                    currentGrabber = null; // Nettoyer le Grabber actif
                }
                else if (grabbed && !secondaryGrabbed && grabber == secondaryGrabber)
                {
                    onHoverExit?.Invoke();
                    grabber.grabAction.action.started -= OnSecondaryGrabActionStarted;
                    grabber.Busy = false;
                    secondaryGrabber = null;
                }
            }
        }
    }

    public virtual void Grab(Grabber grabber)
    {
        currentGrabber = grabber;

        grabber.grabAction.action.canceled += OnGrabActionCanceled;
        grabber.primaryAction.action.started += OnPrimaryStarted;
        grabber.secondaryAction.action.started += OnSecondaryStarted;
        grabber.triggerAction.action.started += OnActivateStarted;
        grabber.triggerAction.action.canceled += OnActivateCanceled;

        onGrabStart?.Invoke();
        grabbed = true;
        grabber.Grabbing = true;

        target = grabber.GrabTransform;

        if (handPositioning) {
            handPositioning.PositionHand(grabber.Hand.transform, grabber.Handedness);
        }

        if (usePhysics) {
            if (customPivot != null) {
                transform.rotation = target.rotation * Quaternion.Inverse(customPivot.localRotation);
                transform.position = target.position - transform.rotation * customPivot.localPosition;
            }
            else
            {
                transform.position = target.position;
                transform.rotation = target.rotation;
            }

            transform.parent = target;
            Debug.Log("Grab Parent");
        }

        

        // Optionnel : on peut initialiser ici previousLocalPosition si nécessaire.
        previousLocalPosition = transform.localPosition;
        previousLocalRotation = transform.localEulerAngles;
    }

    public virtual void SecondaryGrab(Grabber grabber)
    {
        secondaryGrabber = grabber;

        grabber.grabAction.action.canceled += OnSecondaryGrabActionCanceled;
        
        onSecondaryGrabStart?.Invoke();
        secondaryGrabbed = true;
        grabber.Grabbing = true;

        if (handPositioning) {
            handPositioning.PositionHand(grabber.Hand.transform, grabber.Handedness, true);
        }
    }

    public virtual void Release(Grabber grabber)
    {
        Debug.Log("Release Parent");

        grabber.grabAction.action.canceled -= OnGrabActionCanceled;
        grabber.primaryAction.action.started -= OnPrimaryStarted;
        grabber.secondaryAction.action.started -= OnSecondaryStarted;
        grabber.triggerAction.action.started -= OnActivateStarted;
        grabber.triggerAction.action.canceled -= OnActivateCanceled;

        onGrabStop?.Invoke();
        grabbed = false;
        grabber.Grabbing = false;
        grabber.Busy = false;
        target = null;
        currentGrabber = null; // Nettoyer le Grabber actif

        grabber.ReleaseHand();
        
        if (usePhysics)
            transform.parent = null;

        storedMomentum = true;
    }

    public virtual void SecondaryRelease(Grabber grabber)
    {
        Debug.Log("Secondary Release");

        grabber.grabAction.action.canceled -= OnSecondaryGrabActionCanceled;

        onSecondaryGrabStop?.Invoke();
        secondaryGrabbed = false;
        grabber.Grabbing = false;
        grabber.Busy = false;
        secondaryGrabber = null;

        grabber.ReleaseHand();
    }

    public void SendHaptic(HapticConfig config)
    {
        if (currentGrabber)
        {
            currentGrabber.gameObject.SendMessage("StartHaptic", config);
        }
        
        if (secondaryGrabber)
        {
            secondaryGrabber.gameObject.SendMessage("StartHaptic", config);
        }
    }

    // Méthodes de callback
    protected void OnGrabActionStarted(InputAction.CallbackContext context)
    {
        if (currentGrabber != null)
        {
            Grab(currentGrabber);
        }
    }

    protected void OnSecondaryGrabActionStarted(InputAction.CallbackContext context)
    {
        if (secondaryGrabber != null)
        {
            SecondaryGrab(secondaryGrabber);
        }
    }

    protected void OnGrabActionCanceled(InputAction.CallbackContext context)
    {
        if (currentGrabber != null)
        {
            Release(currentGrabber);
            
            // Si on a un grab secondaire actif, le transformer en grab primaire
            if (secondaryGrabbed && secondaryGrabber != null)
            {
                Grabber tempGrabber = secondaryGrabber;
                
                // Libérer le grab secondaire
                SecondaryRelease(secondaryGrabber);
                
                // Le transformer en grab primaire
                Grab(tempGrabber);
            }
        }
    }

    protected void OnSecondaryGrabActionCanceled(InputAction.CallbackContext context)
    {
        Debug.Log("Secondary Grab Canceled");

        if (secondaryGrabber != null)
        {
            SecondaryRelease(secondaryGrabber);
        }
    }

    protected void OnActivateStarted(InputAction.CallbackContext context)
    {
        if (currentGrabber != null)
        {
            onActivateStart?.Invoke();
        }
    }

    protected void OnActivateCanceled(InputAction.CallbackContext context)
    {
        if (currentGrabber != null)
        {
            onActivateStop?.Invoke();
        }
    }

    protected void OnPrimaryStarted(InputAction.CallbackContext context)
    {
        if (currentGrabber != null)
        {
            onPrimary?.Invoke();
        }
    }

    protected void OnSecondaryStarted(InputAction.CallbackContext context)
    {
        if (currentGrabber != null)
        {
            onSecondary?.Invoke();
        }
    }
}
