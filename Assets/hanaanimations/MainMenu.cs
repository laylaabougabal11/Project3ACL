using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;



public class MainMenu : MonoBehaviour
{
    public GameObject ChooseLevelPanel;
    public GameObject MainMenuPanel;
    // [Header("Audio Sources")]
    // public AudioSource musicSource;    // The audio source for background music
    // public AudioSource effectsSource;  // The audio source for sound effects

    // [Header("UI Sliders")]
    // public Slider musicSlider;         // Slider to control music volume
    // public Slider effectsSlider;


    private void Start()
    {
        // Load saved audio levels or set defaults
        // musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        // effectsSlider.value = PlayerPrefs.GetFloat("EffectsVolume", 0.5f);

        // UpdateMusicVolume(musicSlider.value);
        // UpdateEffectsVolume(effectsSlider.value);
    }

    public void Options()
    {
        SceneManager.LoadScene("Options");
    }
    public void StartGame()
    {
        SceneManager.LoadScene("Characterselection");
    }
    public void Quit()
    {
        Application.Quit();
    }
    public void ChooseLevel()
    {
        MainMenuPanel.SetActive(false);
        ChooseLevelPanel.SetActive(true);
    }

    public void back()
    {
        MainMenuPanel.SetActive(true);
        ChooseLevelPanel.SetActive(false);
    }
    public void back2()
    {

        SceneManager.LoadScene("MainMenu");
    }

    public void StartGame2()
    {

        SceneManager.LoadScene("MainScene");
    }

    // public void UpdateMusicVolume(float volume)
    // {
    //     musicSource.volume = volume;
    //     PlayerPrefs.SetFloat("MusicVolume", volume);
    // }

    // public void UpdateEffectsVolume(float volume)
    // {
    //     effectsSource.volume = volume;
    //     PlayerPrefs.SetFloat("EffectsVolume", volume);
    // }
}
