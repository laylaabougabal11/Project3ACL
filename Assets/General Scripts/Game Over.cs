using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GameOver : MonoBehaviour
{
    // Start is called before the first frame update
    public void GoToMainMenu()
    {
        // Ensure the Main Menu scene is added to the build settings
        Debug.Log("Go to Main Menu");
        SceneManager.LoadScene("MainMenu"); // Replace "MainMenu" with the name of your main menu scene
        
    }
    public void RestartLevel()
    {
        SceneManager.LoadScene("BossScene");
    }
}
