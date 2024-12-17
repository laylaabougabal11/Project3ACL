using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RunePanelController : MonoBehaviour
{
    public GameObject runeIconPrefab; // Prefab for the rune icon
    public Transform runePanel;       // Parent panel for rune icons

    private WandererController wanderer;

    void Start()
    {
        FindWanderer();
    }

    void Update()
    {
        if (wanderer == null)
        {
            // Retry finding the Wanderer in case it was loaded late
            FindWanderer();
        }

        if (wanderer != null)
        {
            UpdateRunePanel(wanderer.RuneFragments);
        }
    }

    private void FindWanderer()
    {
        wanderer = FindObjectOfType<WandererController>();

        if (wanderer == null)
        {
            Debug.LogWarning("WandererController not found in the scene. Retrying...");
        }
        else
        {
            Debug.Log("WandererController found successfully!");
        }
    }

    private void UpdateRunePanel(int runeCount)
    {
        // Clear existing icons
        foreach (Transform child in runePanel)
        {
            Destroy(child.gameObject);
        }

        // Add icons for each Rune Fragment
        for (int i = 0; i < runeCount; i++)
        {
            Instantiate(runeIconPrefab, runePanel);
        }
    }
}
