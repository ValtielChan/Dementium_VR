using UnityEngine;

public class SnappingMagazineZone : MonoBehaviour
{
    [Header("Snapping Parameters")]
    public Transform startPoint; // Point de d�part de la glissi�re
    public Transform endPoint;   // Point de fin de la glissi�re
    public LayerMask snappingLayer; // Layer des objets qui peuvent snapper
    public float maxSnapDistance = 0.5f; // Distance maximale pour quitter le snapping

    public Weapon weapon;

    // M�thode pour v�rifier si un objet peut snapper
    public bool CanSnap()
    {
        //return ((1 << obj.layer) & snappingLayer) != 0;
        return weapon.CurrentMagazine == null;
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

    public void LoadMagazine(Magazine mag)
    {
        weapon.CurrentMagazine = mag;
    }
}
