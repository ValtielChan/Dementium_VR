using UnityEngine;
using UnityEngine.AI;

public class Zombie : MonoBehaviour
{
    [SerializeField] private float speed = 2f;
    [SerializeField] private float knockbackForce = 2f;
    [SerializeField] private Animator animator;
    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private Health health;

    private Transform player;
    private bool isHit = false;
    private Vector3 velocity;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        health.onDie.AddListener(Die);
        navMeshAgent.speed = speed;
    }

    void Update()
    {
        if (isHit || health.Dead) return;
        MoveTowardsPlayer();
    }

    private void MoveTowardsPlayer()
    {
        if (player == null) return;

        navMeshAgent.SetDestination(player.position);
        animator.SetBool("Walking", true);
    }

    public void TakeDamage(int damage, Vector3 hitDirection)
    {
        if (health.Dead) return;

        health.Damage(damage);
        animator.SetTrigger("Hit");
        isHit = true;

        navMeshAgent.isStopped = true;
        velocity = hitDirection.normalized * knockbackForce;
        transform.position += velocity * Time.deltaTime;

        Invoke(nameof(ResetHitState), 0.5f);
    }

    private void ResetHitState()
    {
        isHit = false;
        navMeshAgent.isStopped = false;
    }

    private void Die()
    {
        animator.SetBool("Walking", false);
        animator.SetTrigger("Die");
        navMeshAgent.isStopped = true;
        this.enabled = false;
    }
}
