using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PhysicalGrabbableNoSpring : Grabbable
{
    // On conserve la possibilité d'utiliser un customPivot hérité de Grabbable.

    // Dans ce mode, on laisse le Rigidbody non cinématique lorsque l'objet est saisi
    protected override void UpdateRigidbody()
    {
        if (rb)
        {
            // Si l'objet est saisi, on le laisse non cinématique pour que les collisions soient prises en compte.
            rb.isKinematic = false;
            // Désactiver la gravité pendant que l'objet est saisi
            rb.useGravity = !grabbed;
        }
    }

    // On n'implémente pas UpdateTransform car le suivi se fera dans FixedUpdate.
    protected override void UpdateTransform()
    {
        // Intentionnellement vide pour laisser la physique s'en charger dans FixedUpdate.
    }

    void Update()
    {
        if (grabbed && target != null)
        {
            // Calcul de la position et de la rotation désirées en fonction de la cible.
            Vector3 desiredPosition;
            Quaternion desiredRotation;
            if (customPivot != null)
            {
                // Si un pivot personnalisé est défini, on calcule la transformation
                desiredRotation = target.rotation * Quaternion.Inverse(customPivot.localRotation);
                desiredPosition = target.position - desiredRotation * customPivot.localPosition;
            }
            else
            {
                desiredPosition = target.position;
                desiredRotation = target.rotation;
            }

            // Utilisation de MovePosition/MoveRotation pour déplacer l'objet de façon "directe"
            rb.MovePosition(desiredPosition);
            rb.MoveRotation(desiredRotation);
        }
    }

    // Lors de la saisie, on réinitialise les vitesses pour éviter les effets parasites.
    public override void Grab(Grabber grabber)
    {
        base.Grab(grabber);
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            // Désactiver la gravité pendant que l'objet est saisi
            rb.useGravity = false;
        }
    }

    // Lors de la libération, on réactive la gravité si nécessaire.
    public override void Release(Grabber grabber)
    {
        base.Release(grabber);
        if (rb != null)
        {
            // Réactiver la gravité lorsque l'objet est libéré
            rb.useGravity = true;
        }
    }
}
