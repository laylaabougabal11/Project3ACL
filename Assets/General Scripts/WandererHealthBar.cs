using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Required for TextMeshPro

public class WandererHealthBar : MonoBehaviour
{
    public Slider healthSlider;
    public Slider easeHealthSlider;
    public TextMeshProUGUI healthText; // Reference to the TextMeshProUGUI component
    private WandererController wanderer;
    public float lerpSpeed = 5f;

    void Start()
    {
        wanderer = FindObjectOfType<WandererController>();

        if (wanderer != null)
        {
            // Initialize the slider values
            healthSlider.maxValue = wanderer.maxHealth;
            easeHealthSlider.maxValue = wanderer.maxHealth;
            healthSlider.value = wanderer.currentHealth;
            easeHealthSlider.value = wanderer.currentHealth;

            UpdateHealthText(); // Update the text initially
        }
    }

    void Update()
    {
        if (wanderer != null)
        {
            // Update the health slider
            healthSlider.value = wanderer.currentHealth;

            // Smoothly update the ease health slider
            if (healthSlider.value != easeHealthSlider.value)
            {
                easeHealthSlider.value = Mathf.Lerp(easeHealthSlider.value, wanderer.currentHealth, Time.deltaTime * lerpSpeed);
            }

            // Update the health text
            UpdateHealthText();
        }
    }

    public void UpdateHealthBar(int newMaxHealth, int newCurrentHealth)
    {
        // Update the max value and current health dynamically
        healthSlider.maxValue = newMaxHealth;
        easeHealthSlider.maxValue = newMaxHealth;
        healthSlider.value = newCurrentHealth;
        easeHealthSlider.value = newCurrentHealth;

        UpdateHealthText(); // Update the health text
    }

    private void UpdateHealthText()
    {
        if (healthText != null)
        {
            healthText.text = $"{wanderer.currentHealth} HP"; // Format: Current / Max HP
        }
    }
}
