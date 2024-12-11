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

    public float wanderRadius = 5f;  // Radius around the camp for wandering
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

    private bool isAlive = true; // Track if the Minion is alive


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

        if (!isAlive) return; // Skip Update logic if Minion is dead

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

    private void LookAtTarget()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0; // Ignore vertical rotation
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, turnSpeed * Time.deltaTime);
    }



    private void EngageTarget()
    {
        if (!isAlive || navAgent == null || !navAgent.enabled) return;

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
            LookAtTarget();
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
        if (!isAlive) yield break; // Exit the coroutine if the Minion is dead

        canAttack = false;

        if (navAgent != null)
        {
            navAgent.isStopped = true; // Stop the agent before attacking
        }

        // Play attack animation
        animator.SetTrigger("PunchTrigger");

        yield return new WaitForSeconds(0.5f); // Wait for the attack animation's impact frame

        if (isAlive && Vector3.Distance(transform.position, target.position) <= attackRange)
        {
            // Deal damage to the Wanderer
            WandererController wanderer = target.GetComponent<WandererController>();
            if (wanderer != null)
            {
                wanderer.TakeDamage(attackDamage);
            }
        }

        yield return new WaitForSeconds(attackCooldown);

        if (isAlive && navAgent != null)
        {
            navAgent.isStopped = false; // Resume the agent only if Minion is still alive
        }

        canAttack = true;
    }


    private IEnumerator Wander()
    {
        while (!isAlerted && isAlive)
        {

            if (!isAlive) yield break;
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
                while (isAlive && Quaternion.Angle(transform.rotation, targetRotation) > 1f)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
                    yield return null;
                }

                // Wait until the Minion reaches its destination or the interval passes
                float elapsedTime = 0f;
                while (isAlive && navAgent.remainingDistance > navAgent.stoppingDistance && elapsedTime < wanderInterval)
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
        Debug.Log($"{gameObject.name} took {damage} damage. Remaining health: {currentHealth}");
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

        // Reward XP for killing the Minion
        RewardXP();

        // Get the death animation length and destroy the Minion after it finishes
        float animationLength = GetAnimationLength("Die");
        Destroy(gameObject, animationLength);
    }





    private void RewardXP()
    {
        // Find the Wanderer and reward XP
        WandererController wanderer = FindObjectOfType<WandererController>();
        if (wanderer != null)
        {
            wanderer.GainXP(10); // Reward 10 XP for killing a Minion
            Debug.Log("Wanderer gained 10 XP!");
        }
    }

    // Helper method to get the length of an animation
    private float GetAnimationLength(string animationName)
    {
        RuntimeAnimatorController ac = animator.runtimeAnimatorController;
        foreach (AnimationClip clip in ac.animationClips)
        {
            if (clip.name == animationName)
            {
                return clip.length;
            }
        }
        return 2f; // Default to 2 seconds if animation not found
    }

}
