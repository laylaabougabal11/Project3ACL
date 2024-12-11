using System.Collections.Generic;
using UnityEngine;

public class EnemyCampController : MonoBehaviour
{
    private GameObject rune; // The Rune in the camp
    private GameObject healingPotion; // Healing Potion in the camp
    private List<GameObject> enemies = new List<GameObject>(); // List of enemies in the camp
    private int activeEnemyCount; // Tracks the number of active enemies

    private bool isCleared = false; // Tracks if the camp is cleared

    void Start()
    {
        // Automatically find the Rune in the camp hierarchy
        rune = GetComponentInChildren<Transform>().FindChildWithTag("Rune");
        if (rune != null)
        {
            rune.SetActive(false); // Hide the Rune initially
        }
        else
        {
            Debug.LogWarning($"No Rune found in {gameObject.name}");
        }

        // Automatically find the Healing Potion in the camp hierarchy
        healingPotion = GetComponentInChildren<Transform>().FindChildWithTag("Potion");
        if (healingPotion != null)
        {
            healingPotion.SetActive(true); // Activate the Potion
        }
        else
        {
            Debug.LogWarning($"No Potion found in {gameObject.name}");
        }

        // Automatically find all enemies in the camp hierarchy by layer
        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            if (child.gameObject.layer == LayerMask.NameToLayer("enemyLayer"))
            {
                enemies.Add(child.gameObject);
            }
        }

        activeEnemyCount = enemies.Count;

        if (activeEnemyCount == 0)
        {
            Debug.LogWarning($"No enemies found in {gameObject.name}");
        }
    }

    public void DeregisterEnemy(GameObject enemy)
    {
        if (enemies.Contains(enemy))
        {
            enemies.Remove(enemy);
            activeEnemyCount--;

            Debug.Log($"Enemy defeated in {gameObject.name}. Remaining enemies: {activeEnemyCount}");

            // Check if all enemies are defeated
            if (activeEnemyCount <= 0 && !isCleared)
            {
                ClearCamp();
            }
        }
    }

    private void ClearCamp()
    {
        isCleared = true;
        Debug.Log($"Camp {gameObject.name} cleared!");
        ActivateRune();
    }

    private void ActivateRune()
    {
        if (rune != null)
        {
            rune.SetActive(true);
            Debug.Log("Rune activated!");
        }
    }
}
