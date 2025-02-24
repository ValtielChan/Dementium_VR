using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.STP;

public class TeleportHandler : MonoBehaviour
{
    [SerializeField] private Transform teleportPointA;
    [SerializeField] private Transform teleportPointB;
    [SerializeField] private Fade fadeEffect;

    [SerializeField] private HapticConfig hoverHapticConfig;

    private bool isAtPointA = true;
    private Grabber currentGrabber; // R�f�rence au Grabber actif

    private GameObject player;

    public UnityEvent onTeleportStart;
    public UnityEvent onTeleportComplete;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        if (!teleportPointA || !teleportPointB || !fadeEffect)
        {
            Debug.LogError("TeleportHandler mal configur�. V�rifie les r�f�rences.");
            enabled = false;
            return;
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Teleport zone trigger enter");

        if (currentGrabber != null) return; // Un Grabber est d�j� actif, on ignore


        Grabber grabber = other.GetComponent<Grabber>();
        if (grabber && !grabber.Grabbing) // V�rifie qu'il n'est pas d�j� en train de grab
        {
            currentGrabber = grabber;
            currentGrabber.gameObject.SendMessage("StartHaptic", hoverHapticConfig);
            grabber.triggerAction.action.started += TriggerTeleport; // On �coute l'�v�nement d'activation

        }
    }

    private void OnTriggerExit(Collider other)
    {
        Grabber grabber = other.GetComponent<Grabber>();
        if (grabber && grabber == currentGrabber)
        {
            grabber.triggerAction.action.started -= TriggerTeleport;
            currentGrabber = null;
        }
    }

    private void TriggerTeleport(InputAction.CallbackContext context)
    {
        if (currentGrabber == null) return;

     

        onTeleportStart?.Invoke();
        fadeEffect.StartFadeOut(0.5f);

        // D�lais pour la transition
        Invoke(nameof(TeleportPlayer), 0.5f);
    }

    private void TeleportPlayer()
    {

        player.transform.position = isAtPointA ? teleportPointB.position : teleportPointA.position;

        isAtPointA = !isAtPointA;
        fadeEffect.StartFadeIn(0.5f);
        onTeleportComplete?.Invoke();
    }

    private void ActivateCollider(Collider col, bool state)
    {
        col.enabled = state;
    }
}
