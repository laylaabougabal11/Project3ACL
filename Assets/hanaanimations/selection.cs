using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement; // For scene management

public class selection : MonoBehaviour
{
    public GameObject[] characters; // Array of character GameObjects
    public TMP_Text characterNameText; // Text to display character name
    public TMP_Text abilitiesText; // Text to display character abilities

    public string[] characterNames = { "Barbarian", "Rogue", "Sorcerer" }; // Character names
    public string[] characterAbilities = {
        "Strong melee attacks and high health.",
        "Fast and agile, excels in stealth.",
        "Powerful magic spells with ranged attacks."
    };

    private int currentIndex = 0; // Current character index

    void Start()
    {
        UpdateCharacterDisplay();
    }

    public void NextCharacter()
    {
        currentIndex = (currentIndex + 1) % characters.Length;
        UpdateCharacterDisplay();
    }

    public void PreviousCharacter()
    {
        currentIndex--;
        if (currentIndex < 0)
            currentIndex = characters.Length - 1;
        UpdateCharacterDisplay();
    }

    private void UpdateCharacterDisplay()
    {
        // Activate the current character and deactivate others
        for (int i = 0; i < characters.Length; i++)
        {
            characters[i].SetActive(i == currentIndex);
        }

        // Update UI text
        characterNameText.text = characterNames[currentIndex];
        abilitiesText.text = characterAbilities[currentIndex];
    }

    // Called when the 'Play' button is pressed
    public void PlayGame()
    {
        // Save the selected character index
        PlayerPrefs.SetInt("SelectedCharacterIndex", currentIndex);
        PlayerPrefs.Save();

        // Load the game scene
        SceneManager.LoadScene("MainScene"); // Replace with your game scene name
    }
}
