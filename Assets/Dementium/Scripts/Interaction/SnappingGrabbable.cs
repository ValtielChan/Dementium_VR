using UnityEngine;

public class SnappingGrabbable : Grabbable
{
    private SnappingZone currentSnappingZone; // R�f�rence � la zone actuelle
    private Transform startPoint;            // Point de d�part de la glissi�re (d�pend de la zone)
    private Transform endPoint;              // Point de fin de la glissi�re (d�pend de la zone)

    protected new void Update()
    {
        base.Update();

        if (grabbed && currentSnappingZone != null)
        {
            // V�rifie si on doit sortir du mode snap
            CheckExitSnap();

            // Applique les contraintes dynamiques si on est dans une zone de snapping
            ApplySliderConstraint();
        }
    }

    private void CheckExitSnap()
    {
        if (currentSnappingZone == null) return;

        // Calcule la distance entre le grabber et la zone
        float distanceToZone = Vector3.Distance(target.position, currentSnappingZone.transform.position);

        // Si on d�passe la distance maximale, on quitte le mode snap
        if (distanceToZone > currentSnappingZone.maxSnapDistance)
        {
            currentSnappingZone = null;
            startPoint = null;
            endPoint = null;
        }
    }

    private void ApplySliderConstraint()
    {
        if (startPoint == null || endPoint == null) return;

        // Calcule la direction de la glissi�re
        Vector3 slideDirection = (endPoint.position - startPoint.position).normalized;

        // Projette la position actuelle sur la glissi�re
        Vector3 startToCurrent = transform.position - startPoint.position;
        float projectedDistance = Vector3.Dot(startToCurrent, slideDirection);

        // Contrainte de translation entre les limites (0 et la longueur de la glissi�re)
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

        // V�rifie si l'objet entre dans une zone de snapping
        SnappingZone snappingZone = other.GetComponent<SnappingZone>();
        if (snappingZone != null && snappingZone.CanSnap(gameObject))
        {
            // Stocke la r�f�rence � la zone et configure les points
            currentSnappingZone = snappingZone;
            startPoint = snappingZone.GetStartPoint();
            endPoint = snappingZone.GetEndPoint();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);

        // V�rifie si l'objet sort de la zone de snapping
        if (currentSnappingZone != null && other.GetComponent<SnappingZone>() == currentSnappingZone)
        {
            // R�initialise la r�f�rence
            currentSnappingZone = null;
            startPoint = null;
            endPoint = null;
        }
    }
}
