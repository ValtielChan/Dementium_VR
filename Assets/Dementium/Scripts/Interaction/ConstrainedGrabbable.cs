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

    new void Start()
    {
        base.Start();

        initialLocalPosition = transform.localPosition; // Stockage de la position initiale
        initialLocalRotation = transform.localEulerAngles; // Stockage de la rotation initiale

    }


    new void Update()
    {
        base.Update();

        if (grabbed && target != null)
        {
            // Local translations
            Vector3 targetLocalPosition = transform.parent.InverseTransformPoint(target.position);
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

            // Local rotations
            Quaternion targetLocalRotation = Quaternion.Inverse(transform.parent.rotation) * target.rotation;
            Vector3 targetLocalEulerAngles = targetLocalRotation.eulerAngles;
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

            // Apply constraints
            transform.localPosition = constrainedLocalPosition;
            transform.localEulerAngles = constrainedLocalRotation;

            // Save positions/rotations for events
            previousLocalPosition = constrainedLocalPosition;
            previousLocalRotation = constrainedLocalRotation;

        }
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

    public override void Grab(Grabber grabber)
    {
        base.Grab(grabber);
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

        // Position finale pour éviter les imprécisions
        transform.localPosition = initialLocalPosition;
        transform.localEulerAngles = initialLocalRotation;
    }

}
