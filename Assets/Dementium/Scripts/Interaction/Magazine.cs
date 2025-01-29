using System;
using UnityEngine;
using UnityEngine.Events;

public class Magazine : Grabbable
{
    public UnityEvent onSnap;

    public Collider collider;

    private SnappingMagazineZone currentSnappingZone; // Référence à la zone actuelle
    private Transform startPoint;            // Point de départ de la glissière (dépend de la zone)
    private Transform endPoint;              // Point de fin de la glissière (dépend de la zone)

    private bool snapped;

    protected new void Update()
    {
        base.Update();

        if (currentSnappingZone != null)
        {
            // Vérifie si on doit sortir du mode snap
            if (grabbed)
            {
                CheckExitSnap();
                Snap();
            }

            // Applique les contraintes dynamiques si on est dans une zone de snapping
            ApplySliderConstraint();

            
        }
    }

    protected override void UpdateRigidbody()
    {
        if (rb && !snapped)
            rb.isKinematic = grabbed;
    }

    private void Snap()
    {
        if (endPoint != null && !snapped)
        {
            if (Vector3.Distance(transform.position, endPoint.position) < 0.055f)
            {
                Debug.Log("SNAP !");

                snapped = true;
                grabbable = false; 

                transform.position = endPoint.position;
                transform.parent = currentSnappingZone.transform;

                currentSnappingZone.LoadMagazine(this);

                onSnap?.Invoke();
                Release(currentGrabber);

                collider.isTrigger = true;
                rb.isKinematic = true;

            }
        }
    }

    private void CheckExitSnap()
    {
        if (currentSnappingZone == null) return;

        // Calcule la distance entre le grabber et la zone
        float distanceToZone = Vector3.Distance(target.position, currentSnappingZone.transform.position);

        // Si on dépasse la distance maximale, on quitte le mode snap
        if (distanceToZone > currentSnappingZone.maxSnapDistance)
        {
            currentSnappingZone = null;
            startPoint = null;
            endPoint = null;
        }
    }

    public void Eject()
    {
        // Simule la chute du chargeur
        transform.SetParent(null);
        if (rb != null)
        {
            rb.isKinematic = false;
            //rb.AddForce(Vector3.down * 2f, ForceMode.Impulse);
        }

        snapped = false;
        grabbable = true;
    }

    private void ApplySliderConstraint()
    {
        if (startPoint == null || endPoint == null) return;

        // Calcule la direction de la glissière
        Vector3 slideDirection = (endPoint.position - startPoint.position).normalized;

        // Projette la position actuelle sur la glissière
        Vector3 startToCurrent = transform.position - startPoint.position;
        float projectedDistance = Vector3.Dot(startToCurrent, slideDirection);

        // Contrainte de translation entre les limites (0 et la longueur de la glissière)
        float slideLength = Vector3.Distance(startPoint.position, endPoint.position);
        float clampedDistance = Mathf.Clamp(projectedDistance, 0, slideLength);

        // Recalcule la position contrainte
        Vector3 constrainedPosition = startPoint.position + slideDirection * clampedDistance;

        // Applique la position contrainte
        transform.position = constrainedPosition;

        // Applique la rotation de l'endPoint
        transform.rotation = endPoint.rotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        // Vérifie si l'objet entre dans une zone de snapping
        SnappingMagazineZone snappingZone = other.GetComponent<SnappingMagazineZone>();
        if (snappingZone != null && snappingZone.CanSnap(gameObject))
        {
            // Stocke la référence à la zone et configure les points
            currentSnappingZone = snappingZone;
            startPoint = snappingZone.GetStartPoint();
            endPoint = snappingZone.GetEndPoint();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);

        // Vérifie si l'objet sort de la zone de snapping
        if (currentSnappingZone != null && other.GetComponent<SnappingMagazineZone>() == currentSnappingZone)
        {
            // Réinitialise la référence
            currentSnappingZone = null;
            startPoint = null;
            endPoint = null;
        }
    }
}
