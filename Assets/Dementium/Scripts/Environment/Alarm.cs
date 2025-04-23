using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Script qui fait varier progressivement la couleur d'un matériau et l'intensité de lumières de manière synchronisée.
/// </summary>
public class Alarm : MonoBehaviour
{
    [Header("Références")]
    [Tooltip("Le matériau à modifier")]
    public Material targetMaterial;
    
    [Tooltip("Les lumières dont l'intensité variera")]
    public List<Light> targetLights = new List<Light>();

    [Header("Paramètres de couleur")]
    [Tooltip("La première couleur (départ)")]
    public Color firstColor = Color.white;
    
    [Tooltip("La deuxième couleur (arrivée)")]
    public Color secondColor = new Color(0.2f, 0.2f, 0.2f); // Gris foncé par défaut
    
    [Tooltip("Le nom de la propriété du shader à modifier (par défaut: _BaseColor)")]
    public string colorPropertyName = "_BaseColor";

    [Header("Paramètres de lumière")]
    [Tooltip("Intensité minimale des lumières")]
    [Range(0f, 8f)]
    public float minLightIntensity = 0.2f;
    
    [Tooltip("Intensité maximale des lumières")]
    [Range(0f, 8f)]
    public float maxLightIntensity = 1.0f;

    [Header("Paramètres de transition")]
    [Tooltip("Vitesse de transition entre les états")]
    [Range(0.1f, 10f)]
    public float transitionSpeed = 1.0f;
    
    [Tooltip("Courbe de transition pour personnaliser l'effet")]
    public AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    // Variables privées pour le fonctionnement interne
    private float transitionProgress = 0f;
    private bool transitionToSecondColor = true;
    private Dictionary<Light, float> originalIntensities = new Dictionary<Light, float>();

    private void Start()
    {
        // Vérifier si un matériau a été assigné
        if (targetMaterial == null)
        {
            // Essayer de trouver un Renderer sur l'objet et utiliser son matériau
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                targetMaterial = renderer.material;
                Debug.Log("Aucun matériau n'a été directement assigné. Utilisation du matériau du Renderer de cet objet.");
            }
            else
            {
                Debug.LogWarning("Aucun matériau n'a été assigné. La partie animation de couleur sera désactivée.");
            }
        }
        
        // Vérifier la propriété de couleur si un matériau est présent
        if (targetMaterial != null && !targetMaterial.HasProperty(colorPropertyName))
        {
            Debug.LogWarning("Le matériau ne possède pas la propriété '" + colorPropertyName + "'. Vérifiez le nom de la propriété.");
        }
        
        // Si aucune lumière n'est spécifiée, essayer de trouver des lumières dans les enfants
        if (targetLights.Count == 0)
        {
            Light[] childLights = GetComponentsInChildren<Light>();
            if (childLights.Length > 0)
            {
                targetLights.AddRange(childLights);
                Debug.Log("Aucune lumière n'a été directement assignée. Utilisation des lumières trouvées dans les enfants de cet objet.");
            }
            else
            {
                Debug.LogWarning("Aucune lumière n'a été assignée. La partie animation de lumière sera désactivée.");
            }
        }
        
        // Sauvegarder les intensités originales des lumières
        foreach (Light light in targetLights)
        {
            if (light != null)
            {
                originalIntensities[light] = light.intensity;
            }
        }
        
        // Initialiser les états
        if (targetMaterial != null)
        {
            targetMaterial.SetColor(colorPropertyName, firstColor);
        }
        
        UpdateLightIntensities(0f); // Initialiser les lumières à l'intensité minimale
    }

    private void Update()
    {
        // Calculer la progression de la transition
        if (transitionToSecondColor)
        {
            transitionProgress += Time.deltaTime * transitionSpeed;
            if (transitionProgress >= 1f)
            {
                transitionProgress = 1f;
                transitionToSecondColor = false;
            }
        }
        else
        {
            transitionProgress -= Time.deltaTime * transitionSpeed;
            if (transitionProgress <= 0f)
            {
                transitionProgress = 0f;
                transitionToSecondColor = true;
            }
        }

        // Appliquer la courbe de transition
        float curvedProgress = transitionCurve.Evaluate(transitionProgress);
        
        // Mettre à jour la couleur du matériau
        if (targetMaterial != null)
        {
            Color currentColor = Color.Lerp(firstColor, secondColor, curvedProgress);
            targetMaterial.SetColor(colorPropertyName, currentColor);
        }
        
        // Mettre à jour l'intensité des lumières
        UpdateLightIntensities(curvedProgress);
    }
    
    private void UpdateLightIntensities(float progress)
    {
        foreach (Light light in targetLights)
        {
            if (light != null)
            {
                light.intensity = Mathf.Lerp(minLightIntensity, maxLightIntensity, progress);
            }
        }
    }

    // Réinitialiser tout lors de la destruction du composant
    private void OnDestroy()
    {
        ResetToOriginalState();
    }

    // Réinitialiser tout lorsque le script est désactivé
    private void OnDisable()
    {
        ResetToOriginalState();
    }
    
    private void ResetToOriginalState()
    {
        // Restaurer la couleur d'origine du matériau
        if (targetMaterial != null)
        {
            targetMaterial.SetColor(colorPropertyName, firstColor);
        }
        
        // Restaurer les intensités d'origine des lumières
        foreach (Light light in targetLights)
        {
            if (light != null && originalIntensities.ContainsKey(light))
            {
                light.intensity = originalIntensities[light];
            }
        }
    }
}