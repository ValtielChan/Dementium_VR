using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class Pistol : Weapon
{
    [Header("Pistol Settings")]
    public Transform muzzle; // Point de sortie des balles
    public Transform ejectionPort; // Point d'�jection des douilles
    public GameObject shellPrefab; // Pr�fabriqu� de la douille
    public Transform slide; // Partie mobile de l'arme (coulisse)
    public float slideBackDistance = 0.1f; // Distance de recul de la culasse
    public float slideBackDuration = 0.05f; // Dur�e du recul
    public float slideForwardDuration = 0.1f; // Dur�e du retour avant
    public AnimationCurve slideBackCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // Courbe de recul
    public AnimationCurve slideForwardCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // Courbe de retour avant


    private Vector3 slideOriginalPosition; // Position originale de la culasse
    private Coroutine currentSlideCoroutine; // R�f�rence � la coroutine en cours

    void Start()
    {
        // Initialisation de la position originale de la culasse
        if (slide != null)
        {
            slideOriginalPosition = slide.localPosition;
        }

    }

    public override void Fire()
    {
        //Debug.Log("Fire");

        //// V�rifie s'il y a une balle dans la chambre
        if (!hasChamberedRound) return;

        //// Simule le recul et joue le son
        SimulateRecoil();
        PlaySound(fireSound);

        //// Fait appara�tre une balle et une douille
        //if (bulletPrefab != null && muzzle != null)
        //{
        //    SpawnBullet();
        //}

        if (shellPrefab != null && ejectionPort != null)
        {
            EjectShell();
        }
    }

    private void SimulateRecoil()
    {
        if (slide == null) return;

        // Arr�te l'animation en cours avant d'en commencer une nouvelle
        if (currentSlideCoroutine != null)
        {
            StopCoroutine(currentSlideCoroutine);
            slide.localPosition = slideOriginalPosition; // R�initialise imm�diatement
        }

        // D�marre la nouvelle animation
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
        currentMagazine = null; // Lib�re la r�f�rence au chargeur
    }

 

    public void EjectShell()
    {
        Debug.Log("Eject shell");

        // Animation
        if (hasChamberedRound)
        {
            GameObject shell = Instantiate(shellPrefab, ejectionPort.position, ejectionPort.rotation);
            Rigidbody rb = shell.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(ejectionPort.right * 0.1f, ForceMode.Impulse); // �jecte la douille
            }
        }

        // Data
        hasChamberedRound = false;

        if (currentMagazine && currentMagazine.ammo > 0)
        {
            hasChamberedRound = true;
            currentMagazine.ammo -= 1;
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