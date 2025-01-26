using UnityEngine;

public class SnappingGrabbable : Grabbable
{
    private SnappingZone currentSnappingZone; // Référence à la zone actuelle
    private Transform startPoint;            // Point de départ de la glissière (dépend de la zone)
    private Transform endPoint;              // Point de fin de la glissière (dépend de la zone)

    protected new void Update()
    {
        base.Update();

        if (grabbed && currentSnappingZone != null)
        {
            // Applique les contraintes dynamiques si on est dans une zone de snapping
            ApplySliderConstraint();
        }
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

        // Mélange avec le mouvement du grabber
        transform.position = Vector3.Lerp(transform.position, constrainedPosition, Time.deltaTime * 15f);
    }

    private void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        // Vérifie si l'objet entre dans une zone de snapping
        SnappingZone snappingZone = other.GetComponent<SnappingZone>();
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
        if (currentSnappingZone != null && other.GetComponent<SnappingZone>() == currentSnappingZone)
        {
            // Réinitialise la référence
            currentSnappingZone = null;
            startPoint = null;
            endPoint = null;
        }
    }
}
