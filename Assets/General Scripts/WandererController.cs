using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class WandererController : MonoBehaviour, IHealth
{
    // Shared properties for all Wanderers
    protected NavMeshAgent navMeshAgent;
    protected Animator animator;

    public int maxHealth = 100;
    public int currentHealth = 100;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    // XP and Leveling
    public int currentXP = 0;
    public int currentLevel = 1;
    public int maxLevel = 4;
    public int abilityPoints = 0;

    protected Transform selectedTarget;

    // Inventory system
    [SerializeField]
    public int runeFragments;
    public int RuneFragments => runeFragments; // Read-only property for Rune Fragments
    public int healingPotions;
    public GameObject potionModel;

    public GameObject potionIconPrefab; // Prefab for the potion icon
    public Transform potionPanel; // Parent panel for potion icons


    // Abilities
    private HashSet<string> unlockedAbilities = new HashSet<string>();

    public LayerMask enemyLayer;

    public GameObject winPanel;  // Reference to the Win panel
    public GameObject losePanel; // Reference to the Lose panel


    protected virtual void Awake()
    {
        // Ensure the Wanderer persists across scenes
        DontDestroyOnLoad(gameObject);
    }


    protected virtual void Start()
    {
        enemyLayer = LayerMask.GetMask("enemyLayer");

        currentHealth = maxHealth;

        runeFragments = 0;
        healingPotions = 0;

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

        // Ensure the potion is disabled at the start
        if (potionModel != null)
        {
            potionModel.SetActive(false);
        }
    }

    protected virtual void Update()
    {
        HandleMouseClick();

        if (navMeshAgent != null && animator != null)
        {
            float speed = navMeshAgent.velocity.magnitude;
            animator.SetFloat("Speed", speed);
        }
    }

    protected virtual void HandleMouseClick()
    {
        if (navMeshAgent == null || !navMeshAgent.isActiveAndEnabled)
        {
            Debug.LogWarning("NavMeshAgent is not active or enabled.");
            return;
        }

        if (Input.GetMouseButtonDown(0)) // Left mouse button for movement
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                navMeshAgent.SetDestination(hit.point);
            }
        }

        if (Input.GetMouseButtonDown(1)) // Right mouse button for target selection
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

        if (Input.GetKeyDown(KeyCode.F))
        {
            UseHealingPotion();
        }
    }

    public void SelectTarget(Transform target)
    {
        if (selectedTarget != null)
        {
            DeselectTarget(selectedTarget);
        }

        selectedTarget = target;
        HighlightTarget(selectedTarget, true);

        Debug.Log($"Selected Target: {selectedTarget.name}");
    }

    private void DeselectTarget(Transform target)
    {
        HighlightTarget(target, false);
        selectedTarget = null;
    }

    private void HighlightTarget(Transform target, bool highlight)
    {
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
        abilityPoints++;
        maxHealth += 100;
        currentHealth = maxHealth;

        Debug.Log($"Level Up! Current Level: {currentLevel}, Max Health: {maxHealth}, Ability Points: {abilityPoints}");
        FindObjectOfType<WandererHealthBar>()?.UpdateHealthBar(maxHealth, currentHealth);

    }

    public void UnlockAbility(string abilityName)
    {
        if (abilityPoints > 0)
        {
            abilityPoints--;
            Debug.Log($"Unlocked ability: {abilityName}. Remaining Ability Points: {abilityPoints}");
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

        UpdatePotionUI();
    }

    public void UseHealingPotion()
    {
        if (currentHealth == maxHealth || healingPotions == 0)
        {
            Debug.Log("Cannot use potion. Health is full or no potions left.");
            return;
        }

        // Trigger the use potion animation
        animator.SetTrigger("UsePotionTrigger");

        // Show the potion in hand
        if (potionModel != null)
        {
            potionModel.SetActive(true);
        }

        // Heal the character
        int healAmount = maxHealth / 2;
        currentHealth = Mathf.Clamp(currentHealth + healAmount, 0, maxHealth);

        healingPotions--; // Reduce potion count

        Debug.Log($"Used potion. Current health: {currentHealth}, Potions left: {healingPotions}");

        UpdatePotionUI();
        // Hide the potion after a delay (to match animation duration)
        StartCoroutine(HidePotionAfterUse());
    }

    private IEnumerator HidePotionAfterUse()
    {
        Debug.Log("HidePotionAfterUse coroutine started.");
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        if (potionModel != null)
        {
            Debug.Log("Potion model deactivated.");
            potionModel.SetActive(false);
        }
        else
        {
            Debug.LogError("Potion model is null.");
        }
    }


    private void UpdatePotionUI()
    {
        // Check if the potion UI panel or its child objects exist
        if (potionPanel == null)
        {
            Debug.LogWarning("Potion panel is missing! Cannot update the UI.");
            return;
        }

        // Clear existing potion icons safely
        foreach (Transform child in potionPanel.transform)
        {
            if (child != null) // Add a null check
            {
                Destroy(child.gameObject);
            }
        }

        // Re-create potion icons
        for (int i = 0; i < healingPotions; i++)
        {
            if (potionIconPrefab != null) // Ensure prefab exists
            {
                Instantiate(potionIconPrefab, potionPanel.transform);
            }
            else
            {
                Debug.LogWarning("Potion icon prefab is missing!");
            }
        }
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

    public bool HasRequiredRuneFragments(int requiredFragments)
    {
        return runeFragments >= requiredFragments;
    }

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
        navMeshAgent.isStopped = true;
        Debug.Log("Wanderer has died!");

        winPanel.SetActive(false);
        // search for panels with tag "HUD" and set them to false
        GameObject[] hudPanels = GameObject.FindGameObjectsWithTag("HUD");
        foreach (GameObject panel in hudPanels)
        {
            panel.SetActive(false);
        }
        losePanel.SetActive(true);

    }

    protected abstract void HandleInputs();
}
