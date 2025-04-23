using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.STP;

public class TeleportHandler : MonoBehaviour
{
    [SerializeField] private Transform teleportPointA;
    [SerializeField] private Transform teleportPointB;
    private Fade fadeEffect;

    [SerializeField] private HapticConfig hoverHapticConfig;

    [SerializeField] private GameObject roomA;
    [SerializeField] private GameObject roomB;

    [SerializeField] private GameObject colliderA;
    [SerializeField] private GameObject colliderB;

    [Header("Audio")]
    [SerializeField] private AudioSource teleportAudioSource;
    [SerializeField] private AudioClip teleportSound;

    private bool isAtPointA = true;
    private Grabber currentGrabber; // Référence au Grabber actif

    private GameObject player;

    public UnityEvent onTeleportStart;
    public UnityEvent onTeleportComplete;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        fadeEffect = FindAnyObjectByType<Fade>();

        if (!teleportPointA || !teleportPointB || !fadeEffect || !roomA || !roomB)
        {
            Debug.LogError("TeleportHandler mal configuré. Vérifie les références.");
            enabled = false;
            return;
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Teleport zone trigger enter");

        if (currentGrabber != null) return; // Un Grabber est déjà actif, on ignore


        Grabber grabber = other.GetComponent<Grabber>();
        if (grabber && !grabber.Grabbing) // Vérifie qu'il n'est pas déjà en train de grab
        {
            currentGrabber = grabber;
            currentGrabber.gameObject.SendMessage("StartHaptic", hoverHapticConfig);
            grabber.triggerAction.action.started += TriggerTeleport; // On écoute l'événement d'activation

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

        currentGrabber.triggerAction.action.started -= TriggerTeleport;
        currentGrabber = null;

        onTeleportStart?.Invoke();
        fadeEffect.StartFadeOut(0.5f);

        // Jouer le son de téléportation
        if (teleportAudioSource != null && teleportSound != null)
        {
            teleportAudioSource.PlayOneShot(teleportSound);
        }

        // Délais pour la transition
        Invoke(nameof(TeleportPlayer), 0.5f);
    }

    private void TeleportPlayer()
    {
        if (isAtPointA){
            roomB.SetActive(true);
            colliderB.SetActive(true);
        }
        else{
            roomA.SetActive(true);
            colliderA.SetActive(true);
        }

        player.transform.position = isAtPointA ? teleportPointB.position : teleportPointA.position;

        isAtPointA = !isAtPointA;
        fadeEffect.StartFadeIn(0.5f);
        onTeleportComplete?.Invoke();

        if (!isAtPointA){
            roomA.SetActive(false);
            colliderA.SetActive(false);
        }
        else{
            roomB.SetActive(false);
            colliderB.SetActive(false);
        }
        
    }

    private void ActivateCollider(Collider col, bool state)
    {
        col.enabled = state;
    }
}
