using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    public int maxAmmo = 10;
    public int currentAmmo = 0;
    public bool isAutomatic = false;

    public AudioClip fireSound;
    public AudioClip reloadSound;
    public AudioSource audioSource;

    public Transform magazineSlot; // Point où le chargeur s'insère
    public bool hasChamberedRound = false; // Une balle est-elle dans la chambre ?

    protected virtual void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    public abstract void Fire();

    protected void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
