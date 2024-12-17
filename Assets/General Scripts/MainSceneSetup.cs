using UnityEngine;
using Cinemachine;

public class MainSceneSetup : MonoBehaviour
{
    public GameObject[] characterModels; // Array of disabled character GameObjects in MainScene
    private GameObject activeCharacter;

    public CinemachineVirtualCamera virtualCamera; // Reference to the Virtual Camera

    void Start()
    {
        // Ensure we have saved data for character selection
        if (!PlayerPrefs.HasKey("SelectedCharacterIndex"))
        {
            Debug.LogError("No character selection data found! Defaulting to the first character.");
            ActivateCharacter(0);
            return;
        }

        // Get the saved index from PlayerPrefs
        int selectedIndex = PlayerPrefs.GetInt("SelectedCharacterIndex");

        if (selectedIndex < 0 || selectedIndex >= characterModels.Length)
        {
            Debug.LogError("Invalid character index! Defaulting to the first character.");
            ActivateCharacter(0);
        }
        else
        {
            ActivateCharacter(selectedIndex);
        }
    }

    private void ActivateCharacter(int index)
    {
        // Disable all characters first
        for (int i = 0; i < characterModels.Length; i++)
        {
            characterModels[i].SetActive(false);
        }

        // Enable the selected character
        activeCharacter = characterModels[index];
        activeCharacter.SetActive(true);
        Debug.Log($"Activated character: {activeCharacter.name}");

        // Set the Virtual Camera to follow and look at the active character
        if (virtualCamera != null)
        {
            Transform playerTransform = activeCharacter.transform;
            virtualCamera.Follow = playerTransform;
            virtualCamera.LookAt = playerTransform;
            Debug.Log("Virtual Camera updated to follow the activated character.");
        }
        else
        {
            Debug.LogError("Virtual Camera reference is missing! Assign it in the inspector.");
        }
    }
}
