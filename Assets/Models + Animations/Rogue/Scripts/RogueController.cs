using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RogueController : WandererController
{
    // Cooldown management
    private Dictionary<string, float> cooldownTimers;
    private Dictionary<string, float> cooldownDurations;

    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform arrowSpawnPoint;
    [SerializeField] private GameObject smokeBombPrefab;
    [SerializeField] private GameObject showerOfArrowsPrefab;

    private bool isAbilityActive = false;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

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
            { "ShowerOfArrows", 10f }
        };
    }

    private void Update()
    {
        base.Update();
        HandleCooldowns();

        if (!isAbilityActive)
        {
            HandleInputs();
        }

        if (isAbilityActive)
        {
            AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
            if (!currentState.IsTag("Ability"))
            {
                isAbilityActive = false;
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

                if (cooldownTimers[key] < 0)
                {
                    cooldownTimers[key] = 0;
                }
            }
        }
    }

    protected override void HandleInputs()
    {
        if (Input.GetMouseButtonDown(1) && cooldownTimers["Arrow"] <= 0)
        {
            TryUseArrow();
        }

        if (Input.GetKeyDown(KeyCode.Q) && cooldownTimers["SmokeBomb"] <= 0)
        {
            UseSmokeBomb();
        }

        if (Input.GetKeyDown(KeyCode.W) && cooldownTimers["Dash"] <= 0)
        {
            Vector3 targetPosition = GetMouseWorldPosition();
            UseDash(targetPosition);
        }

        if (Input.GetKeyDown(KeyCode.E) && cooldownTimers["ShowerOfArrows"] <= 0)
        {
            Vector3 targetPosition = GetMouseWorldPosition();
            UseShowerOfArrows(targetPosition);
        }
    }

    private void TryUseArrow()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Check if the ray hits an enemy on the enemyLayer
        if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.gameObject.layer == LayerMask.NameToLayer("enemyLayer"))
        {
            // Trigger the shooting animation
            animator.SetTrigger("ShootingTrigger");

            // Start the coroutine to fire the arrow after a delay
            StartCoroutine(DelayArrow(0.5f, hit.point)); // Pass the hit point as the target position
        }
        else
        {
            Debug.LogWarning("Arrow requires an enemy target!");
        }
    }


    private IEnumerator DelayArrow(float delay, Vector3 targetPosition)
    {
        yield return new WaitForSeconds(delay);

        // Ensure the arrow prefab and spawn point are valid
        if (arrowPrefab != null && arrowSpawnPoint != null)
        {
            // Instantiate the arrow at the spawn point
            GameObject arrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, Quaternion.identity);

            // Set the arrow's target position and damage
            ArrowScript arrowScript = arrow.GetComponent<ArrowScript>();
            if (arrowScript != null)
            {
                arrowScript.SetTarget(targetPosition, 5); // Arrow deals 5 damage
                // Debug.Log($"Arrow fired towards {targetPosition}");
            }

            // Apply cooldown
            cooldownTimers["Arrow"] = cooldownDurations["Arrow"];
        }
        else
        {
            Debug.LogError("Arrow prefab or spawn point is not assigned!");
        }
    }



    private void InstantiateProjectile(Transform spawnPoint, Vector3 targetPosition)
    {
        GameObject arrow = Instantiate(arrowPrefab, spawnPoint.position, spawnPoint.rotation);
        ArrowScript arrowScript = arrow.GetComponent<ArrowScript>();
        if (arrowScript != null)
        {
            arrowScript.SetTarget(targetPosition, 5); // Arrow deals 5 damage
        }
    }

    private void UseSmokeBomb()
    {
        animator.SetTrigger("SmokeBombTrigger");

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
        animator.SetBool("Dashing", true);

        navMeshAgent.SetDestination(targetPosition);
        cooldownTimers["Dash"] = cooldownDurations["Dash"];

        StartCoroutine(ResetDashState());
    }

    private IEnumerator ResetDashState()
    {
        while (navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance)
        {
            yield return null;
        }

        animator.SetBool("Dashing", false);
    }

    private void UseShowerOfArrows(Vector3 targetPosition)
    {
        if (showerOfArrowsPrefab != null)
        {
            Instantiate(showerOfArrowsPrefab, targetPosition, Quaternion.identity);

            // Damage all enemies in the area
            Collider[] hitColliders = Physics.OverlapSphere(targetPosition, 5f, enemyLayer);
            foreach (Collider collider in hitColliders)
            {
                IHealth targetHealth = collider.GetComponent<IHealth>();
                if (targetHealth != null)
                {
                    targetHealth.TakeDamage(10); // 10 damage for Shower of Arrows
                    Debug.Log($"Shower of Arrows hit: {collider.name} for 10 damage.");
                }
            }

            cooldownTimers["ShowerOfArrows"] = cooldownDurations["ShowerOfArrows"];
        }
        else
        {
            Debug.LogError("Shower of Arrows prefab is not assigned!");
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return hit.point;
        }
        return Vector3.zero;
    }
}
