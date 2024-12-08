using UnityEngine;

public class EnemyCampController : MonoBehaviour
{
    public GameObject rune; // The Rune in the camp
    public GameObject[] healingPotions; // Healing Potions in the camp

    private bool isCleared = false; // Tracks if the camp is cleared

    void Start()
    {
        // Initially disable the Rune
        if (rune != null)
        {
            rune.SetActive(false);
        }

        // Optionally randomize potion placements if desired
        foreach (GameObject potion in healingPotions)
        {
            potion.SetActive(true);
        }
    }

    public void ActivateRune()
    {
        if (rune != null)
        {
            rune.SetActive(true);
            Debug.Log("Rune activated!");
        }
    }

    public void ClearCamp()
    {
        isCleared = true;
        Debug.Log("Camp cleared!");
        ActivateRune();
    }
}
