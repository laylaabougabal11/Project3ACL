using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    public Slider healthSlider;
    public Slider easeHealthSlider;
    public float lerpSpeed = 5f;

    private IHealth target; // Reference to the health system

    void Start()
    {
        // Search for a parent with the Wanderer or Minion tag
        Transform parent = FindParentWithTags(transform, new string[] { "Wanderer", "Minion" });

        if (parent != null)
        {
            target = parent.GetComponent<IHealth>();
        }

        if (target != null)
        {
            // Initialize the health bar values
            healthSlider.maxValue = target.MaxHealth;
            easeHealthSlider.maxValue = target.MaxHealth;
            healthSlider.value = target.CurrentHealth;
            easeHealthSlider.value = target.CurrentHealth;
        }
        else
        {
            Debug.LogError($"No IHealth component found on a parent with tag Wanderer or Minion for {gameObject.name}");
        }
    }

    void Update()
    {
        if (target != null)
        {
            // Update health bar value
            healthSlider.value = target.CurrentHealth;

            // Smoothly update the eased health slider
            if (healthSlider.value != easeHealthSlider.value)
            {
                easeHealthSlider.value = Mathf.Lerp(easeHealthSlider.value, target.CurrentHealth, Time.deltaTime * lerpSpeed);
            }
        }
    }

    private Transform FindParentWithTags(Transform child, string[] tags)
    {
        Transform current = child;

        while (current != null)
        {
            foreach (string tag in tags)
            {
                if (current.CompareTag(tag))
                {
                    return current;
                }
            }
            current = current.parent;
        }

        return null;
    }
}
