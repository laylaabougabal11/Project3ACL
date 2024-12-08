using UnityEngine;

public class HealingPotionController : MonoBehaviour
{
    private bool isCollected = false; // Prevents double collection

    private void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;

        if (other.CompareTag("Wanderer"))
        {
            CollectPotion(other.gameObject);
        }
    }

    private void CollectPotion(GameObject wanderer)
    {
        isCollected = true; // Mark as collected

        // Add potion to the Wanderer's inventory
        WandererController controller = wanderer.GetComponent<WandererController>();
        if (controller != null)
        {
            controller.AddHealingPotion();
            Debug.Log("Healing Potion collected!");
        }

        // Destroy or disable the potion object
        Destroy(gameObject);
    }
}
