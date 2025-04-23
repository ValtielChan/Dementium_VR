using UnityEngine;
using System.Collections;

public class Rain : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private Material windowMaterial;
    [SerializeField] private Light[] lightningLights;

    [Header("Animation de pluie")]
    [SerializeField] private float rainFrameDuration = 0.2f;
    [SerializeField] private Vector2[] rainOffsets = new Vector2[] 
    {
        new Vector2(0f, 0f),
        new Vector2(0f, 0.5f),
        new Vector2(0.5f, 0f)
    };
    private int currentRainFrame = 0;
    private float rainTimer = 0f;

    [Header("Éclairs")]
    [SerializeField] private float minTimeBetweenLightning = 5f;
    [SerializeField] private float maxTimeBetweenLightning = 15f;
    [SerializeField] private float lightningDuration = 0.8f;
    [SerializeField] private int minLightningFlickers = 2;
    [SerializeField] private int maxLightningFlickers = 5;
    [SerializeField] private float minFlickerDuration = 0.05f;
    [SerializeField] private float maxFlickerDuration = 0.2f;
    [SerializeField] private float minLightIntensity = 0.6f;
    [SerializeField] private float maxLightIntensity = 1.2f;
    [SerializeField] private Vector2 lightningOffset = new Vector2(0.5f, 0.5f);
    private float lightningTimer = 0f;
    private float nextLightningTime = 0f;
    private bool lightningActive = false;
    private float defaultLightIntensity = 1f;
    private float[] defaultLightIntensities;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (windowMaterial == null)
        {
            // Essayer de récupérer le matériau à partir du Renderer
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                windowMaterial = renderer.material;
            }
            else
            {
                Debug.LogError("Aucun matériau trouvé pour l'animation de pluie.");
            }
        }

        // Initialiser les lumières d'éclairs
        if (lightningLights != null && lightningLights.Length > 0)
        {
            defaultLightIntensities = new float[lightningLights.Length];
            
            for (int i = 0; i < lightningLights.Length; i++)
            {
                if (lightningLights[i] != null)
                {
                    defaultLightIntensities[i] = lightningLights[i].intensity;
                    lightningLights[i].enabled = false;
                }
            }
        }

        // Initialiser le timer pour le premier éclair
        SetNextLightningTime();
    }

    // Update is called once per frame
    void Update()
    {
        if (windowMaterial == null) return;

        // Si pas d'éclair en cours, animer la pluie
        if (!lightningActive)
        {
            // Animation de la pluie
            rainTimer += Time.deltaTime;
            if (rainTimer >= rainFrameDuration)
            {
                rainTimer = 0f;
                currentRainFrame = (currentRainFrame + 1) % rainOffsets.Length;
                windowMaterial.mainTextureOffset = rainOffsets[currentRainFrame];
            }

            // Vérifier si c'est le moment de déclencher un éclair
            lightningTimer += Time.deltaTime;
            if (lightningTimer >= nextLightningTime)
            {
                StartCoroutine(LightningEffect());
            }
        }
    }

    private void SetNextLightningTime()
    {
        nextLightningTime = Random.Range(minTimeBetweenLightning, maxTimeBetweenLightning);
        lightningTimer = 0f;
    }

    private IEnumerator LightningEffect()
    {
        lightningActive = true;
        
        // Nombre aléatoire de clignotements pour cet éclair
        int flickerCount = Random.Range(minLightningFlickers, maxLightningFlickers + 1);
        
        // Première séquence - éclair principal
        yield return FlashLightning(Random.Range(0.1f, 0.25f), Random.Range(0.8f, maxLightIntensity));
        
        // Délai aléatoire après le premier flash
        yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));
        
        // Séquence de clignotements secondaires
        for (int i = 0; i < flickerCount; i++)
        {
            // Durée aléatoire pour ce clignotement
            float onDuration = Random.Range(minFlickerDuration, maxFlickerDuration);
            float offDuration = Random.Range(minFlickerDuration, maxFlickerDuration * 1.5f);
            float intensity = Random.Range(minLightIntensity, maxLightIntensity);
            
            // Afficher l'éclair
            yield return FlashLightning(onDuration, intensity);
            
            // Temps d'extinction entre les clignotements
            yield return new WaitForSeconds(offDuration);
        }
        
        // Parfois un dernier flash faible
        if (Random.value > 0.6f)
        {
            yield return FlashLightning(Random.Range(0.05f, 0.1f), Random.Range(0.3f, 0.6f));
        }
        
        lightningActive = false;
        SetNextLightningTime();
    }
    
    private IEnumerator FlashLightning(float duration, float intensity)
    {
        // Afficher l'éclair
        windowMaterial.mainTextureOffset = lightningOffset;
        
        // Activer toutes les lumières d'éclairs
        if (lightningLights != null && lightningLights.Length > 0)
        {
            for (int i = 0; i < lightningLights.Length; i++)
            {
                if (lightningLights[i] != null)
                {
                    lightningLights[i].enabled = true;
                    lightningLights[i].intensity = defaultLightIntensities[i] * intensity;
                }
            }
        }
        
        yield return new WaitForSeconds(duration);
        
        // Revenir à la frame de pluie
        windowMaterial.mainTextureOffset = rainOffsets[currentRainFrame];
        
        // Désactiver toutes les lumières d'éclairs
        if (lightningLights != null && lightningLights.Length > 0)
        {
            for (int i = 0; i < lightningLights.Length; i++)
            {
                if (lightningLights[i] != null)
                {
                    lightningLights[i].enabled = false;
                }
            }
        }
    }
}
