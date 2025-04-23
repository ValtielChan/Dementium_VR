using System.Collections;
using UnityEngine;

public class InventoryCallback : MonoBehaviour
{
    [SerializeField]
    private Grabbable grabbable;


    [SerializeField]
    private Transform inventoryTransform;

    [SerializeField]
    private float storeDelay;

    private bool stored = true;

    public bool Stored { get => stored; set => stored = value; }

    public Grabbable Grabbable { get => grabbable; set => grabbable = value; }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!grabbable.Grabbed && !stored)
        {
            // Vérifier si l'objet est suffisamment proche pour stockage immédiat
            bool immediate = false;
            float proximityThreshold = 0.5f; // Seuil en mètres (défini en dur)
            
            if (inventoryTransform != null)
            {
                float distanceToInventory = Vector3.Distance(transform.position, inventoryTransform.position);
                immediate = distanceToInventory < proximityThreshold;
            }
            
            StartCoroutine(StoreAsync(immediate));  
        }

        if (grabbable.Grabbed)
        {
            stored = false;
            StopAllCoroutines();
        }
    }

    IEnumerator StoreAsync(bool immediate = false)
    {
        if (!immediate)
        {
            yield return new WaitForSeconds(storeDelay);
        }

        if (!grabbable.Grabbed)
        {
            transform.parent = inventoryTransform;

            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            stored = true;
        }
    }
}
