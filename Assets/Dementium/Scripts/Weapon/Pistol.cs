using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class Pistol : Weapon
{
    [Header("Pistol Settings")]
    public Transform muzzle; // Point de sortie des balles
    public GameObject bulletPrefab; // Préfabriqué de la balle
    public Transform ejectionPort; // Point d'éjection des douilles
    public GameObject shellPrefab; // Préfabriqué de la douille
    public Transform slide; // Partie mobile de l'arme (coulisse)
    public float slideBackDistance = 0.1f; // Distance de recul de la culasse
    public float slideBackDuration = 0.05f; // Durée du recul
    public float slideForwardDuration = 0.1f; // Durée du retour avant
    public AnimationCurve slideBackCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // Courbe de recul
    public AnimationCurve slideForwardCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // Courbe de retour avant


    private Vector3 slideOriginalPosition; // Position originale de la culasse
    private Coroutine currentSlideCoroutine; // Référence à la coroutine en cours

    void Start()
    {
        // Initialisation de la position originale de la culasse
        if (slide != null)
        {
            slideOriginalPosition = slide.localPosition;
        }

        // Initialisation des munitions
        currentAmmo = 0; // Pas de munitions au départ
        hasChamberedRound = false; // Pas de balle dans la chambre
    }

    public override void Fire()
    {
        //Debug.Log("Fire");

        //// Vérifie s'il y a une balle dans la chambre
        //if (!hasChamberedRound) return;

        //// Décharge la balle de la chambre
        //hasChamberedRound = false;

        //// Simule le recul et joue le son
        SimulateRecoil();
        PlaySound(fireSound);

        //// Fait apparaître une balle et une douille
        //if (bulletPrefab != null && muzzle != null)
        //{
        //    SpawnBullet();
        //}

        //if (shellPrefab != null && ejectionPort != null)
        //{
        //    EjectShell();
        //}

        //// Recharge une nouvelle balle si le chargeur n'est pas vide
        //if (currentAmmo > 0 && currentMagazine != null)
        //{
        //    currentAmmo--; // Diminue les munitions du chargeur
        //    hasChamberedRound = true; // Charge automatiquement une nouvelle balle
        //}
    }

    private void SimulateRecoil()
    {
        if (slide == null) return;

        // Arrête l'animation en cours avant d'en commencer une nouvelle
        if (currentSlideCoroutine != null)
        {
            StopCoroutine(currentSlideCoroutine);
            slide.localPosition = slideOriginalPosition; // Réinitialise immédiatement
        }

        // Démarre la nouvelle animation
        currentSlideCoroutine = StartCoroutine(BlowBackAnimation());
    }

    private IEnumerator BlowBackAnimation()
    {
        Vector3 recoilPosition = slideOriginalPosition + Vector3.back * slideBackDistance;
        float elapsedTime = 0f;

        // Phase de recul
        while (elapsedTime < slideBackDuration)
        {
            float t = slideBackCurve.Evaluate(elapsedTime / slideBackDuration);
            slide.localPosition = Vector3.Lerp(slideOriginalPosition, recoilPosition, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        slide.localPosition = recoilPosition;
        elapsedTime = 0f;

        // Phase de retour avant
        while (elapsedTime < slideForwardDuration)
        {
            float t = slideForwardCurve.Evaluate(elapsedTime / slideForwardDuration);
            slide.localPosition = Vector3.Lerp(recoilPosition, slideOriginalPosition, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        slide.localPosition = slideOriginalPosition;
        currentSlideCoroutine = null;
    }

    [ContextMenu("Debug Eject Magazine")]
    public void EjectMagazine()
    {
        if (currentMagazine == null) return;

        Debug.Log("Eject Magazine from Pistol");

        currentMagazine.Eject();
        currentMagazine = null; // Libère la référence au chargeur
    }

    private void SpawnBullet()
    {
        GameObject bullet = Instantiate(bulletPrefab, muzzle.position, muzzle.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(muzzle.forward * 500f); // Applique une force à la balle
        }
    }

    private void EjectShell()
    {
        GameObject shell = Instantiate(shellPrefab, ejectionPort.position, ejectionPort.rotation);
        Rigidbody rb = shell.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(ejectionPort.right * 100f, ForceMode.Impulse); // Éjecte la douille
        }
    }

    private void OnDeselect(SelectExitEventArgs args)
    {
        if (args.interactorObject is XRDirectInteractor)
        {
            if (Input.GetButtonDown("XRI_Right_PrimaryButton")) // Bouton A
            {
                EjectMagazine();
            }
        }
    }
}