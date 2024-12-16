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
    public bool IsAlive => currentHealth > 0; // Returns true if health is greater than 0

    private bool isStunned = false;
    private float stunEndTime = 0f;


    public float detectionRange = 15f; // Range at which Minion becomes alerted
    public float attackRange = 2f;     // Attack range for engaging Wanderer
    public float attackCooldown = 2f; // Time between attacks
    public int attackDamage = 5;      // Damage dealt to Wanderer

    private NavMeshAgent navAgent;
    private bool isAlerted = false;
    public bool IsAlerted => isAlerted;
    private bool canAttack = true;

    private Animator animator; // Reference to Animator component
    private Transform target; // Reference to the Wanderer (player)
    private EnemyCampController camp; // Reference to the camp controller, if any

    private bool isAlive = true; // Track if the Minion is alive

    void Start()
    {
        currentHealth = maxHealth;

        navAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // Find the Wanderer in the scene
        target = GameObject.FindGameObjectWithTag("Wanderer")?.transform;

        // Optionally find the parent camp controller
        camp = GetComponentInParent<EnemyCampController>();
    }

    void Update()
    {
        if (!isAlive) return;

        // Handle stun
        if (isStunned)
        {
            if (Time.time >= stunEndTime)
            {
                Unstun();
            }
            return; // Skip the rest of Update if stunned
        }

        if (Vector3.Distance(transform.position, target.position) <= detectionRange)
        {
            isAlerted = true;
        }
        else
        {
            isAlerted = false;
        }

        if (isAlerted)
        {
            EngageTarget();
        }
        else
        {
            animator.SetFloat("Speed", 0);
        }
    }

    public void Stun(float duration)
    {
        isStunned = true;
        stunEndTime = Time.time + duration;
        if (navAgent != null && navAgent.isOnNavMesh)
        {
            navAgent.isStopped = true;
        }
        animator.SetTrigger("StunTrigger"); // Optional: Play stun animation
        Debug.Log($"{gameObject.name} is stunned for {duration} seconds.");
    }

    public void Unstun()
    {
        isStunned = false;
        if (navAgent != null && navAgent.isOnNavMesh)
        {
            navAgent.isStopped = false;
        }
        Debug.Log($"{gameObject.name} is no longer stunned.");
    }

    public void BecomeAlerted()
    {
        isAlerted = true;
        navAgent.isStopped = false;
    }

    public void BecomeNonAggressive()
    {
        isAlerted = false;
        navAgent.isStopped = true;
        animator.SetFloat("Speed", 0);
    }


    private void EngageTarget()
    {
        if (!isAlive || navAgent == null || !navAgent.enabled || target == null) return;

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
        }
    }

    private void LookAtTarget()
    {
        if (target == null) return;

        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0; // Ignore vertical rotation
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    private IEnumerator Attack()
    {
        if (!isAlive) yield break;

        canAttack = false;

        if (navAgent != null)
        {
            navAgent.isStopped = true; // Stop the agent before attacking
        }

        // Play attack animation
        animator.SetTrigger("PunchTrigger");

        yield return new WaitForSeconds(0.5f); // Wait for the attack animation's impact frame

        if (isAlive && target != null && Vector3.Distance(transform.position, target.position) <= attackRange)
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
        if (!isAlive) return; // Prevent multiple calls to Die

        isAlive = false; // Mark as dead
        Debug.Log($"{gameObject.name} has died.");

        // Notify the camp controller
        EnemyCampController camp = GetComponentInParent<EnemyCampController>();
        if (camp != null)
        {
            camp.DeregisterEnemy(gameObject);
        }

        // Check if the NavMeshAgent is valid and active
        if (navAgent != null && navAgent.isOnNavMesh)
        {
            navAgent.isStopped = true; // Stop the agent
            navAgent.enabled = false; // Disable the agent
        }
        else
        {
            Debug.LogWarning("NavMeshAgent is not active or not on a NavMesh.");
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
