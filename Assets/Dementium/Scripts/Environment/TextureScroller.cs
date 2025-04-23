using UnityEngine;

public class TextureScroller : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private Material targetMaterial;
    
    [Header("Paramètres de défilement")]
    [SerializeField] private float scrollSpeed = 0.5f;
    [SerializeField] private bool scrollVertical = true;
    [SerializeField] private bool scrollHorizontal = false;
    [SerializeField] private Vector2 scrollDirection = Vector2.up; // Par défaut vers le haut
    
    private Vector2 currentOffset = Vector2.zero;
    
    void Start()
    {
        if (targetMaterial == null)
        {
            // Essayer de récupérer le matériau à partir du Renderer
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                targetMaterial = renderer.material;
                // Sauvegarder l'offset initial
                currentOffset = targetMaterial.mainTextureOffset;
            }
            else
            {
                Debug.LogError("Aucun matériau trouvé pour le défilement de texture.");
            }
        }
        else
        {
            // Sauvegarder l'offset initial
            currentOffset = targetMaterial.mainTextureOffset;
        }
        
        // Si les deux options sont activées, utiliser la direction personnalisée
        if (scrollVertical && scrollHorizontal)
        {
            // On laisse la direction personnalisée telle quelle
        }
        // Sinon, configurer la direction en fonction des options
        else
        {
            scrollDirection = Vector2.zero;
            
            if (scrollVertical)
                scrollDirection.y = 1f;
                
            if (scrollHorizontal)
                scrollDirection.x = 1f;
        }
        
        // Normaliser la direction pour que la vitesse soit cohérente
        if (scrollDirection.magnitude > 0)
            scrollDirection.Normalize();
    }
    
    void Update()
    {
        if (targetMaterial == null)
            return;
            
        // Calculer le déplacement pour ce frame
        currentOffset += scrollDirection * scrollSpeed * Time.deltaTime;
        
        // Gérer le wrapping (pour que les valeurs restent entre 0 et 1)
        currentOffset.x = currentOffset.x % 1f;
        currentOffset.y = currentOffset.y % 1f;
        
        // Appliquer l'offset au matériau
        targetMaterial.mainTextureOffset = currentOffset;
    }
} 