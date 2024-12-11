using UnityEngine;

public class RuneController : MonoBehaviour
{
    private bool isCollected = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;

        if (other.CompareTag("Wanderer"))
        {
            CollectRune(other.gameObject);
        }
    }

    private void CollectRune(GameObject wanderer)
    {
        isCollected = true; // Prevent multiple pickups
        Debug.Log("Rune collected!");

        // Add Rune to the Wanderer's inventory
        WandererController controller = wanderer.GetComponent<WandererController>();
        if (controller != null)
        {
            controller.AddRuneFragment();
        }

        // Destroy or disable the Rune
        Destroy(gameObject);
    }
}
