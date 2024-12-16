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
    private bool alternateAttack = false; // For alternating attacks in Phase 2

    // Reflective Aura properties
    [Header("Reflective Aura Settings")]
    public GameObject reflectiveAuraPrefab; // Prefab for Reflective Aura visual
    private GameObject reflectiveAuraInstance;
    private bool reflectiveAuraActive = false;
    public int reflectiveDamage = 15; // Damage reflected to the Wanderer
    public float reflectiveAuraDuration = 5f; // Duration of Reflective Aura

    // Blood Spikes properties
    [Header("Blood Spikes Settings")]
    public GameObject bloodSpikePrefab; // Prefab for Blood Spikes
    public Transform bloodSpikeSpawnPoint; // Location to spawn Blood Spikes

    // Shield Visual
    public GameObject shieldVisualPrefab;
    private GameObject shieldVisual;

    // References
    private Animator animator;
    private Transform wanderer;

    public GameObject minionPrefab; // Minion prefab for summoning

    public float bloodSpikeRange = 5f; // Medium range for Blood Spike effect

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
            else if (phase == 2)
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
        if (alternateAttack && !reflectiveAuraActive)
        {
            TriggerBloodSpikes();
            StartCoroutine(BloodSpikes());
        }
        else
        {
            TriggerReflectiveAura();
            StartCoroutine(ReflectiveAura());
        }

        alternateAttack = !alternateAttack; // Alternate between Reflective Aura and Blood Spikes
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

        // Activate Reflective Aura
        reflectiveAuraActive = true;
        if (reflectiveAuraPrefab != null && reflectiveAuraInstance == null)
        {
            reflectiveAuraInstance = Instantiate(reflectiveAuraPrefab, transform.position, Quaternion.identity, transform);
        }

        Debug.Log("Reflective Aura Activated!");

        yield return new WaitForSeconds(reflectiveAuraDuration); // Aura duration

        DeactivateReflectiveAura();

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    private void DeactivateReflectiveAura()
    {
        reflectiveAuraActive = false;

        if (reflectiveAuraInstance != null)
        {
            Destroy(reflectiveAuraInstance);
        }

        Debug.Log("Reflective Aura Deactivated!");
    }

    private GameObject activeBloodSpike; // Track active Blood Spike instance

private IEnumerator BloodSpikes()
{
    canAttack = false;

    Debug.Log("Blood Spike Activated!");

    // Check if a spike is already active
    if (activeBloodSpike == null && bloodSpikePrefab != null && bloodSpikeSpawnPoint != null)
    {
        // Spawn and activate the blood spike
        activeBloodSpike = Instantiate(bloodSpikePrefab, bloodSpikeSpawnPoint.position, bloodSpikeSpawnPoint.rotation);
        Debug.Log($"Blood Spike spawned at {bloodSpikeSpawnPoint.position}");
    }
    else if (activeBloodSpike != null)
    {
        // Reactivate the existing blood spike if deactivated
        activeBloodSpike.SetActive(true);
    }
    else
    {
        Debug.LogError("Blood Spike Prefab or Spawn Point is not assigned!");
    }

    // Check for Wanderer within range
    if (wanderer != null)
    {
        float distanceToWanderer = Vector3.Distance(bloodSpikeSpawnPoint.position, wanderer.position);
        if (distanceToWanderer <= bloodSpikeRange)
        {
            WandererController wandererController = wanderer.GetComponent<WandererController>();
            if (wandererController != null)
            {
                wandererController.TakeDamage(15); // Apply damage
                Debug.Log("Wanderer hit by Blood Spike! Health reduced by 15.");
            }
        }
    }

    // Keep the blood spike active for a duration
    yield return new WaitForSeconds(2f); // Adjust duration as needed

    // Deactivate the blood spike
    if (activeBloodSpike != null)
    {
        activeBloodSpike.SetActive(false);
        Debug.Log("Blood Spike Deactivated!");
    }

    yield return new WaitForSeconds(attackCooldown);
    canAttack = true;
}

    public void TakeDamage(int damage)
    {
        if (!isAlive) return;

        if (reflectiveAuraActive)
        {
            ReflectDamage();
        }
        else if (shieldActive)
        {
            currentShieldHealth -= damage;

            if (currentShieldHealth <= 0)
            {
                shieldActive = false;

                // Apply remaining damage to Lilith
                int overflowDamage = -currentShieldHealth;
                currentHealth -= overflowDamage;

                Debug.Log("Shield destroyed! Remaining damage applied to Lilith.");
                DestroyShieldVisual();
                StartCoroutine(RegenerateShield());
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

    private void ReflectDamage()
    {
        if (wanderer != null)
        {
            WandererController wandererController = wanderer.GetComponent<WandererController>();
            wandererController?.TakeDamage(reflectiveDamage);
            Debug.Log($"Reflected {reflectiveDamage} damage to the Wanderer!");
        }

        // Automatically deactivate Reflective Aura after reflection
        DeactivateReflectiveAura();
    }

    private void CreateShieldVisual()
    {
        if (shieldVisualPrefab != null && shieldVisual == null)
        {
            shieldVisual = Instantiate(shieldVisualPrefab, transform.position, Quaternion.identity, transform);
        }
    }

    private void DestroyShieldVisual()
    {
        if (shieldVisual != null)
        {
            Destroy(shieldVisual);
        }
    }

    private IEnumerator RegenerateShield()
    {
        Debug.Log("Shield regeneration started.");
        yield return new WaitForSeconds(10f);

        shieldActive = true;
        currentShieldHealth = shieldHealth;
        CreateShieldVisual();
        Debug.Log("Shield regenerated!");
    }

    private void EnterPhase2()
    {
        phase = 2;
        currentHealth = maxHealthPhase2;
        currentShieldHealth = shieldHealth;
        shieldActive = true;
        CreateShieldVisual();

        TriggerPhase2();

        Debug.Log("Transitioned to Phase 2!");
    }

    private void Die()
    {
        isAlive = false;

        TriggerDie();

        Debug.Log("Lilith has been defeated!");

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
