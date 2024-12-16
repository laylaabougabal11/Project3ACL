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
        // Find the Wanderer in the scene
        wanderer = FindObjectOfType<WandererController>();

        if (wanderer == null)
        {
            Debug.LogError("WandererController not found in the scene!");
            //
        }
    }

    void Update()
    {
        if (wanderer != null)
        {
            UpdateRunePanel(wanderer.runeFragments);
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
