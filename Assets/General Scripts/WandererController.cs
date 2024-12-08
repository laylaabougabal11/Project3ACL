using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class WandererController : MonoBehaviour
{
    // Shared properties for all Wanderers
    protected NavMeshAgent navMeshAgent; // Reference to NavMeshAgent
    protected Animator animator; // Reference to Animator component

    public int maxHealth = 100;
    protected int currentHealth;
    // Inventory system
    private int runeFragments;
    public int healingPotions;
    // Leveling and XP
    public int currentLevel = 1; // Start at level 1
    public int currentXP = 0; // Start with 0 XP
    public int maxXP = 100; // XP required for the next level
    public int abilityPoints = 0; // Points available to unlock abilities



    // Abilities
    private HashSet<string> unlockedAbilities = new HashSet<string>(); // Track unlocked abilities

    protected virtual void Start()
    {
        // Initialize health
        currentHealth = 10;

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
    }

    protected virtual void HandleMouseClick()
    {
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

    public void GainXP(int xpGained)
    {
        if (currentLevel >= 4)
        {
            Debug.Log("Max level reached. Cannot gain more XP.");
            return;
        }

        currentXP += xpGained;

        // Handle leveling up if XP exceeds maxXP
        while (currentXP >= maxXP && currentLevel < 4)
        {
            LevelUp();
        }

        Debug.Log($"Current XP: {currentXP}/{maxXP}, Level: {currentLevel}");
    }

    private void LevelUp()
    {
        // Calculate overflow XP
        currentXP -= maxXP;

        // Increment level
        currentLevel++;
        abilityPoints++;

        // Increase max HP
        maxHealth += 100;
        currentHealth = maxHealth; // Refill health

        // Update max XP
        maxXP = 100 * currentLevel;

        Debug.Log($"Leveled up! New Level: {currentLevel}, Max HP: {maxHealth}, Ability Points: {abilityPoints}");

        // Prevent further leveling if max level reached
        if (currentLevel >= 4)
        {
            currentXP = 0; // Cap XP at max level
            Debug.Log("Maximum level reached. XP gain disabled.");
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
        currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
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

        currentHealth -= damage;
        Debug.Log($"Health remaining: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        animator.SetTrigger("DieTrigger");
        Debug.Log("Wanderer has died!");
    }

    // Abstract method for class-specific input handling (to be implemented by subclasses)
    protected abstract void HandleInputs();
}
