using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class Pistol : Weapon
{
    public Transform muzzle;
    public GameObject bulletPrefab;
    public Transform ejectionPort;
    public GameObject shellPrefab;
    public Transform slide; // Culasse
    public float slideBackDistance = 0.1f;
    public float slideSpeed = 0.05f;

    private XRGrabInteractable grabInteractable;
    private Magazine currentMagazine;
    private bool isReloading = false;

    void Start()
    {
        currentAmmo = 0; // Pas de munitions au départ
        hasChamberedRound = false;

        grabInteractable = GetComponent<XRGrabInteractable>();
        grabInteractable.selectExited.AddListener(OnDeselect);
    }

    public override void Fire()
    {
        if (isReloading || !hasChamberedRound) return;

        hasChamberedRound = false; // Décharge la balle de la chambre
        SimulateRecoil();
        PlaySound(fireSound);

        if (bulletPrefab != null && muzzle != null)
        {
            SpawnBullet();
        }

        if (shellPrefab != null && ejectionPort != null)
        {
            EjectShell();
        }

        if (currentAmmo > 0 && currentMagazine != null)
        {
            currentAmmo--; // Diminue les munitions du chargeur
            hasChamberedRound = true; // Charge automatiquement une nouvelle balle
        }
    }

    private void SimulateRecoil()
    {
        if (slide == null) return;

        StartCoroutine(RecoilAnimation());
    }

    private IEnumerator RecoilAnimation()
    {
        if (slide == null) yield break;

        Vector3 originalPosition = slide.localPosition;
        Vector3 recoilPosition = originalPosition + Vector3.back * slideBackDistance;

        float elapsedTime = 0f;

        while (elapsedTime < slideSpeed)
        {
            slide.localPosition = Vector3.Lerp(originalPosition, recoilPosition, elapsedTime / slideSpeed);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        slide.localPosition = recoilPosition;

        elapsedTime = 0f;

        while (elapsedTime < slideSpeed)
        {
            slide.localPosition = Vector3.Lerp(recoilPosition, originalPosition, elapsedTime / slideSpeed);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        slide.localPosition = originalPosition;
    }

    public override void Reload()
    {
        // Simule l'insertion d'un chargeur
        if (currentMagazine != null)
        {
            currentAmmo = currentMagazine.AmmoCount;
        }
    }

    public override void EjectMagazine()
    {
        if (currentMagazine == null) return;

        currentMagazine.Eject();
        currentMagazine = null; // Libère la référence au chargeur
    }

    private void SpawnBullet()
    {
        GameObject bullet = Instantiate(bulletPrefab, muzzle.position, muzzle.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(muzzle.forward * 500f);
        }
    }

    private void EjectShell()
    {
        GameObject shell = Instantiate(shellPrefab, ejectionPort.position, ejectionPort.rotation);
        Rigidbody rb = shell.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(ejectionPort.right * 100f, ForceMode.Impulse);
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
