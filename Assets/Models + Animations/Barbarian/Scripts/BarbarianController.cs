using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barbarian : WandererController
{
    private Dictionary<string, float> cooldownTimers;
    private Dictionary<string, float> cooldownDurations;

    private bool isAbilityActive = false;

    // Ability properties
    public float bashRange = 3f;
    public int bashDamage = 20;

    private bool isShieldActive = false;
    // public GameObject shieldEffect;
    // public GameObject maelstromEffect;

    public float chargeSpeed = 10f;
    public int bossDamage = 20;
    private LayerMask walkableLayer;

    private bool isCharging = false;
    private Vector3 chargeTarget;

    // Animation duration mapping
    private Dictionary<string, float> animationDurations;

    protected override void Start()
    {
        base.Start();
        walkableLayer = LayerMask.GetMask("Terrain");

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

        // Define animation durations (adjust based on your animations)
        animationDurations = new Dictionary<string, float>
        {
            { "Bash", 0.5f },
            { "Shield", 1f },
            { "IronMaelstrom", 1f },
            { "Charge", 0.8f }
        };
    }

    private void Update()
    {
        base.Update();
        HandleCooldowns();
        HandleInputs();

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
        // Create a copy of the keys as a list to prevent modification issues
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
            GameObject target = hit.collider.gameObject;
            if (target.CompareTag("Demon") || target.CompareTag("Minion"))
            {
                if (Vector3.Distance(transform.position, target.transform.position) <= bashRange)
                {
                    Bash(target);
                }
                else
                {
                    Debug.Log("Enemy is out of range for Bash!");
                }
            }
        }
    }

    private void Bash(GameObject enemy)
    {
        if (selectedTarget == null) return;

        float distance = Vector3.Distance(transform.position, selectedTarget.position);
        if (distance <= bashRange)
        {
            IHealth targetHealth = selectedTarget.GetComponent<IHealth>();
            targetHealth?.TakeDamage(bashDamage);

            animator.SetTrigger("BasicTrigger");
            StartCoroutine(AbilityTiming("Bash"));
        }
    }

    private void ActivateShield()
    {
        if (isShieldActive) return;

        animator.SetTrigger("DefensiveTrigger");
        isShieldActive = true;

        // if (shieldEffect != null) shieldEffect.SetActive(true);

        StartCoroutine(ShieldDuration());
        StartCoroutine(AbilityTiming("Shield"));
    }

    private IEnumerator ShieldDuration()
    {
        yield return new WaitForSeconds(3f);
        isShieldActive = false;
        // if (shieldEffect != null) shieldEffect.SetActive(false);
    }

    private void IronMaelstrom()
    {
        animator.SetTrigger("WildCardTrigger");
        StartCoroutine(AbilityTiming("IronMaelstrom"));

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 3f, enemyLayer);
        foreach (Collider collider in hitColliders)
        {
            IHealth targetHealth = collider.GetComponent<IHealth>();
            targetHealth?.TakeDamage(30);
        }

        // if (maelstromEffect != null)
        // {
        //     Instantiate(maelstromEffect, transform.position, Quaternion.identity);
        // }
    }

    private void InitiateCharge()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, walkableLayer.value))
        {
            chargeTarget = hit.point;
            Charge();
        }
    }

    private void Charge()
    {
        navMeshAgent.isStopped = true;
        navMeshAgent.enabled = false;

        animator.SetTrigger("UltimateTrigger");

        Vector3 direction = (chargeTarget - transform.position).normalized;
        StartCoroutine(PerformCharge(direction));
    }

    private IEnumerator PerformCharge(Vector3 direction)
    {
        float chargeDuration = Vector3.Distance(transform.position, chargeTarget) / chargeSpeed;
        float elapsedTime = 0f;

        while (elapsedTime < chargeDuration)
        {
            transform.position += direction * chargeSpeed * Time.deltaTime;
            HandleChargeCollisions();
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        navMeshAgent.enabled = true;
        navMeshAgent.isStopped = false;
        animator.SetTrigger("EndUltimate");
    }

    private void HandleChargeCollisions()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 1f);
        foreach (Collider collider in hitColliders)
        {
            if (collider.CompareTag("Minion") || collider.CompareTag("Demon"))
            {
                IHealth targetHealth = collider.GetComponent<IHealth>();
                targetHealth?.TakeDamage(bossDamage);
            }
        }
    }

    public override void TakeDamage(int damage)
    {
        if (isShieldActive) return;
        base.TakeDamage(damage);
    }

    private IEnumerator AbilityTiming(string ability)
    {
        isAbilityActive = true;
        yield return new WaitForSeconds(animationDurations[ability]);
        isAbilityActive = false;
        cooldownTimers[ability] = cooldownDurations[ability];
    }
}
