using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGateController : MonoBehaviour
{
    public string bossSceneName = "BossScene"; // Name of the boss scene
    public int requiredRuneFragments = 3; // Number of Rune Fragments required
    private bool isGateUnlocked = false; // Tracks if the gate is unlocked

    private void OnTriggerEnter(Collider other)
    {
        // Check if the Wanderer entered the gate range
        if (other.CompareTag("Wanderer"))
        {
            WandererController wanderer = other.GetComponent<WandererController>();
            if (wanderer != null && wanderer.runeFragments >= requiredRuneFragments)
            {
                UnlockGate();
                TransitionToBossScene();
            }
            else
            {
                Debug.Log("Gate locked. Collect more Rune Fragments.");
            }
        }
    }

    private void UnlockGate()
    {
        if (!isGateUnlocked)
        {
            isGateUnlocked = true;
            Debug.Log("Gate unlocked!");
            // Add gate opening animation or sound effect here
        }
    }

    private void TransitionToBossScene()
    {
        Debug.Log("Loading Boss Scene...");
        SceneManager.LoadScene(bossSceneName);
    }
}
