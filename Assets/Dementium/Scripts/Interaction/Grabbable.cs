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

    public Transform CustomPivot { get => customPivot; set => customPivot = value; }

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
            if (!usePhysics)
            {
                if (inventoryCallback)
                {
                    rb.isKinematic = grabbed || inventoryCallback.Stored;
                }
                else
                {
                    rb.isKinematic = grabbed;
                }

            }
            else
            {
                rb.useGravity = !grabbed;
                rb.constraints = grabbed ? RigidbodyConstraints.FreezeAll : RigidbodyConstraints.None;
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
            if (!grabbed && grabber && !grabber.Grabbing)
            {
                currentGrabber = grabber; // Assigner le Grabber actif
                SendHaptic(hoverHaptics);
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
        grabber.secondaryAction.action.started += OnSecondaryStarted;
        grabber.triggerAction.action.started += OnActivateStarted;
        grabber.triggerAction.action.canceled += OnActivateCanceled;

        onGrabStart?.Invoke();
        grabbed = true;
        grabber.Grabbing = true;

        target = grabber.GrabTransform;

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
        }

        

        // Optionnel : on peut initialiser ici previousLocalPosition si nécessaire.
        previousLocalPosition = transform.localPosition;
        previousLocalRotation = transform.localEulerAngles;
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
        target = null;
        currentGrabber = null; // Nettoyer le Grabber actif

        if (usePhysics)
            transform.parent = null;

        storedMomentum = true;
    }

    public void SendHaptic(HapticConfig config)
    {
        if (currentGrabber)
        {
            currentGrabber.gameObject.SendMessage("StartHaptic", config);
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

    protected void OnGrabActionCanceled(InputAction.CallbackContext context)
    {
        if (currentGrabber != null)
        {
            Release(currentGrabber);
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
