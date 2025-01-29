using UnityEngine;

public class MagazineSnapping : MonoBehaviour
{
    private SnappingMagazineZone currentSnappingZone; // R�f�rence � la zone actuelle
    private Rigidbody rb;
    private bool isGrabbed = false; // Indique si l'objet est actuellement grab�
    private Transform startPoint;   // Point de d�part de la glissi�re (d�pend de la zone)
    private Transform endPoint;     // Point de fin de la glissi�re (d�pend de la zone)

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (currentSnappingZone != null)
        {
            // Applique la contrainte si une zone est active
            ApplySliderConstraint();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // V�rifie si l'objet entre dans une zone de snapping
        SnappingMagazineZone snappingZone = other.GetComponent<SnappingMagazineZone>();
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
        // V�rifie si l'objet sort de la zone de snapping
        if (currentSnappingZone != null && other.GetComponent<SnappingMagazineZone>() == currentSnappingZone)
        {
            // R�initialise la r�f�rence
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
        if (isGrabbed)
        {
            // Si grab�, m�lange le snapping avec le mouvement du grabber
            transform.position = Vector3.Lerp(transform.position, constrainedPosition, Time.deltaTime * 15f);
        }
        else
        {
            // Si pas grab�, verrouille directement
            transform.position = constrainedPosition;
        }
    }

    public void OnGrabbed()
    {
        // Appel� lorsque l'objet est grab�
        isGrabbed = true;
        rb.isKinematic = true;
    }

    public void OnReleased()
    {
        // Appel� lorsque l'objet est rel�ch�
        isGrabbed = false;
        rb.isKinematic = false;
    }
}
