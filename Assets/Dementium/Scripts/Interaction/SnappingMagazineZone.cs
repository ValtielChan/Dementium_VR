using UnityEngine;

public class SnappingMagazineZone : MonoBehaviour
{
    [Header("Snapping Parameters")]
    public Transform startPoint; // Point de départ de la glissière
    public Transform endPoint;   // Point de fin de la glissière
    public LayerMask snappingLayer; // Layer des objets qui peuvent snapper
    public float maxSnapDistance = 0.5f; // Distance maximale pour quitter le snapping

    public Weapon weapon;

    // Méthode pour vérifier si un objet peut snapper
    public bool CanSnap()
    {
        //return ((1 << obj.layer) & snappingLayer) != 0;
        return weapon.CurrentMagazine == null;
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

    public void LoadMagazine(Magazine mag)
    {
        weapon.CurrentMagazine = mag;
    }
}
