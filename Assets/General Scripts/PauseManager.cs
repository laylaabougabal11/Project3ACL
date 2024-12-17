using UnityEngine;
using UnityEngine.SceneManagement; // For restarting or quitting
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pausePanel; // Reference to the Pause Panel
    public GameObject bossHealthBar; // Reference to the Boss Health Bar (Boss Scene only)
    public GameObject healthBar; // Reference to the Health Bar
    public GameObject xpBar; // Reference to the XP Bar
    public GameObject abilitiesPanel; // Reference to the Abilities Panel
    public GameObject potionPanel; // Reference to the Potion Panel
    public GameObject runePanel; // Reference to the Rune Panel (Main Scene only)
    public GameObject winPanel; // Reference to the Win Panel
    public GameObject losePanel; // Reference to the Lose Panel

    private bool isPaused = false;

    void Start()
    {
        // Ensure the pause panel is hidden at the start
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
    }

    void Update()
    {
        // Toggle pause when Escape key is pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; // Stop time in the game

        // Activate Pause Panel
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        // Disable all other UI panels
        TogglePanels(false);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; // Resume time

        // Deactivate Pause Panel
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        // Re-enable all other UI panels
        TogglePanels(true);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // Reset time before reloading
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // Reload current scene
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }

    private void TogglePanels(bool state)
    {
        // Disable/Enable only the relevant panels
        if (healthBar != null) healthBar.SetActive(state);
        if (xpBar != null) xpBar.SetActive(state);
        if (abilitiesPanel != null) abilitiesPanel.SetActive(state);
        if (potionPanel != null) potionPanel.SetActive(state);

        // Check the current scene for conditional panels
        if (SceneManager.GetActiveScene().name == "BossScene")
        {
            if (bossHealthBar != null) bossHealthBar.SetActive(state);
        }
        else if (SceneManager.GetActiveScene().name == "MainScene")
        {
            if (runePanel != null) runePanel.SetActive(state);
        }

        // Win and Lose panels are ignored because they override everything else
        if (winPanel != null && winPanel.activeSelf) winPanel.SetActive(false);
        if (losePanel != null && losePanel.activeSelf) losePanel.SetActive(false);
    }
}
