using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SorcererController : WandererController
{
    private Dictionary<string, float> cooldownTimers;
    private Dictionary<string, float> cooldownDurations;

    private bool isAbilityActive = false;

    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private Transform fireballSpawnPoint;
    [SerializeField] private GameObject clonePrefab;
    [SerializeField] private GameObject infernoPrefab;
    public GameObject cloneExplosionEffectPrefab;
    protected override void Start()
    {
        base.Start();


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

                if (cooldownTimers[key] < 0)
                {
                    cooldownTimers[key] = 0;
                }
            }
        }
    }

    protected override void HandleInputs()
    {
        if (Input.GetMouseButtonDown(1) && cooldownTimers["Fireball"] <= 0)
        {
            TryCastFireball();
        }

        if (Input.GetKeyDown(KeyCode.Q) && cooldownTimers["Teleport"] <= 0)
        {
            TryTeleport();
        }

        if (Input.GetKeyDown(KeyCode.W) && cooldownTimers["Clone"] <= 0)
        {
            TryCreateClone();
        }

        if (Input.GetKeyDown(KeyCode.E) && cooldownTimers["Inferno"] <= 0)
        {
            TryActivateInferno();
        }
    }

    private void TryCastFireball()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.gameObject.layer == LayerMask.NameToLayer("enemyLayer"))
        {
            animator.SetTrigger("BasicTrigger");
            StartCoroutine(CastFireballWithDelay(0.5f, hit.point));
        }
        else
        {
            Debug.LogWarning("Fireball requires an enemy target!");
        }
    }

    private IEnumerator CastFireballWithDelay(float delay, Vector3 targetPosition)
    {
        yield return new WaitForSeconds(delay);

        if (fireballPrefab != null && fireballSpawnPoint != null)
        {
            GameObject fireball = Instantiate(fireballPrefab, fireballSpawnPoint.position, Quaternion.identity);
            Fireball fireballScript = fireball.GetComponent<Fireball>();
            fireballScript?.Initialize(targetPosition, 5); // Fireball deals 5 damage
            ApplyDamageToEnemies(targetPosition, 4f, 5); // Fireball might damage nearby enemies
            cooldownTimers["Fireball"] = cooldownDurations["Fireball"];
        }
    }

    private void TryTeleport()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit) && NavMesh.SamplePosition(hit.point, out var navHit, 1.0f, NavMesh.AllAreas))
        {
            transform.position = navHit.position;
            cooldownTimers["Teleport"] = cooldownDurations["Teleport"];
            Debug.Log($"Teleported to {navHit.position}");
        }
        else
        {
            Debug.LogWarning("Invalid teleport destination!");
        }
    }

    private void TryCreateClone()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit) && NavMesh.SamplePosition(hit.point, out var navHit, 1.0f, NavMesh.AllAreas))
        {
            GameObject clone = Instantiate(clonePrefab, navHit.position, Quaternion.identity);
            StartCoroutine(CloneLifecycle(clone));
            cooldownTimers["Clone"] = cooldownDurations["Clone"];
        }
        else
        {
            Debug.LogWarning("Invalid clone creation position!");
        }
    }

    private IEnumerator CloneLifecycle(GameObject clone)
    {
        yield return new WaitForSeconds(5f);

        ApplyDamageToEnemies(clone.transform.position, 5f, 10); // Clone explosion deals 10 damage

        if (cloneExplosionEffectPrefab != null)
        {
            Instantiate(cloneExplosionEffectPrefab, clone.transform.position, Quaternion.identity);
        }

        Destroy(clone);
    }

    private void TryActivateInferno()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            StartCoroutine(ActivateInferno(hit.point));
        }
        else
        {
            Debug.LogWarning("Invalid Inferno position!");
        }
    }

    private IEnumerator ActivateInferno(Vector3 position)
    {
        GameObject inferno = Instantiate(infernoPrefab, position, Quaternion.identity);

        for (int i = 0; i < 5; i++)
        {
            yield return new WaitForSeconds(1f);
            ApplyDamageToEnemies(position, 5f, i == 0 ? 10 : 2); // Initial damage: 10, tick damage: 2
        }

        Destroy(inferno);
        cooldownTimers["Inferno"] = cooldownDurations["Inferno"];
    }

    private void ApplyDamageToEnemies(Vector3 position, float radius, int damage)
    {
        Collider[] hitColliders = Physics.OverlapSphere(position, radius, enemyLayer);

        foreach (Collider collider in hitColliders)
        {
            IHealth targetHealth = collider.GetComponent<IHealth>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(damage);
                Debug.Log($"Damaged {collider.name} for {damage} points.");
            }
        }
    }

}
