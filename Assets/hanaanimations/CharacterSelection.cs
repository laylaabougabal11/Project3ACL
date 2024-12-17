using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelection : MonoBehaviour
{
    public GameObject[] selectionModels; // Array of models shown in the selection scene
    public GameObject[] mainSceneCharacters; // Array of character prefabs for the main scene (assigned in inspector)

    private int currentIndex = 0; // Currently selected character index
    private GameObject selectedCharacterInstance = null; // Holds the enabled character

    void Start()
    {
        UpdateSelectionDisplay();
    }

    public void NextCharacter()
    {
        currentIndex = (currentIndex + 1) % selectionModels.Length;
        UpdateSelectionDisplay();
    }

    public void PreviousCharacter()
    {
        currentIndex--;
        if (currentIndex < 0) currentIndex = selectionModels.Length - 1;
        UpdateSelectionDisplay();
    }

    private void UpdateSelectionDisplay()
    {
        for (int i = 0; i < selectionModels.Length; i++)
        {
            selectionModels[i].SetActive(i == currentIndex);
        }
    }

    public void ConfirmSelection()
    {
        // Ensure the previous character instance is removed (if any)
        if (selectedCharacterInstance != null)
        {
            Destroy(selectedCharacterInstance);
        }

        // Instantiate the chosen character prefab and mark it as DontDestroyOnLoad
        selectedCharacterInstance = Instantiate(mainSceneCharacters[currentIndex]);
        DontDestroyOnLoad(selectedCharacterInstance);

        Debug.Log($"Character selected: {mainSceneCharacters[currentIndex].name}");

        // Save the selected index to PlayerPrefs (optional)
        PlayerPrefs.SetInt("SelectedCharacterIndex", currentIndex);
        PlayerPrefs.Save();

        // Load the Main Scene
        SceneManager.LoadScene("MainScene");
    }
}
