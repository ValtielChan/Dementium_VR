using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Zombie : MonoBehaviour
{
    [SerializeField] private float detectionDistance;
    [SerializeField] private float attackDistance;
    [SerializeField] private float attackCooldown;

    [SerializeField] private Animator animator;
    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private Health health;

    private Transform player;

    private bool huntPlayer;
    private bool isAttacking;

    public bool IsAttacking { get => isAttacking; set => isAttacking = value; }

    void Start()
    {

        player = GameObject.FindGameObjectWithTag("Player").transform;

        HitStateBehaviour hitState = animator.GetBehaviour<HitStateBehaviour>();

        if (hitState != null)
        {
            hitState.OnStateEnterAction += () => {
                if (navMeshAgent != null)
                {
                    navMeshAgent.isStopped = true;
                }
            };

            hitState.OnStateExitAction += () =>
            {
                if (navMeshAgent != null)
                {
                    navMeshAgent.isStopped = false;
                }
            };
        }

    }

    void Update()
    {
        if (Vector3.Distance(transform.position, player.position) < detectionDistance) {
            
            navMeshAgent.destination = player.position;
            navMeshAgent.stoppingDistance = attackDistance;
        }

        if (Vector3.Distance(transform.position, player.position) < attackDistance + 0.5 && !IsAttacking)
        {
            StartCoroutine(Attack());
        }

        animator.SetBool("Walking", navMeshAgent.velocity.magnitude > 0.2f);
    }

    public IEnumerator Knockback(Vector3 direction, float distance, float speed)
    {
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + direction.normalized * distance;

        float elapsedTime = 0f;
        float duration = distance / speed;

        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
    }

    public void StopMovement()
    {
        if (navMeshAgent != null)
        {
            navMeshAgent.isStopped = true;
        }
    }

    private IEnumerator Attack()
    {
        IsAttacking = true;

        animator.SetTrigger("Attack");

        yield return new WaitForSeconds(attackCooldown);

        IsAttacking = false;
    }
}
