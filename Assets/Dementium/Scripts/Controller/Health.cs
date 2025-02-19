using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [SerializeField]
    private int health;

    [SerializeField]
    private int maxHealth;

    public UnityEvent onDie;
    public UnityEvent onDamage;

    private bool dead;

    public bool Dead { get => dead; set => dead = value; }



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (health <= 0 && !dead)
        {
            onDie?.Invoke();
            dead = true;
        }
    }

    public void Damage(int damage)
    {
        Debug.Log($"Damage ! {damage}");

        health -= damage;
        onDamage?.Invoke();
    }

    public void Heal(int heal)
    {
        health += heal;
        health = Mathf.Clamp(health, 0, maxHealth);
    }
}
