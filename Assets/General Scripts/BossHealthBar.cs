using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : MonoBehaviour
{
    public Slider healthSlider;         // Main slider for the Boss's HP
    public Slider easeHealthSlider;    // Slider for smooth transition
    public float lerpSpeed = 5f;       // Speed for easing the health bar

    private BossLilithController boss; // Reference to the single Boss Controller

    void Start()
    {
        // Directly find the single boss in the scene
        boss = FindObjectOfType<BossLilithController>();

        if (boss != null)
        {
            // Initialize slider values with Boss's max health
            healthSlider.maxValue = boss.MaxHealth;
            easeHealthSlider.maxValue = boss.MaxHealth;
            healthSlider.value = boss.CurrentHealth;
            easeHealthSlider.value = boss.CurrentHealth;
        }
        else
        {
            Debug.LogError("BossLilithController not found in the scene. Ensure the boss is present and active.");
        }
    }

    void Update()
    {
        if (boss != null)
        {
            // Update the health bar to reflect the current health
            healthSlider.value = boss.CurrentHealth;

            // Smoothly animate the eased health slider
            if (Mathf.Abs(healthSlider.value - easeHealthSlider.value) > 0.01f)
            {
                easeHealthSlider.value = Mathf.Lerp(easeHealthSlider.value, boss.CurrentHealth, Time.deltaTime * lerpSpeed);
            }
        }
    }
}
