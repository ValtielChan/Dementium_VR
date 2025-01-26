using UnityEngine;

public class SnappingZone : MonoBehaviour
{
    [Header("Snapping Parameters")]
    public Transform startPoint; // Point de d�part de la glissi�re
    public Transform endPoint;   // Point de fin de la glissi�re
    public LayerMask snappingLayer; // Layer des objets qui peuvent snapper

    // M�thode pour v�rifier si un objet peut snapper
    public bool CanSnap(GameObject obj)
    {
        return ((1 << obj.layer) & snappingLayer) != 0;
    }

    // Retourne les points de la glissi�re
    public Transform GetStartPoint()
    {
        return startPoint;
    }

    public Transform GetEndPoint()
    {
        return endPoint;
    }
}
