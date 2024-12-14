using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RogueController : WandererController
{
    // Cooldown management
    private Dictionary<string, float> cooldownTimers;
    private Dictionary<string, float> cooldownDurations;

    [SerializeField] private GameObject arrowPrefab; // Arrow prefab
    public Transform arrowSpawnPoint; // Arrow spawn point
private bool isAbilityActive = false; // Flag to check if an ability is active
    [SerializeField] private GameObject smokeBombPrefab; // Smoke bomb prefab
    [SerializeField] private GameObject showerOfArrowsPrefab; // Shower of arrows prefab

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        // Initialize cooldowns
        cooldownTimers = new Dictionary<string, float>
        {
            { "Arrow", 0f },
            { "SmokeBomb", 0f },
            { "Dash", 0f },
            { "ShowerOfArrows", 0f }
        };

        cooldownDurations = new Dictionary<string, float>
        {
            { "Arrow", 1f },
            { "SmokeBomb", 10f },
            { "Dash", 5f },
            { "ShowerOfArrows", 15f }
        };
    }

    // Update is called once per frame
    private void Update()
    {
        base.Update(); // Call the parent class's Update method to handle shared logic
        HandleCooldowns();
        HandleInputs();
        UpdateDashSpeed(); // Add this line to update dash speed
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
        if (Input.GetMouseButtonDown(1) && cooldownTimers["Arrow"] <= 0)
        {
           StartCoroutine(DelayArrow(1f));
        }
        if (Input.GetMouseButtonDown(1) && cooldownTimers["Arrow"] > 0)
        {
            Debug.Log("Arrow ability still in cooldown.");
        }

        if (Input.GetKeyDown(KeyCode.W) && cooldownTimers["SmokeBomb"] <= 0)
        {
           UseSmokeBomb();
        }
        if (Input.GetKeyDown(KeyCode.W) && cooldownTimers["SmokeBomb"] > 0)
        {
            Debug.Log("Smoke Bomb still in cooldown.");
        }

        if (Input.GetKeyDown(KeyCode.Q) && cooldownTimers["Dash"] <= 0)
        {
            Vector3 targetPosition = GetMouseWorldPosition();
            UseDash(targetPosition);
        }
        if (Input.GetKeyDown(KeyCode.Q) && cooldownTimers["Dash"] > 0)
        {
            Debug.Log("Dash still in cooldown.");
        }

        if (Input.GetKeyDown(KeyCode.E) && cooldownTimers["ShowerOfArrows"] <= 0)
        {
            Vector3 targetPosition = GetMouseWorldPosition();
            UseShowerOfArrows(targetPosition);
        }
        if (Input.GetKeyDown(KeyCode.E) && cooldownTimers["ShowerOfArrows"] > 0)
        {
            Debug.Log("Shower of Arrows still in cooldown.");
        }
    }

    // Abilities
 private void UseArrow()
{
    Debug.Log("Using Arrow ability!");

    if (animator == null)
    {
        Debug.LogError("Animator is not assigned! Ensure it's attached in the Inspector.");
        return;
    }

    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    if (Physics.Raycast(ray, out RaycastHit hit))
    {
        if (hit.collider.CompareTag("Enemy"))
        {
            RotateToFireTarget(); // Rotate towards the target
            Debug.Log($"Enemy clicked: {hit.collider.name}");
            animator.SetTrigger("ShootingTrigger");

            if (arrowPrefab != null && arrowSpawnPoint != null)
            {
                InstantiateProjectile(arrowSpawnPoint, hit.point);
                cooldownTimers["Arrow"] = cooldownDurations["Arrow"];
            }
            else
            {
                Debug.LogError("Arrow prefab or spawn point is not assigned!");
            }
        }
        else
        {
            Debug.Log("Clicked object is not an enemy!");
        }
    }
    else
    {
        Debug.Log("No target selected!");
    }
}

private IEnumerator DelayArrow(float delay){
    yield return new WaitForSeconds(delay);
    UseArrow();
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

    private void UseShowerOfArrows(Vector3 targetPosition)
    {
        Debug.Log($"Using Shower of Arrows ability at: {targetPosition}");

        if (showerOfArrowsPrefab != null)
        {
            Instantiate(showerOfArrowsPrefab, targetPosition, Quaternion.identity);
            cooldownTimers["ShowerOfArrows"] = cooldownDurations["ShowerOfArrows"];
        }
        else
        {
            Debug.LogError("Shower of Arrows prefab is not assigned!");
        }
    }

    // Helper to get the world position from the mouse click
    private Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return hit.point;
        }
        return Vector3.zero;
    }
   void InstantiateProjectile(Transform spawnPoint, Vector3 targetPosition)
{
    var arrow = Instantiate(arrowPrefab, spawnPoint.position, spawnPoint.rotation) as GameObject;
    var arrowScript = arrow.GetComponent<ArrowScript>();
    if (arrowScript != null)
    {
        arrowScript.SetTarget(targetPosition);
    }
}

    private void UseSmokeBomb()
    {
        Debug.Log("Using Smoke Bomb ability!");
        animator.SetTrigger("SmokeBombTrigger"); // Trigger the animation

        // Start a coroutine to execute the ability after a delay
        StartCoroutine(UseSmokeBombWithDelay());
    }

    private IEnumerator UseSmokeBombWithDelay()
    {
        // Optional: Wait for the animation to finish
        yield return new WaitForSeconds(2); // Replace with the actual animation name

        // Execute the Smoke Bomb ability
        if (smokeBombPrefab != null)
        {
            Instantiate(smokeBombPrefab, transform.position, Quaternion.identity);
            cooldownTimers["SmokeBomb"] = cooldownDurations["SmokeBomb"];
        }
        else
        {
            Debug.LogError("Smoke Bomb prefab is not assigned!");
        }
    }

    


    private void UseDash(Vector3 targetPosition)
    {
        Debug.Log($"Using Dash ability to: {targetPosition}");
        animator.SetBool("Dashing", true);

        isDashing = true; // Set the dashing flag to true
        //navMeshAgent.speed = originalSpeed * 2; // Double the speed for the dash
        navMeshAgent.SetDestination(targetPosition); // Set the destination for the dash
        cooldownTimers["Dash"] = cooldownDurations["Dash"];
    }

    private void UpdateDashSpeed()
    {
        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
        {
            if (!navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude == 0f)
            {
               
                animator.SetBool("Dashing", false);
               
                isDashing = false; // Reset the dashing flag
            }
        }
    }
}