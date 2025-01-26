using UnityEngine;

public class SnappingZone : MonoBehaviour
{
    [Header("Snapping Parameters")]
    public Transform startPoint; // Point de départ de la glissière
    public Transform endPoint;   // Point de fin de la glissière
    public LayerMask snappingLayer; // Layer des objets qui peuvent snapper

    // Méthode pour vérifier si un objet peut snapper
    public bool CanSnap(GameObject obj)
    {
        return ((1 << obj.layer) & snappingLayer) != 0;
    }

    // Retourne les points de la glissière
    public Transform GetStartPoint()
    {
        return startPoint;
    }

    public Transform GetEndPoint()
    {
        return endPoint;
    }
}
