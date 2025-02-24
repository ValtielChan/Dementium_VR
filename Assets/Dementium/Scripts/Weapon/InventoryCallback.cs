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

    bool stored = true;

    public bool Stored { get => stored; set => stored = value; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if (!grabbable.Grabbed && !stored)
        {
            StartCoroutine(StoreAsync());  
        }

        if (grabbable.Grabbed)
        {
            stored = false;
            StopAllCoroutines();
        }
    }

    IEnumerator StoreAsync()
    {
        yield return new WaitForSeconds(storeDelay);

        if (!grabbable.Grabbed)
        {
            transform.parent = inventoryTransform;

            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            stored = true;
        }

    }
}
