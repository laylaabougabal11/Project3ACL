using UnityEngine;
using UnityEngine.AI;

public abstract class movement : MonoBehaviour
{
    // Shared properties for all Wanderers
    public int maxHealth = 100;
    protected int currentHealth;
    public int healingPotions; // Track the number of healing potions available

    protected NavMeshAgent navMeshAgent; // Reference to NavMeshAgent
    protected Animator animator; // Reference to Animator component

    protected virtual void Start()
    {
        // Initialize health
        currentHealth = 30;

        healingPotions = 3; // Set the initial number of healing potions

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

        if (navMeshAgent != null && animator != null)
        {
            // Calculate the current speed of the NavMeshAgent
            float speed = navMeshAgent.velocity.magnitude;

            // Update the Speed parameter in the Animator
            animator.SetFloat("Speed", speed);
        }

        // Check if the Wanderer is stationary
        if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance && navMeshAgent.velocity.magnitude <= 0.1f)
        {
            animator.SetFloat("Speed", 0f); // Transition to Idle
        }

        // Debug: Use potion with a key press (optional testing input)
        if (Input.GetKeyDown(KeyCode.T))
        {
            UseHealingPotion();
        }
    }

    protected virtual void HandleMouseClick()
    {
        if (Input.GetMouseButtonDown(1)) // Right mouse button for movement
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                navMeshAgent.SetDestination(hit.point); // Set NavMeshAgent's destination

                // Immediately update the Animator to transition to movement
                animator.SetFloat("Speed", navMeshAgent.speed); // Set Speed for walking/running

            }
        }
    }



    // Healing potion logic
    public void UseHealingPotion()
    {
        if (healingPotions <= 0)
        {
            Debug.Log("No healing potions left!");
            return;
        }

        if (currentHealth >= maxHealth)
        {
            Debug.Log("Health is already full!");
            return;
        }

        // Calculate the healing amount
        int healingAmount = maxHealth / 2;
        currentHealth = Mathf.Min(currentHealth + healingAmount, maxHealth); // Prevent exceeding max health
        healingPotions--; // Decrease the potion count

        Debug.Log($"Healing potion used! Current Health: {currentHealth}. Potions left: {healingPotions}");

        // Optionally, play a healing animation
        animator.SetTrigger("UsePotionTrigger");
    }

    // Damage handling (shared logic)
    public virtual void TakeDamage(int damage)
    {
        currentHealth -= damage;
        animator.SetTrigger("GetDamagedTrigger");
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