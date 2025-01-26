using UnityEngine;

public class Magazine : MonoBehaviour
{
    private int ammo = 10;
    private bool isSnapped = false;

    public void Eject()
    {
        // Simule la chute du chargeur
        transform.SetParent(null);
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.AddForce(Vector3.down * 2f, ForceMode.Impulse);
        }

        isSnapped = false;
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.CompareTag("Weapon"))
    //    {
    //        Weapon weapon = other.GetComponent<Weapon>();
    //        if (weapon != null && !isSnapped)
    //        {
    //            SnapToWeapon(weapon);
    //        }
    //    }
    //}

    //private void SnapToWeapon(Weapon weapon)
    //{
    //    transform.position = weapon.magazineSlot.position;
    //    transform.rotation = weapon.magazineSlot.rotation;
    //    transform.SetParent(weapon.transform);

    //    isSnapped = true;
    //}
}
