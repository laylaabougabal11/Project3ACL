using UnityEngine;
using UnityEngine.AI;

public abstract class WandererController : MonoBehaviour
{
    // Shared properties for all Wanderers
    public int maxHealth = 100;
    protected int currentHealth;

    protected NavMeshAgent navMeshAgent; // Reference to NavMeshAgent
    protected Animator animator; // Reference to Animator component
    protected bool isDashing = false; // Flag to check if the Wanderer is dashing
    protected float originalSpeed; // Original speed of the Wanderer

    protected virtual void Start()
    {
        // Initialize health
        currentHealth = maxHealth;

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

        originalSpeed = navMeshAgent.speed; // Store the original speed
    }

    protected virtual void Update()
    {
        HandleMouseClick(); // Handle movement based on mouse click

        // Update movement animations based on agent velocity
        if (navMeshAgent != null && animator != null)
        {
            if (!isDashing) // Only update speed if not dashing
            {
                float speed = navMeshAgent.velocity.magnitude; // Get the speed from NavMeshAgent's velocity
                animator.SetFloat("Speed", speed); // Set Speed parameter for animations
            }
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