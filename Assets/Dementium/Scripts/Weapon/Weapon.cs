using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    public bool isAutomatic = false;

    public AudioClip fireSound;
    public AudioSource audioSource;

    public bool hasChamberedRound = true;

    protected Magazine currentMagazine;

    public Magazine CurrentMagazine { get => currentMagazine; set => currentMagazine = value; }

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
