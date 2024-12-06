using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Barbarian : WandererController
{
    // Cooldowns and ability durations
    private Dictionary<string, float> cooldownTimers;
    private Dictionary<string, float> cooldownDurations;

    // Ability state
    private bool isShieldActive = false;

    // Shield visual effect
    public GameObject shieldEffect;

    // Damage area for Iron Maelstrom
    public GameObject maelstromEffect;

    // Movement for Charge
    public float chargeSpeed = 10f;

    private Animator animator;

    protected override void Start()
    {
        base.Start();

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

        animator = GetComponent<Animator>(); // Get the Animator component

    }

    private void Update()
    {
        base.Update(); // Call the parent class's Update method to handle shared logic
        HandleCooldowns();
        HandleInputs();

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
        if (Input.GetMouseButtonDown(0) && cooldownTimers["Bash"] <= 0)
        {
            Bash();
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
            Charge();
        }
    }

    private void Bash()
    {
        animator.SetTrigger("BasicTrigger"); // Trigger the Basic Attack animation

        Debug.Log("Bash activated!");
        cooldownTimers["Bash"] = cooldownDurations["Bash"];
        // Add logic for dealing damage
    }

    private void ActivateShield()
    {
        animator.SetTrigger("DefensiveTrigger"); // Trigger the Defensive ability animation

        if (isShieldActive) return;

        Debug.Log("Shield activated!");
        isShieldActive = true;

        if (shieldEffect != null)
        {
            shieldEffect.SetActive(true);
        }

        StartCoroutine(ShieldDuration());
        cooldownTimers["Shield"] = cooldownDurations["Shield"];
        // Additional logic for defensive action
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
        Debug.Log("Iron Maelstrom activated!");
        cooldownTimers["IronMaelstrom"] = cooldownDurations["IronMaelstrom"];

        if (maelstromEffect != null)
        {
            Instantiate(maelstromEffect, transform.position, Quaternion.identity);
        }

        animator.SetTrigger("WildCardTrigger"); // Trigger Wild Card animation
        // Logic for the ability
    }

    private void Charge()
    {
        Debug.Log("Charge activated!");
        cooldownTimers["Charge"] = cooldownDurations["Charge"];
        animator.SetTrigger("UltimateTrigger"); // Trigger Ultimate animation
        // Logic for executing the ultimate ability
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
