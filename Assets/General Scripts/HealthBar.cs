using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class HealthBar : MonoBehaviour
{
    public Slider healthSlider;
    public Slider easeHealthSlider;
    private WandererController wanderer;
    public float lerpSpeed = 5f;

    // Start is called before the first frame update
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
        }
    }

    // Update is called once per frame
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
        }
    }

}

