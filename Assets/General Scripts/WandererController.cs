using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class WandererController : MonoBehaviour, IHealth
{
    // Shared properties for all Wanderers
    protected NavMeshAgent navMeshAgent; // Reference to NavMeshAgent
    protected Animator animator; // Reference to Animator component

    public int maxHealth = 100;
    public int currentHealth = 40;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    // XP and Leveling
    public int currentXP = 0;
    public int currentLevel = 1;
    public int maxLevel = 4;
    public int abilityPoints = 0; // Points to unlock abilities

    protected Transform selectedTarget; // Current selected target



    // Inventory system
    private int runeFragments;
    public int healingPotions;


    // Abilities
    private HashSet<string> unlockedAbilities = new HashSet<string>(); // Track unlocked abilities

    protected virtual void Start()
    {
        // Initialize health
        currentHealth = 100;

        runeFragments = 0;
        healingPotions = 0;

        // Get components on the Wanderer GameObject
        navMeshAgent = GetComponent<NavMeshAgent>();
        if (navMeshAgent == null)
        {
            Debug.LogError("NavMeshAgent not found on Wanderer!");
        }

        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator not found on Wanderer!");
        }
    }

    protected virtual void Update()
    {
        HandleMouseClick(); // Handle movement based on mouse click

        // Update movement animations based on agent velocity
        if (navMeshAgent != null && animator != null)
        {
            float speed = navMeshAgent.velocity.magnitude; // Get the speed from NavMeshAgent's velocity
            animator.SetFloat("Speed", speed); // Set Speed parameter for animations
        }

        HandleTargetSelection();

    }

    protected virtual void HandleMouseClick()
    {

        // Ensure NavMeshAgent is active and enabled
        if (navMeshAgent == null || !navMeshAgent.isActiveAndEnabled)
        {
            Debug.LogWarning("NavMeshAgent is not active or enabled.");
            return;
        }
        if (Input.GetMouseButtonDown(0)) // Right mouse button for movement
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                navMeshAgent.SetDestination(hit.point); // Set NavMeshAgent's destination to the point clicked
            }
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            UseHealingPotion();
        }
    }

    private void HandleTargetSelection()
    {
        if (Input.GetMouseButtonDown(0)) // Left mouse button
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.CompareTag("Minion") || hit.collider.CompareTag("Demon"))
                {
                    SelectTarget(hit.collider.transform);
                }
            }
        }
    }

    private void SelectTarget(Transform target)
    {
        // Deselect previous target
        if (selectedTarget != null)
        {
            DeselectTarget(selectedTarget);
        }

        // Select new target
        selectedTarget = target;
        HighlightTarget(selectedTarget, true); // Highlight the selected target

        Debug.Log($"Selected Target: {selectedTarget.name}");
    }

    private void DeselectTarget(Transform target)
    {
        HighlightTarget(target, false); // Remove highlight
        selectedTarget = null;
    }

    private void HighlightTarget(Transform target, bool highlight)
    {
        // Optional: Add a visual effect or outline to the target
        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = highlight ? Color.red : Color.white;
        }
    }

    public Transform GetSelectedTarget()
    {
        return selectedTarget;
    }

    public void GainXP(int xp)
    {
        if (currentLevel >= maxLevel)
        {
            Debug.Log("Wanderer has reached max level and cannot gain more XP.");
            return;
        }

        currentXP += xp;
        Debug.Log($"Gained {xp} XP. Current XP: {currentXP}");

        int xpToLevelUp = 100 * currentLevel;

        while (currentXP >= xpToLevelUp && currentLevel < maxLevel)
        {
            LevelUp();
            currentXP -= xpToLevelUp;
            xpToLevelUp = 100 * currentLevel;
        }
    }
    private void LevelUp()
    {
        currentLevel++;
        abilityPoints++; // Gain an ability point
        maxHealth += 100; // Increase max health
        currentHealth = maxHealth; // Refill health

        Debug.Log($"Level Up! Current Level: {currentLevel}, Max Health: {maxHealth}, Ability Points: {abilityPoints}");
    }

    public void UnlockAbility(string abilityName)
    {
        if (abilityPoints > 0)
        {
            abilityPoints--;
            Debug.Log($"Unlocked ability: {abilityName}. Remaining Ability Points: {abilityPoints}");
            // Logic for unlocking the ability permanently (e.g., enabling it in the HUD or skills list)
        }
        else
        {
            Debug.Log("Not enough ability points to unlock this ability.");
        }
    }

    public void AddHealingPotion()
    {
        healingPotions++;
        Debug.Log($"Healing Potions: {healingPotions}");
    }

    public void UseHealingPotion()
    {
        if (currentHealth == maxHealth || healingPotions == 0)
        {
            Debug.Log("Cannot use potion. Health is full or no potions left.");
            return;
        }

        animator.SetTrigger("UsePotionTrigger"); // Trigger the upper body animation

        // Heal the character
        int healAmount = maxHealth / 2;
        currentHealth = Mathf.Clamp(currentHealth + healAmount, 0, maxHealth);

        healingPotions--; // Decrease potion count

        Debug.Log($"Used potion. Current health: {currentHealth}, Potions left: {healingPotions}");
    }

    public void AddRuneFragment()
    {
        runeFragments++;
        Debug.Log($"Rune Fragments: {runeFragments}");

        if (runeFragments >= 3)
        {
            Debug.Log("You have enough Rune Fragments to unlock the gate!");
        }
    }

    // Damage handling (shared logic)
    public virtual void TakeDamage(int damage)
    {
        animator.SetTrigger("GetDamagedTrigger");

        currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHealth);
        Debug.Log($"Health remaining: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        animator.SetTrigger("DieTrigger");
        // Disable the NavMeshAgent
        navMeshAgent.isStopped = true;
        Debug.Log("Wanderer has died!");
    }

    // Abstract method for class-specific input handling (to be implemented by subclasses)
    protected abstract void HandleInputs();
}
