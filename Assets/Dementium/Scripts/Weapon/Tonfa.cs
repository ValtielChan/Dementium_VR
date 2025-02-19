using UnityEngine;

public class Tonfa : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Collide with {collision.gameObject.name}");
        if (collision.gameObject.tag == "Enemy")
        {
            collision.gameObject.SendMessage("Damage", 1);
        }
    }
}
