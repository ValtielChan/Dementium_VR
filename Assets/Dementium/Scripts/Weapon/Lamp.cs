using UnityEngine;

public class Lamp : MonoBehaviour
{
    [SerializeField]
    private Grabbable grabbable;

    [SerializeField]
    private Transform pivot;

    [SerializeField]
    private Transform reversePivot;

    [Header("Light Settings")]
    [SerializeField] private Light spotlight;
    [SerializeField] private Transform raycastOrigin;
    [SerializeField] private float maxIntensity = 52f;
    [SerializeField] private float maxDistance = 20f;
    [SerializeField] private float minDistance = 0.5f;
    [SerializeField] private float intensityCurve = 1f;
    [SerializeField] private LayerMask raycastLayerMask = -1;

    private void Update()
    {
        AdjustLightIntensity();
    }

    private void AdjustLightIntensity()
    {
        if (spotlight == null || raycastOrigin == null) return;

        // Lance un raycast dans la direction de la lampe
        if (Physics.Raycast(raycastOrigin.position, raycastOrigin.forward, out RaycastHit hit, maxDistance, raycastLayerMask))
        {
            float distance = hit.distance;
            
            // Assure que la distance est au moins égale à minDistance
            distance = Mathf.Max(distance, minDistance);
            
            // Calcule l'intensité en fonction de la distance
            // Plus on est proche, plus on réduit l'intensité
            float intensityFactor = Mathf.Pow(distance / maxDistance, intensityCurve);
            
            // Applique l'intensité calculée
            spotlight.intensity = maxIntensity * intensityFactor;
        }
        else
        {
            // Si rien n'est touché, utiliser l'intensité maximale
            spotlight.intensity = maxIntensity;
        }
    }

    public void SwitchPivot()
    {
        if (grabbable.CustomPivot == pivot) 
            grabbable.CustomPivot = reversePivot;
         else
            grabbable.CustomPivot = pivot;
    }
}
