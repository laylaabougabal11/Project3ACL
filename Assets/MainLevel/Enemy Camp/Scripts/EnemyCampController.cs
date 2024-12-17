using System.Collections.Generic;
using UnityEngine;

public class EnemyCampController : MonoBehaviour
{
    [Header("Camp Settings")]
    public int maxAlertedMinions = 5;
    public int maxAlertedDemons = 1;

    [Header("Camp References")]
    private GameObject rune;
    private GameObject healingPotion;
    private List<MinionController> minions = new List<MinionController>();
    private List<DemonController> demons = new List<DemonController>();
    private List<IHealth> alertedEnemies = new List<IHealth>();

    private bool isCleared = false;

    void Start()
    {
        // Find the Rune and hide it
        rune = GetComponentInChildren<Transform>().FindChildWithTag("Rune");
        if (rune != null)
        {
            rune.SetActive(false);
        }
        else
        {
            Debug.LogWarning($"No Rune found in {gameObject.name}");
        }

        // Find the Healing Potion and activate it
        healingPotion = GetComponentInChildren<Transform>().FindChildWithTag("Potion");
        if (healingPotion != null)
        {
            healingPotion.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"No Potion found in {gameObject.name}");
        }//

        // Automatically find all enemies in the camp
        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            if (child.gameObject.layer == LayerMask.NameToLayer("enemyLayer"))
            {
                if (child.CompareTag("Minion"))
                {
                    MinionController minion = child.GetComponent<MinionController>();
                    if (minion != null) minions.Add(minion);
                }
                else if (child.CompareTag("Demon"))
                {
                    DemonController demon = child.GetComponent<DemonController>();
                    if (demon != null) demons.Add(demon);
                }
            }
        }

        if (minions.Count + demons.Count == 0)
        {
            Debug.LogWarning($"No enemies found in {gameObject.name}");
        }
    }

    void Update()
    {
        if (!isCleared)
        {
            UpdateAlertedEnemies();
        }
    }

    private void UpdateAlertedEnemies()
    {
        int alertedMinions = 0;
        int alertedDemons = 0;

        // Reset the current alerted enemies list
        alertedEnemies.Clear();

        // Alert minions up to the maximum allowed
        foreach (MinionController minion in minions)
        {
            if (minion == null || !minion.IsAlive) continue;

            if (minion.IsAlerted)
            {
                if (alertedMinions < maxAlertedMinions)
                {
                    alertedEnemies.Add(minion);
                    alertedMinions++;
                }
                else
                {
                    minion.BecomeNonAggressive();
                }
            }
            else if (alertedMinions < maxAlertedMinions)
            {
                minion.BecomeAlerted();
                alertedEnemies.Add(minion);
                alertedMinions++;
            }
        }

        // Alert demons up to the maximum allowed
        foreach (DemonController demon in demons)
        {
            if (demon == null || !demon.IsAlive) continue;

            if (demon.IsAlerted)
            {
                if (alertedDemons < maxAlertedDemons)
                {
                    alertedEnemies.Add(demon);
                    alertedDemons++;
                }
                else
                {
                    demon.BecomeNonAggressive();
                }
            }
            else if (alertedDemons < maxAlertedDemons)
            {
                demon.BecomeAlerted();
                alertedEnemies.Add(demon);
                alertedDemons++;
            }
        }
    }

    public void DeregisterEnemy(GameObject enemy)
    {
        if (enemy.TryGetComponent(out MinionController minion))
        {
            minions.Remove(minion);
        }
        else if (enemy.TryGetComponent(out DemonController demon))
        {
            demons.Remove(demon);
        }

        alertedEnemies.Remove(enemy.GetComponent<IHealth>());

        Debug.Log($"Enemy defeated in {gameObject.name}. Remaining enemies: {minions.Count + demons.Count}");

        // Check if all enemies are defeated
        if (minions.Count + demons.Count == 0 && !isCleared)
        {
            ClearCamp();
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
