using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barbarian : WandererController
{
    // Cooldowns and ability durations
    private Dictionary<string, float> cooldownTimers;
    private Dictionary<string, float> cooldownDurations;

    private bool isAbilityActive = false; // Track if an ability is playing

    // Bash ability properties
    public float bashRange = 3f; // Maximum range to perform Bash
    public int bashDamage = 20; // Damage dealt by Bash ability


    // Shield ability properties
    private bool isShieldActive = false;

    // Shield visual effect
    public GameObject shieldEffect;

    // Damage area for Iron Maelstrom
    public GameObject maelstromEffect;

    // Charge ability properties
    public float chargeSpeed = 10f; // Speed of the charge
    public int bossDamage = 20; // Damage dealt to the boss
    public LayerMask walkableLayer; // LayerMask for walkable surfaces
    public LayerMask enemyLayer; // LayerMask for enemies
    public LayerMask destructibleLayer; // LayerMask for destructible objects

    private bool isCharging = false; // Track if the Barbarian is currently charging
    private Vector3 chargeTarget; // Target position for the charge


    protected override void Start()
    {
        enemyLayer = LayerMask.GetMask("enemyLayer"); // Add all relevant layers

        base.Start();

        walkableLayer = LayerMask.GetMask("Terrain");

        // Initialize cooldowns
        cooldownTimers = new Dictionary<string, float>
        {
            { "Bash", 0f },
            { "Shield", 0f },
            { "IronMaelstrom", 0f },
            { "Charge", 0f }
        };

        cooldownDurations = new Dictionary<string, float>
        {
            { "Bash", 1f },
            { "Shield", 10f },
            { "IronMaelstrom", 5f },
            { "Charge", 2f }
        };
    }

    private void Update()
    {
        base.Update(); // Call the parent class's Update method to handle shared logic
        HandleCooldowns();
        HandleInputs();

        if (isAbilityActive)
        {
            AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);

            // Check if the current animation is NOT an ability animation anymore
            if (!currentState.IsTag("Ability"))
            {
                isAbilityActive = false; // Mark the ability as complete
                Debug.Log("Returning to Blend Tree.");
            }
        }

    }

    private void HandleCooldowns()
    {
        var keys = new List<string>(cooldownTimers.Keys);

        foreach (var key in keys)
        {
            if (cooldownTimers[key] > 0)
            {
                cooldownTimers[key] -= Time.deltaTime;
            }
        }
    }

    protected override void HandleInputs()
    {
        if (Input.GetMouseButtonDown(1) && cooldownTimers["Bash"] <= 0)
        {
            TryBash();
        }
        if (Input.GetKeyDown(KeyCode.W) && cooldownTimers["Shield"] <= 0)
        {
            ActivateShield();
        }
        if (Input.GetKeyDown(KeyCode.Q) && cooldownTimers["IronMaelstrom"] <= 0)
        {
            IronMaelstrom();
        }
        if (Input.GetKeyDown(KeyCode.E) && cooldownTimers["Charge"] <= 0)
        {
            InitiateCharge();
        }
    }

    private void TryBash()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            GameObject target = hit.collider.gameObject; // Get the clicked object

            if (target.CompareTag("Demon") || target.CompareTag("Minion")) // Check if the clicked object is an enemy
            {
                float distance = Vector3.Distance(transform.position, target.transform.position);

                if (distance <= bashRange) // Check if the enemy is within range
                {
                    Bash(target); // Perform the Bash ability
                }
                else
                {
                    Debug.Log("Enemy is out of range for Bash!");
                }
            }
            else
            {
                Debug.Log("No valid enemy selected!");
            }
        }
    }

    private void Bash(GameObject enemy)
    {
        if (selectedTarget == null)
        {
            Debug.Log("No target selected!");
            return;
        }

        float distance = Vector3.Distance(transform.position, selectedTarget.position);
        if (distance <= bashRange) // Check range
        {
            Debug.Log($"Bashing {selectedTarget.name}");
            IHealth targetHealth = selectedTarget.GetComponent<IHealth>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(bashDamage); // Deal damage
            }

            animator.SetTrigger("BasicTrigger"); // Play animation
            isAbilityActive = true; // Mark ability as active

            cooldownTimers["Bash"] = cooldownDurations["Bash"]; // Apply cooldown
        }
        else
        {
            Debug.Log("Target is out of range!");
        }
    }


    private void ActivateShield()
    {
        if (isShieldActive) return;

        animator.SetTrigger("DefensiveTrigger"); // Use animator from the parent class
        isAbilityActive = true; // Mark the ability as active

        Debug.Log("Shield activated!");
        isShieldActive = true;

        if (shieldEffect != null)
        {
            shieldEffect.SetActive(true);
        }

        StartCoroutine(ShieldDuration());
        cooldownTimers["Shield"] = cooldownDurations["Shield"];
    }

    private IEnumerator ShieldDuration()
    {
        yield return new WaitForSeconds(3f);
        Debug.Log("Shield deactivated!");
        isShieldActive = false;

        if (shieldEffect != null)
        {
            shieldEffect.SetActive(false);
        }
    }

    private void IronMaelstrom()
    {
        animator.SetTrigger("WildCardTrigger");
        isAbilityActive = true;

        Debug.Log("Iron Maelstrom activated!");

        cooldownTimers["IronMaelstrom"] = cooldownDurations["IronMaelstrom"];

        float radius = 3f;
        int damage = 30;

        Debug.Log($"Iron Maelstrom checking for enemies within {radius} units.");

        // Damage enemies in a radius
        DamageEnemiesInRadius(transform.position, radius, damage);

        if (maelstromEffect != null)
        {
            Instantiate(maelstromEffect, transform.position, Quaternion.identity);
        }
    }

    private void DamageEnemiesInRadius(Vector3 center, float radius, int damage)
    {
        Collider[] hitColliders = Physics.OverlapSphere(center, radius, enemyLayer);

        if (hitColliders.Length == 0)
        {
            Debug.Log("No enemies detected in the radius.");
        }
        else
        {
            Debug.Log($"Detected {hitColliders.Length} enemies in the radius.");
        }

        foreach (Collider collider in hitColliders)
        {
            float distance = Vector3.Distance(center, collider.transform.position);
            Debug.Log($"{collider.name} is {distance} units away from the center.");

            IHealth targetHealth = collider.GetComponent<IHealth>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(damage);
                Debug.Log($"Iron Maelstrom hit {collider.name} for {damage} damage.");
            }
            else
            {
                Debug.Log($"{collider.name} does not have an IHealth component.");
            }
        }
    }




    private void InitiateCharge()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Perform the raycast without a range limit
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, walkableLayer.value))
        {
            chargeTarget = hit.point; // Set the target position

            Charge(); // Start the charge ability

            Debug.Log($"Charging to {chargeTarget}");
        }
        else
        {
            Debug.Log("Invalid charge target. Ensure it's walkable.");
        }



    }

    private void Charge()
    {
        // Disable NavMeshAgent during the charge
        if (navMeshAgent != null)
        {
            navMeshAgent.isStopped = true;
            navMeshAgent.enabled = false;
        }

        // Trigger the charge animation
        animator.SetTrigger("UltimateTrigger");

        // Calculate the direction to the target
        Vector3 direction = (chargeTarget - transform.position).normalized;

        // Rotate to face the target direction
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = targetRotation;

        // Start the charging coroutine
        StartCoroutine(PerformCharge(direction));
    }

    private IEnumerator PerformCharge(Vector3 direction)
    {
        isCharging = true;
        float chargeDistance = Vector3.Distance(transform.position, chargeTarget);
        float chargeDuration = chargeDistance / chargeSpeed;

        float elapsedTime = 0f;
        while (elapsedTime < chargeDuration)
        {
            // Move the Barbarian towards the target
            Vector3 move = direction * chargeSpeed * Time.deltaTime;
            transform.position += move;

            // Check for collisions with enemies and destructible objects
            HandleChargeCollisions();

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // End the charge
        isCharging = false;

        // Re-enable NavMeshAgent
        if (navMeshAgent != null)
        {
            navMeshAgent.enabled = true;
            navMeshAgent.isStopped = false;
        }

        Debug.Log("Charge completed.");
    }

    private void HandleChargeCollisions()
    {
        float collisionRadius = 1f; // Adjust based on Barbarian's size
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, collisionRadius);

        foreach (Collider collider in hitColliders)
        {
            if (collider.CompareTag("Minion") || collider.CompareTag("Demon"))
            {
                // Damage the enemy
                IHealth targetHealth = collider.GetComponent<IHealth>();
                if (targetHealth != null)
                {
                    targetHealth.TakeDamage(bossDamage);
                    Debug.Log($"Charge hit {collider.name} for {bossDamage} damage.");
                }
            }
            else if (collider.CompareTag("Destructible"))
            {
                // Destroy destructible objects
                Destroy(collider.gameObject);
                Debug.Log($"Charge destroyed {collider.name}.");
            }
        }
    }



    public override void TakeDamage(int damage)
    {
        if (isShieldActive)
        {
            Debug.Log("Damage blocked by Shield!");
            return;
        }

        base.TakeDamage(damage);
    }
}
