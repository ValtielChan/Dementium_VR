using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class DOFConstraint
{
    public bool enableLocalTranslationX = true;
    public float minLocalTranslationX = -0.1f;
    public float maxLocalTranslationX = 0.1f;

    public bool enableLocalTranslationY = false;
    public float minLocalTranslationY;
    public float maxLocalTranslationY;

    public bool enableLocalTranslationZ = false;
    public float minLocalTranslationZ;
    public float maxLocalTranslationZ;

    public bool enableLocalRotationX = false;
    public float minLocalRotationX;
    public float maxLocalRotationX;

    public bool enableLocalRotationY = false;
    public float minLocalRotationY;
    public float maxLocalRotationY;

    public bool enableLocalRotationZ = false;
    public float minLocalRotationZ;
    public float maxLocalRotationZ;

    public float springStrength = 0.0f;

    // Events for limits
    public UnityEvent onMinTranslationLimitReached;
    public UnityEvent onMaxTranslationLimitReached;
    public UnityEvent onMinRotationLimitReached;
    public UnityEvent onMaxRotationLimitReached;
}

public class ConstrainedGrabbable : Grabbable
{
    [Header("Constraints")]
    public DOFConstraint dofConstraint;

    private Vector3 initialLocalPosition;
    private Vector3 initialLocalRotation;

    private Vector3 grabberInitialPosition; // Référence initiale de position du Grabber
    private Quaternion grabberInitialRotation; // Référence initiale de rotation du Grabber

    new void Start()
    {
        base.Start();

        initialLocalPosition = transform.localPosition; // Stockage de la position initiale
        initialLocalRotation = transform.localEulerAngles; // Stockage de la rotation initiale
    }

    new protected void OnTriggerEnter(Collider other)
    {
        if (grabbable)
        {
            Grabber grabber = other.GetComponent<Grabber>();
            if (grabber && !grabber.Grabbing) // On ignore grabber.Busy
            {
                if (!grabbed)
                {
                    // Premier grab potentiel
                    currentGrabber = grabber;
                    SendHaptic(hoverHaptics);
                    onHoverEnter?.Invoke();
                    
                    // S'abonner à l'action choisie (HandTrigger ou IndexTrigger)
                    if (grabTriggerType == GrabTriggerType.HandTrigger)
                    {
                        grabber.grabAction.action.started += OnGrabActionStarted;
                    }
                    else // IndexTrigger
                    {
                        grabber.triggerAction.action.started += OnGrabActionStarted;
                    }
                    
                    grabber.Busy = true;
                }
                // Pas de logique de secondary grabber ici
            }
        }
    }

    new protected void OnTriggerExit(Collider other)
    {
        if (grabbable)
        {
            Grabber grabber = other.GetComponent<Grabber>();
            if (grabber)
            {
                if (!grabbed && grabber == currentGrabber)
                {
                    onHoverExit?.Invoke();
                    
                    // Se désabonner de l'action choisie
                    if (grabTriggerType == GrabTriggerType.HandTrigger)
                    {
                        grabber.grabAction.action.started -= OnGrabActionStarted;
                    }
                    else // IndexTrigger
                    {
                        grabber.triggerAction.action.started -= OnGrabActionStarted;
                    }
                    
                    grabber.Busy = false;
                    currentGrabber = null;
                }
                // Pas de logique de secondary grabber ici
            }
        }
    }

    new void Update()
    {
        if (grabbed && target != null)
        {
            // Calcul des translations locales par rapport à la position initiale du Grabber
            Vector3 grabberDeltaPosition = target.position - grabberInitialPosition;
            Vector3 targetLocalPosition = transform.parent.InverseTransformPoint(transform.parent.position + grabberDeltaPosition);
            Vector3 constrainedLocalPosition = transform.localPosition;

            if (dofConstraint.enableLocalTranslationX)
            {
                constrainedLocalPosition.x = Mathf.Clamp(targetLocalPosition.x, dofConstraint.minLocalTranslationX, dofConstraint.maxLocalTranslationX);
                TriggerTranslationEvent(constrainedLocalPosition.x, previousLocalPosition.x, dofConstraint.minLocalTranslationX, dofConstraint.maxLocalTranslationX);
            }

            if (dofConstraint.enableLocalTranslationY)
            {
                constrainedLocalPosition.y = Mathf.Clamp(targetLocalPosition.y, dofConstraint.minLocalTranslationY, dofConstraint.maxLocalTranslationY);
                TriggerTranslationEvent(constrainedLocalPosition.y, previousLocalPosition.y, dofConstraint.minLocalTranslationY, dofConstraint.maxLocalTranslationY);
            }

            if (dofConstraint.enableLocalTranslationZ)
            {
                constrainedLocalPosition.z = Mathf.Clamp(targetLocalPosition.z, dofConstraint.minLocalTranslationZ, dofConstraint.maxLocalTranslationZ);
                TriggerTranslationEvent(constrainedLocalPosition.z, previousLocalPosition.z, dofConstraint.minLocalTranslationZ, dofConstraint.maxLocalTranslationZ);
            }

            // Calcul des rotations locales par rapport à la rotation initiale du Grabber
            Quaternion grabberDeltaRotation = target.rotation * Quaternion.Inverse(grabberInitialRotation);
            Vector3 targetLocalEulerAngles = (grabberDeltaRotation * Quaternion.Euler(initialLocalRotation)).eulerAngles;
            Vector3 constrainedLocalRotation = transform.localEulerAngles;

            if (dofConstraint.enableLocalRotationX)
            {
                constrainedLocalRotation.x = Mathf.Clamp(targetLocalEulerAngles.x, dofConstraint.minLocalRotationX, dofConstraint.maxLocalRotationX);
                TriggerRotationEvent(constrainedLocalRotation.x, previousLocalRotation.x, dofConstraint.minLocalRotationX, dofConstraint.maxLocalRotationX);
            }

            if (dofConstraint.enableLocalRotationY)
            {
                constrainedLocalRotation.y = Mathf.Clamp(targetLocalEulerAngles.y, dofConstraint.minLocalRotationY, dofConstraint.maxLocalRotationY);
                TriggerRotationEvent(constrainedLocalRotation.y, previousLocalRotation.y, dofConstraint.minLocalRotationY, dofConstraint.maxLocalRotationY);
            }

            if (dofConstraint.enableLocalRotationZ)
            {
                constrainedLocalRotation.z = Mathf.Clamp(targetLocalEulerAngles.z, dofConstraint.minLocalRotationZ, dofConstraint.maxLocalRotationZ);
                TriggerRotationEvent(constrainedLocalRotation.z, previousLocalRotation.z, dofConstraint.minLocalRotationZ, dofConstraint.maxLocalRotationZ);
            }

            // Appliquer les transformations contraintes
            transform.localPosition = constrainedLocalPosition;
            transform.localEulerAngles = constrainedLocalRotation;

            // Sauvegarder les positions/rotations pour les événements
            previousLocalPosition = constrainedLocalPosition;
            previousLocalRotation = constrainedLocalRotation;
        }
    }

    public override void Grab(Grabber grabber)
    {
        base.Grab(grabber);

        // Stocker la transformation initiale du Grabber
        grabberInitialPosition = grabber.transform.position;
        grabberInitialRotation = grabber.transform.rotation;

        // **Solution** : Initialiser les positions/rotations précédentes avec la valeur actuelle
        previousLocalPosition = transform.localPosition;
        previousLocalRotation = transform.localEulerAngles;
    }

    public override void Release(Grabber grabber)
    {
        base.Release(grabber);

        if (dofConstraint.springStrength > 0)
        {
            StartCoroutine(ReturnToInitialPosition());
        }
    }

    private IEnumerator ReturnToInitialPosition()
    {
        float elapsedTime = 0f;
        Vector3 startPosition = transform.localPosition;
        Vector3 startRotation = transform.localEulerAngles;

        while (elapsedTime < 1f) // Durée réglable
        {
            elapsedTime += Time.deltaTime * dofConstraint.springStrength;

            // Lerp vers la position et rotation initiales
            transform.localPosition = Vector3.Lerp(startPosition, initialLocalPosition, elapsedTime);
            transform.localEulerAngles = Vector3.Lerp(startRotation, initialLocalRotation, elapsedTime);

            yield return null;
        }

        dofConstraint.onMaxTranslationLimitReached?.Invoke();

        // Position finale pour éviter les imprécisions
        transform.localPosition = initialLocalPosition;
        transform.localEulerAngles = initialLocalRotation;
    }

    private void TriggerTranslationEvent(float current, float previous, float min, float max)
    {
        if (current <= min && previous > min)
        {
            dofConstraint.onMinTranslationLimitReached?.Invoke();
        }
        if (current >= max && previous < max)
        {
            dofConstraint.onMaxTranslationLimitReached?.Invoke();
        }
    }

    private void TriggerRotationEvent(float current, float previous, float min, float max)
    {
        if (current <= min && previous > min)
        {
            dofConstraint.onMinRotationLimitReached?.Invoke();
        }
        if (current >= max && previous < max)
        {
            dofConstraint.onMaxRotationLimitReached?.Invoke();
        }
    }
}
