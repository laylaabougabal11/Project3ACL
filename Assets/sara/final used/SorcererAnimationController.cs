using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SorcererController : WandererController
{
    private Dictionary<string, float> cooldownTimers;
    private Dictionary<string, float> cooldownDurations;

    private bool isAbilityActive = false;

    [SerializeField] private GameObject fireballPrefab;  // Fireball prefab
    [SerializeField] private Transform fireballSpawnPoint; // Fireball spawn point
    [SerializeField] private GameObject clonePrefab;
    public GameObject cloneExplosionEffectPrefab;
    [SerializeField] private GameObject infernoPrefab; // Ring of fire prefab


    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        // Initialize cooldowns
        cooldownTimers = new Dictionary<string, float>
        {
            { "Fireball", 0f },
            { "Teleport", 0f },
            { "Clone", 0f },
            { "Inferno", 0f }
        };

        cooldownDurations = new Dictionary<string, float>
        {
            { "Fireball", 1f },
            { "Teleport", 10f },
            { "Clone", 10f },
            { "Inferno", 15f }
        };

    }

    // Update is called once per frame
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
        if (Input.GetMouseButtonDown(1) && cooldownTimers["Fireball"] <= 0)
        {
            if (TrySelectEnemy()) // Only proceed if an enemy is clicked
            {
                RotateToFireTarget(); // Rotate the sorcerer to face the target
                animator.SetTrigger("BasicTrigger"); // Trigger the fireball animation
                StartCoroutine(CastFireballWithDelay(1f)); // Add a delay before casting the fireball
            }
            else
            {
                Debug.LogWarning("Fireball can only target enemies!");
            }
        }
        if (Input.GetMouseButtonDown(1) && cooldownTimers["Fireball"] > 0)
        {
            Debug.Log("Fireball still in cooldown");
        }
        if (Input.GetKeyDown(KeyCode.W) && cooldownTimers["Teleport"] <= 0)
        {
            StartCoroutine(TeleportWithDelay(0.2f)); // Teleport after a short delay
        }
        if (Input.GetKeyDown(KeyCode.W) && cooldownTimers["Teleport"] > 0)
        {
            Debug.Log("Teleport still in cooldown");
        }
        if (Input.GetKeyDown(KeyCode.Q) && cooldownTimers["Clone"] <= 0)
        {
            CreateClone(); // Trigger the Clone ability
        }
        if (Input.GetKeyDown(KeyCode.Q) && cooldownTimers["Clone"] > 0)
        {
            Debug.Log("Clone still in cooldown");
        }
        if (Input.GetKeyDown(KeyCode.E) && cooldownTimers["Inferno"] <= 0)
        {
            StartCoroutine(ActivateInferno());
        }
        if (Input.GetKeyDown(KeyCode.E) && cooldownTimers["Inferno"] > 0)
        {
            Debug.Log("Inferno still in cooldown");
        }
    }

    private bool TrySelectEnemy()
    {
        // Raycast to check if the clicked object is an enemy
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Enemy enemy = hit.collider.GetComponent<Enemy>();
            if (enemy != null)
            {
                return true; // Successfully selected an enemy
            }
        }

        return false; // No enemy selected
    }

    private IEnumerator CastFireballWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay); // Wait for the specified delay

        CastFireball(); // Cast the fireball after the delay
    }


    private void RotateToFireTarget()
    {
        // Get the mouse position and calculate the rotation
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 direction = (hit.point - transform.position).normalized;
            direction.y = 0; // Keep the rotation flat on the horizontal plane
            transform.rotation = Quaternion.LookRotation(direction); // Rotate the sorcerer to face the target
        }
    }

    private void CastFireball()
    {
        // Get the mouse position and raycast to detect the target
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Check if the hit object is an enemy
            Enemy enemy = hit.collider.GetComponent<Enemy>();
            if (enemy != null) // Proceed only if the hit object is an enemy
            {
                if (fireballPrefab != null && fireballSpawnPoint != null)
                {
                    // Instantiate the Fireball
                    GameObject fireball = Instantiate(fireballPrefab, fireballSpawnPoint.position, Quaternion.identity);

                    // Initialize the Fireball with the enemy's position
                    Fireball fireballScript = fireball.GetComponent<Fireball>();
                    if (fireballScript != null)
                    {
                        fireballScript.Initialize(hit.point);
                    }

                    // Reset the Fireball cooldown
                    cooldownTimers["Fireball"] = cooldownDurations["Fireball"];

                    Debug.Log("Fireball cast towards enemy: " + hit.collider.gameObject.name);
                }
            }
        }
    }


    private IEnumerator TeleportWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay); // Wait for the delay

        Teleport(); // Perform the teleport
    }

    private void Teleport()
    {
        // Get the mouse position and calculate the teleport position
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Check if the hit position is valid (on a walkable surface)
            UnityEngine.AI.NavMeshHit navHit;
            if (UnityEngine.AI.NavMesh.SamplePosition(hit.point, out navHit, 1.0f, UnityEngine.AI.NavMesh.AllAreas))
            {
                // Stop movement
                if (navMeshAgent != null)
                {
                    navMeshAgent.ResetPath();
                    navMeshAgent.velocity = Vector3.zero;
                }

                // Set animator speed to 0
                animator.SetFloat("Speed", 0);

                // Teleport the sorcerer to the target position
                transform.position = navHit.position;

                // Reset the Teleport cooldown
                cooldownTimers["Teleport"] = cooldownDurations["Teleport"];

                Debug.Log("Teleported to position: " + navHit.position);
            }
            else
            {
                Debug.LogWarning("Invalid teleport destination!");
            }
        }
    }


    private void CreateClone()
    {
        // Get the mouse position and calculate the clone position
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Check if the hit position is valid (on a walkable surface)
            UnityEngine.AI.NavMeshHit navHit;
            if (UnityEngine.AI.NavMesh.SamplePosition(hit.point, out navHit, 1.0f, UnityEngine.AI.NavMesh.AllAreas))
            {
                // Spawn the clone at the target position
                GameObject clone = Instantiate(clonePrefab, navHit.position, Quaternion.identity);

                // Reset the Clone cooldown
                cooldownTimers["Clone"] = cooldownDurations["Clone"];

                // Start the clone lifecycle
                StartCoroutine(CloneLifecycle(clone));
            }
            else
            {
                Debug.LogWarning("Invalid clone destination!");
            }
        }
    }

    private IEnumerator CloneLifecycle(GameObject clone)
    {
        // Clone exists for 5 seconds
        yield return new WaitForSeconds(5f);

        // Trigger the clone's explosion
        CloneExplosion(clone);
    }

    private void CloneExplosion(GameObject clone)
    {
        // Damage enemies within a radius
        float explosionRadius = 5f; // Adjust as needed
        Collider[] hitColliders = Physics.OverlapSphere(clone.transform.position, explosionRadius);
        foreach (Collider hit in hitColliders)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(10); // Deal 10 damage to each enemy
            }
        }

        // Spawn the explosion effect
        if (cloneExplosionEffectPrefab != null)
        {
            Instantiate(cloneExplosionEffectPrefab, clone.transform.position, Quaternion.identity);
        }

        Debug.Log("Clone exploded!");

        // Destroy the clone GameObject
        Destroy(clone);
    }

    private IEnumerator ActivateInferno()
    {
        // Get the mouse position and calculate the Inferno position
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Spawn the Inferno ring
            GameObject inferno = Instantiate(infernoPrefab, hit.point, Quaternion.identity);

            // Damage enemies immediately
            DamageEnemiesInRadius(hit.point, 5f, 10); // Initial damage: 10 points

            // Apply damage every second for 5 seconds
            for (int i = 0; i < 5; i++)
            {
                yield return new WaitForSeconds(1f);
                DamageEnemiesInRadius(hit.point, 5f, 2); // Continuous damage: 2 points per second
            }

            // Destroy the Inferno ring after 5 seconds
            Destroy(inferno);

            // Reset the Inferno cooldown
            cooldownTimers["Inferno"] = cooldownDurations["Inferno"];
        }
        else
        {
            Debug.LogWarning("Invalid Inferno target position!");
        }
    }

    private void DamageEnemiesInRadius(Vector3 position, float radius, int damage)
    {
        Collider[] hitColliders = Physics.OverlapSphere(position, radius);
        foreach (Collider hit in hitColliders)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage); // Apply damage to the enemy
            }
        }
    }
}
