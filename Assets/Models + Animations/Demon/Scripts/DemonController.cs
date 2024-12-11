using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DemonController : MonoBehaviour, IHealth
{
    // Demon properties
    public int maxHealth = 40;
    private int currentHealth;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    public float patrolRadius = 15f;    // Radius for patrolling around the camp
    public float detectionRange = 20f; // Range at which Demon becomes alerted
    public float attackRange = 2f;     // Attack range for sword swing
    public float explosiveRange = 5f; // Range for explosive attack
    public float attackCooldown = 1.5f; // Time between attacks
    public int swordDamage = 10;      // Damage dealt by sword swing
    public int explosiveDamage = 15;  // Damage dealt by explosive

    private NavMeshAgent navAgent;
    private Vector3 campCenter; // Center of the camp
    private bool isAlerted = false;
    private bool canAttack = true;
    private bool isAlive = true;

    private Animator animator; // Reference to Animator component
    private Transform target; // Reference to the Wanderer (player)

    void Start()
    {
        currentHealth = maxHealth;

        navAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // Find the parent camp and set camp center
        EnemyCampController camp = GetComponentInParent<EnemyCampController>();
        if (camp != null)
        {
            campCenter = camp.transform.position;
        }
        else
        {
            Debug.LogError("Camp not found for Demon!");
        }

        // Find the Wanderer in the scene
        GameObject wanderer = GameObject.FindGameObjectWithTag("Wanderer");
        if (wanderer != null)
        {
            target = wanderer.transform;
        }
        else
        {
            Debug.LogError("Wanderer not found in the scene!");
        }

        // Start patrolling
        StartCoroutine(Patrol());
    }

    void Update()
    {
        if (!isAlive) return;

        // Check if the Wanderer is within detection range
        if (target != null && Vector3.Distance(transform.position, target.position) <= detectionRange)
        {
            isAlerted = true; // Demon becomes alerted
        }

        if (isAlerted)
        {
            EngageTarget();
        }
    }

    private void EngageTarget()
    {
        if (target == null) return;

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
            // Chase the Wanderer
            navAgent.SetDestination(target.position);
            animator.SetFloat("Speed", navAgent.velocity.magnitude);
        }
        else
        {
            // Reset alert if the Wanderer leaves detection range
            isAlerted = false;
            StartCoroutine(Patrol());
        }
    }

    private IEnumerator AttackPattern()
    {
        canAttack = false;
        navAgent.isStopped = true;

        // Sword Swing 1
        animator.SetTrigger("SwordSwingTrigger");
        yield return new WaitForSeconds(0.5f); // Wait for the attack animation's impact frame
        DealDamageToTarget(swordDamage);

        yield return new WaitForSeconds(attackCooldown);

        // Sword Swing 2
        animator.SetTrigger("SwordSwingTrigger");
        yield return new WaitForSeconds(0.5f); // Wait for the attack animation's impact frame
        DealDamageToTarget(swordDamage);

        yield return new WaitForSeconds(attackCooldown);

        // Explosive Attack
        animator.SetTrigger("ExplosiveTrigger");
        yield return new WaitForSeconds(0.5f); // Wait for the explosive animation's impact frame
        Explode();

        yield return new WaitForSeconds(attackCooldown);

        navAgent.isStopped = false;
        canAttack = true;
    }

    private void DealDamageToTarget(int damage)
    {
        if (Vector3.Distance(transform.position, target.position) <= attackRange)
        {
            WandererController wanderer = target.GetComponent<WandererController>();
            if (wanderer != null)
            {
                wanderer.TakeDamage(damage);
            }
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
                if (wanderer != null)
                {
                    wanderer.TakeDamage(explosiveDamage);
                }
            }
        }
    }

    private IEnumerator Patrol()
    {
        while (!isAlerted)
        {
            // Get a random point within the camp's radius
            Vector3 randomPoint = GetRandomPointWithinRadius(campCenter, patrolRadius);

            // Check if the random point is on the NavMesh
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, patrolRadius, NavMesh.AllAreas))
            {
                // Set the destination to the random point
                navAgent.SetDestination(hit.position);
                animator.SetFloat("Speed", navAgent.velocity.magnitude);

                // Wait until the Demon reaches its destination or stops
                while (navAgent.remainingDistance > navAgent.stoppingDistance)
                {
                    yield return null;
                }
            }

            // Wait for a short duration before patrolling to another point
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
        currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHealth);

        // Trigger the pain animation
        animator.SetTrigger("PainTrigger");

        // Alert the Demon when it is damaged
        isAlerted = true;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (!isAlive) return;

        isAlive = false; // Mark as dead
        Debug.Log($"{gameObject.name} has died.");

        // Notify the camp controller
        EnemyCampController camp = GetComponentInParent<EnemyCampController>();
        if (camp != null)
        {
            camp.DeregisterEnemy(gameObject);
        }

        // Stop the NavMeshAgent
        if (navAgent != null)
        {
            navAgent.isStopped = true;
            navAgent.enabled = false;
        }

        // Trigger death animation
        animator.SetTrigger("DieTrigger");

        // Disable collider to avoid interaction after death
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        // Get the death animation length and destroy the Demon after it finishes
        float animationLength = GetAnimationLength("Die");
        Destroy(gameObject, animationLength);
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
        return 2f; // Default to 2 seconds if the animation length is not found
    }

    private void RewardXP()
    {
        WandererController wanderer = target.GetComponent<WandererController>();
        if (wanderer != null)
        {
            wanderer.GainXP(30); // Reward 30 XP for killing the Demon
        }
    }
}
