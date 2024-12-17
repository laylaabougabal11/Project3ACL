using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelection : MonoBehaviour
{
    public GameObject[] mainSceneCharacters; // Array of characters already in the scene
    private int currentIndex = 0; // Currently selected character index

    void Start()
    {
        UpdateSelectionDisplay();
    }

    public void NextCharacter()
    {
        currentIndex = (currentIndex + 1) % mainSceneCharacters.Length;
        UpdateSelectionDisplay();
    }

    public void PreviousCharacter()
    {
        currentIndex--;
        if (currentIndex < 0)
            currentIndex = mainSceneCharacters.Length - 1;
        UpdateSelectionDisplay();
    }

    private void UpdateSelectionDisplay()
    {
        for (int i = 0; i < mainSceneCharacters.Length; i++)
        {
            mainSceneCharacters[i].SetActive(i == currentIndex);
        }

        Debug.Log($"Selected Character: {mainSceneCharacters[currentIndex].name}");
    }

    public void ConfirmSelection()
    {
        // Ensure only the selected character is active
        for (int i = 0; i < mainSceneCharacters.Length; i++)
        {
            if (i == currentIndex)
            {
                mainSceneCharacters[i].SetActive(true);
                DontDestroyOnLoad(mainSceneCharacters[i]); // Persist across scenes
            }
            else
            {
                mainSceneCharacters[i].SetActive(false);
            }
        }

        Debug.Log($"Character confirmed: {mainSceneCharacters[currentIndex].name}");

        // Save the selected index to PlayerPrefs (optional)
        PlayerPrefs.SetInt("SelectedCharacterIndex", currentIndex);
        PlayerPrefs.Save();

        // Load the Main Scene
        SceneManager.LoadScene("MainScene");
    }
}
