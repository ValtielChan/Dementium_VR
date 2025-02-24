using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using System;
using Random = UnityEngine.Random;

public class Pistol : Weapon
{
    [Header("Pistol Settings")]
    [SerializeField]
    private float damage;

    [SerializeField]
    private Transform muzzle;

    [SerializeField]
    private Transform ejectionPort;

    [SerializeField]
    private GameObject shellPrefab;

    [SerializeField]
    private GameObject enemyImpactPrefab;

    [SerializeField]
    private GameObject defaultImpactPrefab;

    [SerializeField]
    private Transform slide;

    [SerializeField]
    private float slideBackDistance = 0.1f;

    [SerializeField]
    private float slideBackDuration = 0.05f;

    [SerializeField]
    private float slideForwardDuration = 0.1f;

    [SerializeField]
    private GameObject muzzleFlashEffect;

    [SerializeField]
    private AnimationCurve slideBackCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); 

    [SerializeField]
    private AnimationCurve slideForwardCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);


    private Vector3 slideOriginalPosition; // Position originale de la culasse
    private Coroutine currentSlideCoroutine; // Référence à la coroutine en cours

    void Start()
    {
        muzzleFlashEffect.SetActive(false);

        // Initialisation de la position originale de la culasse
        if (slide != null)
        {
            slideOriginalPosition = slide.localPosition;
        }

    }

    public override void Fire()
    {
        //Debug.Log("Fire");

        //// Vérifie s'il y a une balle dans la chambre
        if (!hasChamberedRound) return;

        //// Simule le recul et joue le son
        SimulateRecoil();
        PlaySound(fireSound);

        StartCoroutine(MuzzleFlash());

        if (shellPrefab != null && ejectionPort != null)
        {
            EjectShell();
        }

        ShootRaycast();
    }

    private IEnumerator MuzzleFlash()
    {
        

        float randomRotation = Random.Range(0f, 360f);
        float randomScale = Random.Range(1f, 1.2f);

        muzzleFlashEffect.transform.localRotation = Quaternion.Euler(0, 0, randomRotation);
        muzzleFlashEffect.transform.localScale = Vector3.one * randomScale;

        muzzleFlashEffect.SetActive(true);
        yield return new WaitForSeconds(0.05f);
        muzzleFlashEffect.SetActive(false);
    }

    private void ShootRaycast()
    {
        if (muzzle == null) return;

        RaycastHit hit;
        if (Physics.Raycast(muzzle.position, muzzle.forward, out hit, Mathf.Infinity))
        {
            GameObject impactEffect = hit.collider.CompareTag("Enemy") ? enemyImpactPrefab : defaultImpactPrefab;

            if (impactEffect != null)
            {
                GameObject impactInstance = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impactInstance, 2f); // Auto-destruction après 2 secondes
            }

            if (hit.collider.CompareTag("Enemy"))
            {
                hit.collider.SendMessage("Damage", damage, SendMessageOptions.DontRequireReceiver);
            }
        }
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
                rb.AddForce(ejectionPort.right * 0.1f, ForceMode.Impulse); // Éjecte la douille
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