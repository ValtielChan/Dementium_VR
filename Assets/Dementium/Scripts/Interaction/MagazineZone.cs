using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class MagazineZone : Interactable
{
    public GameObject magazinePrefab;

    protected override void OnInteractionStarted(InputAction.CallbackContext context)
    {
        base.OnInteractionStarted(context);

        Debug.Log("Instantiate Magazine");

        Grabbable magazine = Instantiate(magazinePrefab).GetComponent<Grabbable>();

        magazine.Grab(currentInteractor);
    }
}
