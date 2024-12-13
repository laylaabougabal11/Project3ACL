using System.Collections;
using UnityEngine;

public class BossLilithController : MonoBehaviour, IHealth
{
    // Phase-related properties
    public int phase = 1;
    private bool isAlive = true;

    // Health properties
    public int maxHealthPhase1 = 50;
    public int maxHealthPhase2 = 50;
    public int shieldHealth = 50;
    private int currentHealth;
    private int currentShieldHealth;
    private bool shieldActive = false;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => phase == 1 ? maxHealthPhase1 : maxHealthPhase2;

    // Combat properties
    public float attackCooldown = 2f;
    private bool canAttack = true;

    // References
    private Animator animator;
    private Transform wanderer;

    // Attack effects
    public GameObject minionPrefab; // Prefab for summoning minions
    // public GameObject divebombEffect;
    // public GameObject bloodSpikeEffect;

    void Start()
    {
        animator = GetComponent<Animator>();

        // Find the Wanderer
        wanderer = GameObject.FindGameObjectWithTag("Wanderer").transform;

        // Initialize health for Phase 1
        currentHealth = maxHealthPhase1;
    }

    void Update()
    {
        if (!isAlive) return;

        LookAtWanderer();

        if (canAttack)
        {
            if (phase == 1)
            {
                HandlePhase1Attacks();
            }
            else
            {
                HandlePhase2Attacks();
            }
        }
    }

    private void LookAtWanderer()
    {
        if (wanderer != null)
        {
            Vector3 direction = (wanderer.position - transform.position).normalized;
            direction.y = 0; // Ignore vertical rotation
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    private void HandlePhase1Attacks()
    {
        if (MinionsDefeated())
        {
            TriggerSummonMinions();
            StartCoroutine(SummonMinions());
        }
        else
        {
            TriggerDivebomb();
            StartCoroutine(DivebombAttack());
        }
    }

    private void HandlePhase2Attacks()
    {
        if (shieldActive)
        {
            TriggerReflectiveAura();
            StartCoroutine(ReflectiveAura());
        }
        else
        {
            TriggerBloodSpikes();
            StartCoroutine(BloodSpikes());
        }
    }

    private IEnumerator SummonMinions()
    {
        canAttack = false;

        // Spawn minions
        for (int i = 0; i < 3; i++)
        {
            Vector3 spawnPosition = transform.position + Random.insideUnitSphere * 5f;
            spawnPosition.y = transform.position.y;
            Instantiate(minionPrefab, spawnPosition, Quaternion.identity);
        }

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    private IEnumerator DivebombAttack()
    {
        canAttack = false;

        // Wait for the divebomb effect
        yield return new WaitForSeconds(0.5f);

        // Apply damage to Wanderer in range
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 5f);
        foreach (Collider collider in hitColliders)
        {
            if (collider.CompareTag("Wanderer"))
            {
                WandererController wanderer = collider.GetComponent<WandererController>();
                wanderer?.TakeDamage(20);
            }
        }

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    private IEnumerator ReflectiveAura()
    {
        canAttack = false;

        // Activate the aura
        shieldActive = true;

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    private IEnumerator BloodSpikes()
    {
        canAttack = false;

        // Activate blood spikes
        // Instantiate(bloodSpikeEffect, transform.position, Quaternion.identity);

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    public void TakeDamage(int damage)
    {
        if (!isAlive) return;

        if (shieldActive)
        {
            currentShieldHealth -= damage;

            if (currentShieldHealth <= 0)
            {
                shieldActive = false;
                currentShieldHealth = 0;
                Debug.Log("Shield destroyed!");
            }
        }
        else
        {
            currentHealth -= damage;

            if (currentHealth <= 0)
            {
                if (phase == 1)
                {
                    EnterPhase2();
                }
                else
                {
                    Die();
                }
            }
        }
    }

    private void EnterPhase2()
    {
        phase = 2;
        currentHealth = maxHealthPhase2;
        currentShieldHealth = shieldHealth;
        shieldActive = true;

        TriggerPhase2();

        Debug.Log("Transitioned to Phase 2!");
    }

    private void Die()
    {
        isAlive = false;

        TriggerDie();

        Debug.Log("Lilith has been defeated!");

        // Destroy Lilith after the animation
        Destroy(gameObject, 5f);
    }

    private bool MinionsDefeated()
    {
        GameObject[] minions = GameObject.FindGameObjectsWithTag("Minion");
        return minions.Length == 0;
    }

    // Animation Triggers
    private void TriggerSummonMinions() => animator.SetTrigger("SummonMinions");
    private void TriggerDivebomb() => animator.SetTrigger("Divebomb");
    private void TriggerReflectiveAura() => animator.SetTrigger("ReflectiveAura");
    private void TriggerBloodSpikes() => animator.SetTrigger("BloodSpikes");
    private void TriggerPhase2() => animator.SetBool("IsPhase2", true);
    private void TriggerDie() => animator.SetBool("IsDead", true);
}
