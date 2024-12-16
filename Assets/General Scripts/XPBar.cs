using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System; // Import TextMeshPro namespace

public class WandererXPBar : MonoBehaviour
{
    public Slider xpSlider; // Main slider for XP
    public Slider easeXpSlider; // Slider for smooth transition
    public TextMeshProUGUI xpText; // Text for XP display
    public TextMeshProUGUI levelText; // Reference to the text inside the slider

    private WandererController wanderer;
    public float lerpSpeed = 5f;

    void Start()
    {
        wanderer = FindObjectOfType<WandererController>();

        if (wanderer != null)
        {
            // Initialize the slider values
            xpSlider.maxValue = wanderer.currentLevel * 100;
            easeXpSlider.maxValue = wanderer.currentLevel * 100;
            xpSlider.value = wanderer.currentXP;
            easeXpSlider.value = wanderer.currentXP;

            UpdateXPText();
            UpdateLevelText();
        }
    }



    void Update()
    {
        if (wanderer != null)
        {
            // Update the XP slider
            xpSlider.value = wanderer.currentXP;

            // Smoothly update the ease XP slider only when XP increases
            if (easeXpSlider.value < xpSlider.value)
            {
                easeXpSlider.value = Mathf.Lerp(easeXpSlider.value, xpSlider.value, Time.deltaTime * lerpSpeed);
            }
            else
            {
                easeXpSlider.value = xpSlider.value; // Instant decrease
            }

            // Handle max level case
            if (wanderer.currentLevel >= wanderer.maxLevel)
            {
                xpSlider.value = xpSlider.maxValue;
                easeXpSlider.value = xpSlider.maxValue;
            }
            else
            {
                // Update max value dynamically based on level
                xpSlider.maxValue = wanderer.currentLevel * 100;
                easeXpSlider.maxValue = wanderer.currentLevel * 100;
            }

            // Update the XP text
            UpdateXPText();
            UpdateLevelText();
        }
    }

    private void UpdateLevelText()
    {
        if (levelText != null)
        {
            // Display the current level inside the slider
            levelText.text = $"Level: {wanderer.currentLevel}";
        }
    }

    private void UpdateXPText()
    {
        if (xpText != null)
        {
            // Format: "Current XP / Max XP"
            xpText.text = $"{wanderer.currentXP} XP";
        }
    }
}
