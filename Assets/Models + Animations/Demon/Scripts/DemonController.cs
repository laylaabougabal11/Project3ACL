using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class DemonController : MonoBehaviour, IHealth
{
    // Demon properties
    public int maxHealth = 40;
    private int currentHealth;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    public float patrolRadius = 15f;
    public float detectionRange = 20f;
    public float attackRange = 2f;
    public float explosiveRange = 5f;
    public float attackCooldown = 1.5f;
    public int swordDamage = 10;
    public int explosiveDamage = 15;

    private NavMeshAgent navAgent;
    private Vector3 campCenter;
    private bool isAlerted = false;
    private bool canAttack = true;
    private bool isAlive = true;

    private Animator animator;
    private Transform target;

    void Start()
    {
        currentHealth = maxHealth;

        navAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        EnemyCampController camp = GetComponentInParent<EnemyCampController>();
        if (camp != null)
        {
            campCenter = camp.transform.position;
        }
        else
        {
            Debug.LogWarning("Camp not found for Demon.");
        }

        GameObject wanderer = GameObject.FindGameObjectWithTag("Wanderer");
        if (wanderer != null)
        {
            target = wanderer.transform;
        }
        else
        {
            Debug.LogWarning("Wanderer not found in the scene.");
        }

        StartCoroutine(Patrol());
    }

    void Update()
    {
        if (!isAlive) return;

        // Update the blend tree parameter for movement
        if (navAgent != null)
        {
            animator.SetFloat("Speed", navAgent.velocity.magnitude);
        }

        if (target != null && Vector3.Distance(transform.position, target.position) <= detectionRange)
        {
            isAlerted = true;
        }

        if (isAlerted)
        {
            EngageTarget();
        }
    }

    private void EngageTarget()
    {
        if (!isAlive || navAgent == null || !navAgent.enabled) return;

        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        if (distanceToTarget <= attackRange)
        {
            if (canAttack)
            {
                StartCoroutine(AttackPattern());
            }
        }
        else if (distanceToTarget <= detectionRange)
        {
            if (navAgent.isOnNavMesh)
            {
                navAgent.SetDestination(target.position);
                // Blend Tree handles the movement animations based on speed
            }
        }
        else
        {
            isAlerted = false;
            StartCoroutine(Patrol());
        }
    }

    private IEnumerator AttackPattern()
    {
        canAttack = false;

        if (navAgent != null && navAgent.isOnNavMesh)
        {
            navAgent.isStopped = true;
        }

        // Sword Swing 1
        animator.SetTrigger("SwordSwingTrigger");
        yield return new WaitForSeconds(0.5f);
        DealDamageToTarget(swordDamage);

        yield return new WaitForSeconds(attackCooldown);

        // Sword Swing 2
        animator.SetTrigger("SwordSwingTrigger");
        yield return new WaitForSeconds(0.5f);
        DealDamageToTarget(swordDamage);

        yield return new WaitForSeconds(attackCooldown);

        // Explosive Attack
        animator.SetTrigger("ExplosiveTrigger");
        yield return new WaitForSeconds(0.5f);
        Explode();

        yield return new WaitForSeconds(attackCooldown);

        if (isAlive && navAgent != null && navAgent.isOnNavMesh)
        {
            navAgent.isStopped = false;
        }

        canAttack = true;
    }

    private void DealDamageToTarget(int damage)
    {
        if (target != null && Vector3.Distance(transform.position, target.position) <= attackRange)
        {
            WandererController wanderer = target.GetComponent<WandererController>();
            wanderer?.TakeDamage(damage);
        }
    }

    private void Explode()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosiveRange);
        foreach (Collider collider in hitColliders)
        {
            if (collider.CompareTag("Wanderer"))
            {
                WandererController wanderer = collider.GetComponent<WandererController>();
                wanderer?.TakeDamage(explosiveDamage);
            }
        }
    }

    private IEnumerator Patrol()
    {
        while (!isAlerted && isAlive)
        {
            if (navAgent == null || !navAgent.enabled || !navAgent.isOnNavMesh) yield break;

            Vector3 randomPoint = GetRandomPointWithinRadius(campCenter, patrolRadius);

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, patrolRadius, NavMesh.AllAreas))
            {
                navAgent.SetDestination(hit.position);

                while (navAgent.remainingDistance > navAgent.stoppingDistance)
                {
                    yield return null;
                }
            }

            yield return new WaitForSeconds(2f);
        }
    }

    private Vector3 GetRandomPointWithinRadius(Vector3 center, float radius)
    {
        Vector2 randomCircle = Random.insideUnitCircle * radius;
        return new Vector3(center.x + randomCircle.x, center.y, center.z + randomCircle.y);
    }

    public void TakeDamage(int damage)
    {
        if (!isAlive) return;

        currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHealth);
        animator.SetTrigger("PainTrigger");

        isAlerted = true;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (!isAlive) return;

        isAlive = false;

        EnemyCampController camp = GetComponentInParent<EnemyCampController>();
        camp?.DeregisterEnemy(gameObject);

        if (navAgent != null && navAgent.isOnNavMesh)
        {
            navAgent.isStopped = true;
            navAgent.enabled = false;
        }

        animator.SetTrigger("DieTrigger");

        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        Destroy(gameObject, GetAnimationLength("Die"));
    }

    private float GetAnimationLength(string animationName)
    {
        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            if (clip.name == animationName)
            {
                return clip.length;
            }
        }
        return 2f;
    }
}
