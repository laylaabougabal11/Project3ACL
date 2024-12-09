using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class MinionController : MonoBehaviour, IHealth
{
    // Minion properties
    public int maxHealth = 20;
    private int currentHealth;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    public float wanderRadius = 10f;  // Radius around the camp for wandering
    public float wanderInterval = 3f; // Time between choosing new directions
    public float turnSpeed = 2f;      // Speed of turning for smooth movement
    public float detectionRange = 15f; // Range at which Minion becomes alerted
    public float attackRange = 2f;   // Attack range for engaging Wanderer
    public float attackCooldown = 2f; // Time between attacks
    public int attackDamage = 5;     // Damage dealt to Wanderer

    private NavMeshAgent navAgent;
    private Vector3 campCenter; // Center of the camp
    private bool isAlerted = false;
    private bool canAttack = true;

    private Animator animator; // Reference to Animator component
    private Transform target; // Reference to the Wanderer (player)

    void Start()
    {
        currentHealth = maxHealth;

        navAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // Find the parent camp and set camp center
        Transform camp = FindParentWithTag(transform, "Camp");
        if (camp != null)
        {
            campCenter = camp.position;
        }
        else
        {
            Debug.LogError("Camp not found for Minion!");
        }

        // Find the Wanderer in the scene
        target = GameObject.FindGameObjectWithTag("Wanderer").transform;
        // Debug.Log("Minion target: " + target.name);

        // Start wandering
        StartCoroutine(Wander());
    }

    void Update()
    {
        // Check if the Wanderer is within detection range
        if (Vector3.Distance(transform.position, target.position) <= detectionRange)
        {
            isAlerted = true; // Minion becomes alerted
        }

        if (isAlerted)
        {
            EngageTarget();
        }
        else
        {
            // Update Speed parameter in Animator for wandering
            animator.SetFloat("Speed", navAgent.velocity.magnitude);
        }
    }


    private void EngageTarget()
    {
        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        if (distanceToTarget <= attackRange)
        {
            if (canAttack)
            {
                StartCoroutine(Attack());
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
            StartCoroutine(Wander());
        }
    }


    private IEnumerator Attack()
    {
        canAttack = false;
        navAgent.isStopped = true;

        // Play attack animation
        animator.SetTrigger("PunchTrigger");

        yield return new WaitForSeconds(0.5f); // Wait for the attack animation's impact frame

        if (Vector3.Distance(transform.position, target.position) <= attackRange)
        {
            // Deal damage to the Wanderer
            WandererController wanderer = target.GetComponent<WandererController>();
            if (wanderer != null)
            {
                wanderer.TakeDamage(attackDamage);
            }
        }

        yield return new WaitForSeconds(attackCooldown);
        navAgent.isStopped = false;
        canAttack = true;
    }

    private IEnumerator Wander()
    {
        while (!isAlerted)
        {
            // Get a random point within the camp's radius
            Vector3 randomPoint = GetRandomPointWithinRadius(campCenter, wanderRadius);

            // Check if the random point is on the NavMesh
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, wanderRadius, NavMesh.AllAreas))
            {
                // Set the destination to the random point
                navAgent.SetDestination(hit.position);

                // Gradually rotate towards the new direction for smoother movement
                Quaternion targetRotation = Quaternion.LookRotation(hit.position - transform.position);
                while (Quaternion.Angle(transform.rotation, targetRotation) > 1f)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
                    yield return null;
                }

                // Wait until the Minion reaches its destination or the interval passes
                float elapsedTime = 0f;
                while (navAgent.remainingDistance > navAgent.stoppingDistance && elapsedTime < wanderInterval)
                {
                    elapsedTime += Time.deltaTime;
                    animator.SetFloat("Speed", navAgent.velocity.magnitude); // Update Speed parameter
                    yield return null;
                }
            }

            // Wait before picking a new random point
            yield return new WaitForSeconds(wanderInterval);
        }
    }

    private Vector3 GetRandomPointWithinRadius(Vector3 center, float radius)
    {
        // Generate a random point within a circle on the XZ plane
        Vector2 randomCircle = Random.insideUnitCircle * radius;
        return new Vector3(center.x + randomCircle.x, center.y, center.z + randomCircle.y);
    }

    private Transform FindParentWithTag(Transform child, string tag)
    {
        Transform current = child;

        while (current != null)
        {
            if (current.CompareTag(tag))
            {
                return current;
            }
            current = current.parent;
        }

        return null;
    }

    public void TakeDamage(int damage)
    {
        currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHealth);

        // Trigger the pain animation
        animator.SetTrigger("PainTrigger");

        // Alert the Minion when it is damaged
        isAlerted = true;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Minion has died.");

        // Trigger death animation
        animator.SetTrigger("DieTrigger");

        // Destroy the Minion after the animation
        Destroy(gameObject, 2f);
    }
}
