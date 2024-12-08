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

            if (target.CompareTag("Enemy")) // Check if the clicked object is an enemy
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
        animator.SetTrigger("BasicTrigger"); // Use animator from the parent class
        Debug.Log("Bash activated!");
        isAbilityActive = true; // Mark the ability as active

        cooldownTimers["Bash"] = cooldownDurations["Bash"];
        // Add logic for dealing damage

        // Apply damage to the enemy

        // EnemyController enemyController = enemy.GetComponent<EnemyController>();
        // if (enemyController != null)
        // {
        //     enemyController.TakeDamage(bashDamage);
        //     Debug.Log($"Enemy hit for {bashDamage} damage!");
        // }
        // else
        // {
        //     Debug.LogError("The selected target does not have an EnemyController script.");
        // }
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
        animator.SetTrigger("WildCardTrigger"); // Use animator from the parent class
        isAbilityActive = true; // Mark the ability as active

        Debug.Log("Iron Maelstrom activated!");
        cooldownTimers["IronMaelstrom"] = cooldownDurations["IronMaelstrom"];

        if (maelstromEffect != null)
        {
            Instantiate(maelstromEffect, transform.position, Quaternion.identity);
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
        // Trigger the charge animation
        animator.SetTrigger("UltimateTrigger");

        // Calculate the direction to the target
        Vector3 direction = (chargeTarget - transform.position).normalized;

        // Rotate to face the target direction
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = targetRotation;

        // Start charging
        isCharging = true;
        Debug.Log("Charge animation triggered and facing target.");
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
